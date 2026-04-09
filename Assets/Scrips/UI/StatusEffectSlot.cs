using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StatusEffectSlot : MonoBehaviour
{
	// 상태이상 아이콘 프리팹
	public GameObject statusEffectIconPrefab;

	// 고정 표시 영역(월드 단위)
	[Header("레이아웃 영역 설정")]
	public Vector2 areaSize = new Vector2(2.0f, 1f);   // 전체 영역 크기 (w×h)
	public Vector2 areaPadding = new Vector2(0.05f, 0.05f); // 내부 패딩
	public Vector2 minIconSpacing = new Vector2(0.1f, 0.15f); // 아이콘 간 최소 간격 (x: 가로, y: 세로/위아래, 겹침 방지, 호버 영역 확보)
	public bool useEllipsisOnOverflow = true;

	// 기존 옵션(참고용): world-space 고정 간격 배치 파라미터
	[Header("레거시 간격 옵션(비활성 시 무시)")]
	public Vector2 cellSize = new Vector2(0.5f, 0.5f); // 각 셀의 크기(간격)
	public int maxPerRow = 5; // 한 줄에 5개
	public int maxRowsLegacy = 2;   // 2줄(레거시)
	public GameObject moreIconPrefab; // ...아이콘 프리팹

	// 현재 슬롯에 표시 중인 상태이상 아이콘 리스트
	[SerializeField] private List<StatusEffectInstance> activeInstances = new List<StatusEffectInstance>();
	
	// 하위 오브젝트 관리용 GameObject 리스트
	[Header("하위 오브젝트 관리")]
	[SerializeField] private List<GameObject> childObjectList = new List<GameObject>();
	
	private GameObject moreIconInstance;


	[Header("상태이상 정산 이펙트")]
	[SerializeField] private GameObject statusEffectSprite;
	[SerializeField] private SpriteRenderer iconRenderer;
	[SerializeField] private TextMesh damageText;
	[SerializeField] private float settleEffectInterval = 0.02f; // 상태이상 연출 간 간격 (턴 템포 보호)
	[SerializeField] private int maxBlockingStatusEffectAnimations = 1; // 턴 지연 방지를 위해 블로킹 연출 개수 제한

	// 자식 변동 시 자동으로 재정렬 (중첩 방지)
	private void OnTransformChildrenChanged()
	{
		// StatusEffectSlot이 비활성화되어 있으면 코루틴 시작하지 않음
		if (!gameObject.activeInHierarchy)
			return;
			
		// 즉시 업데이트 시도
		RebuildCurrentChildren();
		
		// 다음 프레임에도 확인 (에디터 복제 시 타이밍 문제 대응)
		StartCoroutine(DelayedRebuildCheck());
	}
	
	/// <summary>
	/// 다음 프레임에 레이아웃 재확인 (에디터 복제 대응)
	/// </summary>
	private IEnumerator DelayedRebuildCheck()
	{
		yield return null; // 다음 프레임 대기
		RebuildCurrentChildren();
	}

	/// <summary>
	/// 현재 자식들의 상태이상 프리팹을 기준으로 그리드 재정렬 (영역 내 스케일 조정 포함)
	/// </summary>
	public void RebuildCurrentChildren()
	{
		UpdateLayoutAndSize();
	}
	
	/// <summary>
	/// 레이아웃 및 크기 업데이트 (개수 체크 > 크기 책정 > 반영)
	/// </summary>
	private void UpdateLayoutAndSize()
	{
		// 기존 moreIcon 제거
		if (moreIconInstance != null)
		{
			Destroy(moreIconInstance);
			moreIconInstance = null;
		}

		// 하위 오브젝트 리스트 업데이트
		childObjectList.Clear();
		
		// 표시 대상 수집 (StatusEffect 계열만)
		List<Transform> effects = new List<Transform>();
		for (int i = 0; i < transform.childCount; i++)
		{
			var child = transform.GetChild(i);
			if (child == null) continue;
			if (child.GetComponent<StatusEffectInstance>() != null ||
				child.GetComponent("StatusEffectInstanceStun") != null ||
				child.GetComponent<StatusEffectInstanceBuff>() != null ||
				child.GetComponent<StatusEffectInstanceReaction>() != null)
			{
				effects.Add(child);
				childObjectList.Add(child.gameObject);
			}
		}

		// 개수 체크 > 크기 책정 > 반영
		LayoutWithSizeManagement(effects);
	}

	/// <summary>
	/// 레이아웃 및 크기 관리 (고정 크기 적용)
	/// </summary>
	private void LayoutWithSizeManagement(List<Transform> items)
	{
		// 1. 개수 체크 (childObjectList 기준)
		int count = childObjectList.Count;
		
		// null이거나 개수가 0이면 종료
		if (count == 0 || items == null || items.Count == 0)
		{
			HandleEllipsis(0, 0, Vector3.zero, 1f, 1f, 1);
			return;
		}
		
		// 2. 사용 가능한 영역 계산
		float usableW = Mathf.Max(0.01f, areaSize.x - areaPadding.x * 2f);
		float usableH = Mathf.Max(0.01f, areaSize.y - areaPadding.y * 2f);

		// 3. 행/열 결정 (가로부터 채우기, 최대 2줄)
		int rows = 1;
		int cols = count;
		
		// 2줄이 필요한지 확인 (간단한 기준: 5개 이상이면 2줄)
		if (count > 4)
		{
			rows = 2;
			cols = Mathf.CeilToInt(count / 2f);
		}

		// 4. 셀 크기 계산 (최소 간격 보장)
		float cellW = usableW / Mathf.Max(1, cols);
		float cellH = usableH / Mathf.Max(1, rows);
		
		// 최소 간격 보장: 셀 크기가 최소 간격보다 작으면 조정
		if (cellW < minIconSpacing.x)
		{
			cellW = minIconSpacing.x;
			// 셀 크기 조정에 따라 사용 가능한 너비 재계산
			float totalNeededW = cellW * cols;
			if (totalNeededW > usableW)
			{
				// 필요한 공간이 부족하면 간격을 줄이되 최소 간격은 유지
				cellW = Mathf.Max(minIconSpacing.x, usableW / cols);
			}
		}
		if (cellH < minIconSpacing.y)
		{
			cellH = minIconSpacing.y;
			float totalNeededH = cellH * rows;
			if (totalNeededH > usableH)
			{
				cellH = Mathf.Max(minIconSpacing.y, usableH / rows);
			}
		}

		// 5. 배치 (크기 조정 없음, 원본 프리팹 크기 유지, 최소 간격 보장)
		Vector3 topLeft = new Vector3(-usableW * 0.5f, usableH * 0.5f, 0f);
		int displayCap = rows * cols;

		// 위치 배치만 수행 (크기는 원본 프리팹 크기 그대로 유지, 최소 간격 보장)
		for (int i = 0; i < items.Count; i++)
		{
			Transform tr = items[i];
			bool visible = (i < displayCap);
			
			if (!visible)
			{
				if (tr.gameObject.activeSelf)
					tr.gameObject.SetActive(false);
				continue;
			}

			// 위치 계산 (가로부터 채우기, 최소 간격 보장)
			int row = i / cols;
			int col = i % cols;
			Vector3 cellCenter = topLeft + new Vector3(col * cellW + cellW * 0.5f, -(row * cellH + cellH * 0.5f), 0f);
			float zOffset = row * 0.0005f;
			tr.localPosition = new Vector3(cellCenter.x, cellCenter.y, tr.localPosition.z + zOffset);

			// 크기는 건드리지 않음 (원본 프리팹 크기 유지)

			if (!tr.gameObject.activeSelf)
				tr.gameObject.SetActive(true);
		}

		HandleEllipsis(count, displayCap, topLeft, cellW, cellH, cols);
	}

	/// <summary>
	/// 상태이상 추가
	/// </summary>
	public void AddStatusEffect(StatusEffectData effectData, int duration, int value, CharacterStats owner)
	{
		GameObject icon = Instantiate(statusEffectIconPrefab, transform);
		
		// 프리팹의 원본 크기 그대로 사용 (스케일 변경 없음)
		
		var instance = icon.GetComponent<StatusEffectInstance>();
		if (instance != null)
		{
			instance.Initialize(effectData, duration, value, owner);
			activeInstances.Add(instance);
			
			// 하위 오브젝트 리스트에 추가
			childObjectList.Add(icon);
			
			// 정렬 및 크기 조정 메서드 호출
			UpdateLayoutAndSize();
		}
		else
		{
			Debug.LogWarning("StatusEffectInstance 컴포넌트가 프리팹에 없습니다.");
		}
	}

	/// <summary>
	/// 상태이상 제거
	/// </summary>
	public void RemoveStatusEffect(StatusEffectData effectData)
	{
		for (int i = activeInstances.Count - 1; i >= 0; i--)
		{
			if (activeInstances[i].EffectData == effectData)
			{
				GameObject obj = activeInstances[i].gameObject;
				
				// 하위 오브젝트 리스트에서 제거
				childObjectList.Remove(obj);
				
				Destroy(obj);
				activeInstances.RemoveAt(i);
			}
		}
		
		// 정렬 및 크기 조정 메서드 호출
		UpdateLayoutAndSize();
	}

	/// <summary>
	/// 모든 상태이상 초기화
	/// </summary>
	public void ClearAll()
	{
		foreach (var instance in activeInstances)
			Destroy(instance.gameObject);
		activeInstances.Clear();
		childObjectList.Clear();
		UpdateLayoutAndSize();
	}

	/// <summary>
	/// 영역 내 간단한 그리드 배치 (가로부터 채우기, 최대 2줄, 크기 관리 없음)
	/// </summary>
	private void LayoutWithinArea(List<Transform> items)
	{
		if (items == null || items.Count == 0)
		{
			HandleEllipsis(0, 0, Vector3.zero, 1f, 1f, 1);
			return;
		}

		int count = items.Count;
		
		// 1. 사용 가능한 영역 계산
		float usableW = Mathf.Max(0.01f, areaSize.x - areaPadding.x * 2f);
		float usableH = Mathf.Max(0.01f, areaSize.y - areaPadding.y * 2f);

		// 2. 행/열 결정 (가로부터 채우기, 최대 2줄)
		int rows = 1;
		int cols = count;
		
		// 2줄이 필요한지 확인 (간단한 기준: 5개 이상이면 2줄)
		if (count > 4)
		{
			rows = 2;
			cols = Mathf.CeilToInt(count / 2f);
		}

		// 3. 셀 크기 계산
		float cellW = usableW / Mathf.Max(1, cols);
		float cellH = usableH / Mathf.Max(1, rows);

		// 4. 배치 (크기 조정 없음, 위치만 정렬)
		Vector3 topLeft = new Vector3(-usableW * 0.5f, usableH * 0.5f, 0f);
		int displayCap = rows * cols;

		for (int i = 0; i < items.Count; i++)
		{
			Transform tr = items[i];
			bool visible = (i < displayCap);
			
			if (!visible)
			{
				if (tr.gameObject.activeSelf)
					tr.gameObject.SetActive(false);
				continue;
			}

			// 위치 계산 (가로부터 채우기)
			int row = i / cols;
			int col = i % cols;
			Vector3 cellCenter = topLeft + new Vector3(col * cellW + cellW * 0.5f, -(row * cellH + cellH * 0.5f), 0f);
			float zOffset = row * 0.0005f;
			tr.localPosition = new Vector3(cellCenter.x, cellCenter.y, tr.localPosition.z + zOffset);

			// 스케일은 건드리지 않음 (프리팹 원본 크기 유지)

			if (!tr.gameObject.activeSelf)
				tr.gameObject.SetActive(true);
		}

		HandleEllipsis(count, displayCap, topLeft, cellW, cellH, cols);
	}

	private void HandleEllipsis(int count, int displayCap, Vector3 topLeft, float cellW, float cellH, int cols)
	{
		if (moreIconInstance != null)
		{
			Destroy(moreIconInstance);
			moreIconInstance = null;
		}

		if (!useEllipsisOnOverflow) return;
		if (count <= displayCap || moreIconPrefab == null) return;

		int idx = Mathf.Max(0, displayCap - 1);
		int row = idx / Mathf.Max(1, cols);
		int col = idx % Mathf.Max(1, cols);
		Vector3 pos = topLeft + new Vector3(col * cellW + cellW * 0.5f, -(row * cellH + cellH * 0.5f), 0f);
		moreIconInstance = Instantiate(moreIconPrefab, transform);
		moreIconInstance.transform.localPosition = pos;
	}

	/// <summary>
	/// 원본 스프라이트 크기 가져오기 (스케일 미적용)
	/// </summary>
	private Vector2 GetSpriteSizeWorld(Transform tr, bool useCurrentScale = false)
	{
		SpriteRenderer sr = tr.GetComponentInChildren<SpriteRenderer>();
		if (sr != null && sr.sprite != null)
		{
			if (useCurrentScale)
			{
				// 현재 스케일이 적용된 크기
				return sr.bounds.size;
			}
			else
			{
				// 원본 스프라이트 크기 (스케일 1일 때의 크기)
				return sr.sprite.bounds.size;
			}
		}
		// 기본값
		return new Vector2(0.5f, 0.5f);
	}

	/// <summary>
	/// 레거시: 고정 셀 간격 배치. 내부적으로는 크기 관리 레이아웃을 호출.
	/// </summary>
	private void UpdateGridLayout()
	{
		UpdateLayoutAndSize();
	}

	/// <summary>
	/// 상태이상 정산 + 연출을 함께 처리하는 메서드
	/// </summary>
	public IEnumerator SettleStatusEffectsWithAnimation(CharacterStats character)
	{
		if (character == null)
			yield break;
			
		// StatusEffectSlot이 비활성화되어 있으면 즉시 종료
		if (!gameObject.activeInHierarchy)
		{
			Debug.LogWarning($"[StatusEffectSlot] SettleStatusEffectsWithAnimation: StatusEffectSlot이 비활성화되어 있어 종료합니다.");
			yield break;
		}

		// 1. 상태이상 정산 (기존 로직)
		var controller = character.GetComponent<StatusEffectController>();
		if (controller != null)
		{
			controller.ApplyStatusEffectsOnTurnStart();
		}

		// 2. BattleUIManager의 상태이상 피해 팝업 처리가 완료될 때까지 대기
		if (BattleUIManager.Instance != null)
		{
			yield return BattleUIManager.Instance.WaitForStatusEffectPopupsToComplete();
		}
		
		// 대기 중에 StatusEffectSlot이 비활성화되었는지 다시 확인
		if (!gameObject.activeInHierarchy)
		{
			Debug.LogWarning($"[StatusEffectSlot] SettleStatusEffectsWithAnimation: 대기 중 StatusEffectSlot이 비활성화되어 종료합니다.");
			yield break;
		}

		// 3. 연출용 스프라이트 초기화
		InitializeStatusEffectSprite();

		// 4. 상태이상 연출 실행
		var statusEffects = controller?.GetActiveEffectPrefabs();
		if (statusEffects != null)
		{
			int shown = 0;
			foreach (var effect in statusEffects)
			{
				if (effect != null)
				{
					// 매 루프마다 활성화 상태 확인
					if (!gameObject.activeInHierarchy)
					{
						Debug.LogWarning($"[StatusEffectSlot] SettleStatusEffectsWithAnimation: 연출 중 StatusEffectSlot이 비활성화되어 종료합니다.");
						yield break;
					}

					if (shown >= Mathf.Max(0, maxBlockingStatusEffectAnimations))
						break;
					
					yield return StartCoroutine(PlayStatusEffectSpriteAnimation(effect));
					shown++;
					if (settleEffectInterval > 0f)
						yield return new WaitForSeconds(settleEffectInterval);
				}
			}
		}

		// 5. 최종 정리
		FinalizeStatusEffectSprite();
		
		// 6. 모든 상태이상 피해 팝업 애니메이션 완료 후 사망 처리
		if (character != null && character.gameObject != null && character.Hp <= 0 && !character.IsDead)
		{
			character.Deathcheck(allowAllyCollapseDiceRoll: true);
			character.DeathAction();
		}
	}

	/// <summary>
	/// 연출용 스프라이트 오브젝트 찾기
	/// </summary>
	private void FindStatusEffectSprite()
	{
		if (statusEffectSprite != null) return;

		// StatusEffectSlot 내부의 StatusEffectSprite 찾기
		statusEffectSprite = transform.GetChild(0)?.gameObject;
		
		if (statusEffectSprite == null)
		{
			Debug.LogWarning("[StatusEffectSlot] StatusEffectSprite를 찾을 수 없습니다.");
			return;
		}

		// 컴포넌트 참조 설정
		iconRenderer = statusEffectSprite.GetComponent<SpriteRenderer>();
		damageText = statusEffectSprite.transform.Find("DamageText")?.GetComponent<TextMesh>();
	}

	/// <summary>
	/// 연출 시작 전 스프라이트 초기화
	/// </summary>
	private void InitializeStatusEffectSprite()
	{
		if (statusEffectSprite == null)
			FindStatusEffectSprite();

		if (statusEffectSprite == null) return;

		// 연출용 스프라이트 오브젝트 활성화
		statusEffectSprite.SetActive(true);

		// 스프라이트 null로 초기화 (이전 연출 잔재 제거)
		if (iconRenderer != null)
		{
			iconRenderer.sprite = null;
			Color color = iconRenderer.color;
			color.a = 1f;
			iconRenderer.color = color;
		}

		if (damageText != null)
		{
			damageText.text = "";
		}

		// 위치 초기화
		statusEffectSprite.transform.localPosition = Vector3.zero;
	}

	/// <summary>
	/// 개별 상태이상 연출 실행
	/// </summary>
	private IEnumerator PlayStatusEffectSpriteAnimation(GameObject statusEffect)
	{
		// 1. 상태이상 정보 설정
		SetupStatusEffectSprite(statusEffect);

		// 2. 애니메이션 실행
		yield return StartCoroutine(AnimateStatusEffectSprite());

		// 3. 연출 완료 후 스프라이트 정리
		ClearStatusEffectSprite();
	}

	/// <summary>
	/// 상태이상 스프라이트 설정
	/// </summary>
	private void SetupStatusEffectSprite(GameObject statusEffect)
	{
		var statusEffectInstance = statusEffect.GetComponent<StatusEffectInstance>();
		if (statusEffectInstance != null && statusEffectInstance.EffectData != null)
		{
			// 아이콘 스프라이트 설정
			if (iconRenderer != null)
			{
				iconRenderer.sprite = statusEffectInstance.EffectData.icon;
			}

			// 피해 수치 텍스트 설정
			if (damageText != null)
			{
				damageText.text = statusEffectInstance.value.ToString();
			}
		}
	}

	/// <summary>
	/// 상태이상 스프라이트 정리
	/// </summary>
	private void ClearStatusEffectSprite()
	{
		// 스프라이트 null로 정리
		if (iconRenderer != null)
		{
			iconRenderer.sprite = null;
		}

		if (damageText != null)
		{
			damageText.text = "";
		}

		// 위치 초기화
		statusEffectSprite.transform.localPosition = Vector3.zero;

		// 색상 초기화
		if (iconRenderer != null)
		{
			Color color = iconRenderer.color;
			color.a = 1f;
			iconRenderer.color = color;
		}
	}

	/// <summary>
	/// 상태이상 스프라이트 애니메이션
	/// </summary>
	private IEnumerator AnimateStatusEffectSprite()
	{
		var startPos = statusEffectSprite.transform.localPosition;
		var targetPos = startPos + Vector3.up * 2f; // 위로 2유닛 이동

		float duration = 0.22f;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			float t = elapsed / duration;

			// 위치 애니메이션
			statusEffectSprite.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);

			// 페이드아웃 효과
			if (iconRenderer != null)
			{
				Color color = iconRenderer.color;
				color.a = Mathf.Lerp(1f, 0f, t);
				iconRenderer.color = color;
			}

			elapsed += Time.deltaTime;
			yield return null;
		}
	}

	/// <summary>
	/// 최종 정리
	/// </summary>
	private void FinalizeStatusEffectSprite()
	{
		if (statusEffectSprite != null)
		{
			// 모든 연출 완료 후 최종 정리
			statusEffectSprite.SetActive(false);

			// 스프라이트 null로 정리
			if (iconRenderer != null)
			{
				iconRenderer.sprite = null;
			}

			if (damageText != null)
			{
				damageText.text = "";
			}

			// 위치 초기화
			statusEffectSprite.transform.localPosition = Vector3.zero;

			// 색상 초기화
			if (iconRenderer != null)
			{
				Color color = iconRenderer.color;
				color.a = 1f;
				iconRenderer.color = color;
			}
		}
	}
}
