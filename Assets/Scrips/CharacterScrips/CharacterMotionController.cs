using UnityEngine;
using System.Collections;

public class CharacterMotionController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CharacterStats characterStats;
    private string currentMotion;  // 현재 모션을 저장할 필드 추가

    /// <summary>인스펙터 미할당 시 부모·루트에서 CharacterStats를 찾는다(넉다운/기절 자세 적용 누락 방지).</summary>
    private CharacterStats ResolveCharacterStats()
    {
        if (characterStats != null) return characterStats;
        characterStats = GetComponent<CharacterStats>();
        if (characterStats == null)
            characterStats = GetComponentInParent<CharacterStats>(true);
        return characterStats;
    }

    // 전투 위치 관련 변수
    private Vector3 originalPosition = Vector3.zero;  // 원래 위치 (0,0,0)
    [SerializeField] private Transform battlePoint;    // 전투 위치 오브젝트
    private bool isInBattlePosition;   // 전투 위치 여부
    public bool IsInBattlePosition => isInBattlePosition;   // 전투 위치 여부 (읽기 전용 프로퍼티)

    private void Start()
    {
        Debug.Log($"[MotionController] 내 태그: {gameObject.tag}");
        string pointName = "BattleCoordinatePlayer";
        if (gameObject.CompareTag("Enemy"))
        {
            pointName = "BattleCoordinateEnemy";
        }
        GameObject pointObj = GameObject.Find(pointName);
        battlePoint = pointObj != null ? pointObj.transform : null;

        Debug.Log($"[MotionController] 할당된 BattlePoint: {battlePoint?.name}");
        if (battlePoint == null)
        {
            Debug.LogWarning($"[MotionController] 이름 {pointName}을(를) 가진 오브젝트를 찾을 수 없습니다.");
        }
        else
        {
            Debug.Log($"[MotionController] 전투 위치 오브젝트로 설정 완료: {pointName}");
        }

        // 스프라이트 오브젝트의 초기 위치를 0,0,0으로 설정
        transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// 전투 위치로 이동합니다.
    /// </summary>
    public void MoveToBattlePosition()
    {
        if (!IsInBattlePosition && battlePoint != null)
        {
            // 월드 좌표로 전투 위치 설정
            transform.position = battlePoint.position;
            isInBattlePosition = true;
            Debug.Log($"[MotionController] 전투 위치로 이동: {battlePoint.name}, 위치: {transform.position}");
        }
        else if (battlePoint == null)
        {
            Debug.LogWarning("[MotionController] 전투 위치 오브젝트가 설정되지 않았습니다.");
        }
        else
        {
            Debug.Log($"[MotionController] 이미 전투 위치에 있음: {gameObject.name}");
        }
    }

    /// <summary>
    /// 지정된 위치로 이동합니다.
    /// </summary>
    /// <param name="position">이동할 위치</param>
    public void MoveToPosition(Vector3 position)
    {
        transform.position = position;
        isInBattlePosition = true; // 전투 중으로 간주
        Debug.Log($"[MotionController] 지정 위치로 이동: {gameObject.name}, 위치: {position}");
    }

    /// <summary>
    /// 스킬 모션을 실행합니다.
    /// </summary>
    /// <param name="motionType">모션 타입</param>
    public void PlaySkillMotion(string motionType)
    {
        Debug.Log($"[MotionController] PlaySkillMotion 호출: {motionType}");
        if (!characterStats.IsActive) return;

        currentMotion = motionType;
        
        // 1. 먼저 캐릭터별 고급 모션 데이터 확인
        var advancedMotion = GetAdvancedMotionData(motionType);
        if (advancedMotion != null)
        {
            PlayAdvancedMotion(advancedMotion);
            return;
        }
        
        // 2. 기본 모션 시스템 (기존 방식)
        if (motionType == "Buff")
        {
            // 버프는 전용 처리로 Stand 폴백까지 포함
            PlayBuffMotion();
        }
        else
        {
            PlayBasicMotion(motionType);
        }
    }

    /// <summary>
    /// 스킬 모션 타입의 예상 지속 시간(초). MotionData에 duration이 있으면 사용하고, 없거나 0이면 fallback을 쓴다.
    /// 영체 연출 등 공격 모션 길이에 맞춰 소멸 타이밍을 맞출 때 사용.
    /// </summary>
    public float GetExpectedSkillMotionDuration(string motionType, float fallbackSeconds = 0.45f)
    {
        var motionData = GetAdvancedMotionData(motionType);
        if (motionData != null && motionData.duration > 0f)
            return motionData.duration;
        return fallbackSeconds;
    }

    /// <summary>
    /// 고급 모션 데이터를 가져옵니다.
    /// </summary>
    private MotionData GetAdvancedMotionData(string motionType)
    {
        // 캐릭터별 모션 데이터 오브젝트 확인
        string motionDataPath = $"MotionData/{characterStats.data.ID}_Motions";
        var motionDataObject = Resources.Load<MotionDataObject>(motionDataPath);
        
        if (motionDataObject != null)
        {
            var motionData = motionDataObject.GetMotionData(motionType);
            if (motionData != null)
            {
                Debug.Log($"[MotionController] 고급 모션 데이터 발견: {motionType}");
                return motionData;
            }
        }
        
        return null;
    }

    /// <summary>
    /// 고급 모션을 재생합니다.
    /// </summary>
    private void PlayAdvancedMotion(MotionData motionData)
    {
        // 스프라이트 변경
        if (!string.IsNullOrEmpty(motionData.spritePath))
        {
            Sprite motionSprite = Resources.Load<Sprite>(motionData.spritePath);
            if (motionSprite != null)
            {
                spriteRenderer.sprite = motionSprite;
                Debug.Log($"[MotionController] 고급 모션 스프라이트 변경: {motionData.spritePath}");
            }
        }
        
        // 위치 오프셋 적용
        if (motionData.offset != Vector2.zero)
        {
            transform.localPosition += new Vector3(motionData.offset.x, motionData.offset.y, 0);
        }
        
        // 모션 지속 시간이 있으면 코루틴으로 처리
        if (motionData.duration > 0)
        {
            StartCoroutine(AdvancedMotionCoroutine(motionData));
        }
    }

    /// <summary>
    /// 고급 모션 코루틴 (지속 시간, 복귀 등 처리)
    /// </summary>
    private IEnumerator AdvancedMotionCoroutine(MotionData motionData)
    {
        yield return new WaitForSeconds(motionData.duration);
        
        // 이전 모션으로 복귀 여부 확인
        if (motionData.returnToPrevious)
        {
            ResetMotion();
        }
        
        // 위치 오프셋 복구
        if (motionData.offset != Vector2.zero)
        {
            transform.localPosition -= new Vector3(motionData.offset.x, motionData.offset.y, 0);
        }
    }

    /// <summary>
    /// 기본 모션을 재생합니다 (기존 방식).
    /// </summary>
    private void PlayBasicMotion(string motionType)
    {
        // 실제 스프라이트 변경
        string motionPath = $"{characterStats.data.Sprite}/{motionType}";
        Sprite motionSprite = Resources.Load<Sprite>(motionPath);
        if (motionSprite != null)
        {
            spriteRenderer.sprite = motionSprite;
            Debug.Log($"[MotionController] 기본 모션 스프라이트 변경: {motionPath}");
        }
        else
        {
            Debug.LogWarning($"[MotionController] {motionPath} 스프라이트를 찾지 못했습니다.");
        }
    }

    private SpriteRenderer GetPrimarySpriteRenderer()
    {
        if (spriteRenderer != null) return spriteRenderer;

        // BattleEffectManager.KnockbackCharacter와 동일: 캐릭터 루트의 "Sprite" 자식 우선 (첫 번째 SR이 UI/바일 때 오동작 방지)
        var stats = ResolveCharacterStats();
        if (stats != null)
        {
            Transform sp = stats.transform.Find("Sprite");
            if (sp != null)
            {
                var r = sp.GetComponent<SpriteRenderer>();
                if (r != null) return r;
            }
        }

        return GetComponentInChildren<SpriteRenderer>(true);
    }

    /// <summary>
    /// 넉다운 부착 직시(코드에서 호출), 또는 기절/넉다운 토큰 소모 직후 ~ 다음 본인 턴 시작까지 유지할 피격 자세(스프라이트 Hit, 틴트 흰색).
    /// </summary>
    public void ApplyPostStunReleaseHitHold()
    {
        ResolveCharacterStats();
        if (characterStats == null || characterStats.data == null) return;
        var sr = GetPrimarySpriteRenderer();
        if (sr == null) return;

        currentMotion = "Hit";
        string hitPath = $"{characterStats.data.Sprite}/Hit";
        Sprite hitSprite = Resources.Load<Sprite>(hitPath);
        if (hitSprite != null)
        {
            sr.sprite = hitSprite;
            Debug.Log($"[MotionController] 피격 자세 유지(Hit): {hitPath} ({gameObject.name})");
        }
        else
        {
            Debug.LogWarning($"[MotionController] {hitPath} 스프라이트를 찾지 못했습니다. ({gameObject.name})");
        }

        sr.color = Color.white;
    }

    /// <summary>
    /// 피격 모션을 실행합니다.
    /// </summary>
    public void PlayHitMotion()
    {
        ResolveCharacterStats();
        if (characterStats == null || !characterStats.IsActive) return;
        currentMotion = "Hit";
        var sr = GetPrimarySpriteRenderer();
        if (sr == null) return;

        string hitPath = $"{characterStats.data.Sprite}/Hit";
        Sprite hitSprite = Resources.Load<Sprite>(hitPath);
        if (hitSprite != null)
        {
            sr.sprite = hitSprite;
            Debug.Log($"[MotionController] 피격 모션 스프라이트 변경: {hitPath}");
        }
        else
        {
            Debug.LogWarning($"[MotionController] {hitPath} 스프라이트를 찾지 못했습니다.");
        }

        sr.color = Color.red;
    }

    /// <summary>
    /// 버프 모션을 실행합니다.
    /// </summary>
    public void PlayBuffMotion()
    {
        if (!characterStats.IsActive) return;
        currentMotion = "Buff";
        // 실제 스프라이트 변경 (버프 전용 스프라이트가 있으면 사용, 없으면 Stand 사용)
        string buffPath = $"{characterStats.data.Sprite}/Buff";
        Sprite buffSprite = Resources.Load<Sprite>(buffPath);
        if (buffSprite != null)
        {
            spriteRenderer.sprite = buffSprite;
            Debug.Log($"[MotionController] 버프 모션 스프라이트 변경: {buffPath}");
        }
        else
        {
            // 버프 전용 스프라이트가 없으면 Stand 스프라이트 사용
            string standPath = $"{characterStats.data.Sprite}/Stand";
            Sprite standSprite = Resources.Load<Sprite>(standPath);
            if (standSprite != null)
            {
                spriteRenderer.sprite = standSprite;
                Debug.Log($"[MotionController] 버프 모션 - Stand 스프라이트 사용: {standPath}");
            }
        }

        // === 연한 파란색으로 색상 변경 (버프 효과) ===
        spriteRenderer.color = new Color(0.5f, 0.5f, 1f, 1f); // 연한 파란색
    }

    /// <summary>
    /// 디버프 시전자 모션: Buff 스프라이트는 재사용하되 색상은 바꾸지 않는다.
    /// </summary>
    public void PlayDebuffCasterMotion()
    {
        if (!characterStats.IsActive) return;
        currentMotion = "Buff";

        string buffPath = $"{characterStats.data.Sprite}/Buff";
        Sprite buffSprite = Resources.Load<Sprite>(buffPath);
        if (buffSprite != null)
        {
            spriteRenderer.sprite = buffSprite;
            Debug.Log($"[MotionController] 디버프 시전자 모션 스프라이트 변경: {buffPath}");
        }
        else
        {
            string standPath = $"{characterStats.data.Sprite}/Stand";
            Sprite standSprite = Resources.Load<Sprite>(standPath);
            if (standSprite != null)
            {
                spriteRenderer.sprite = standSprite;
                Debug.Log($"[MotionController] 디버프 시전자 모션 - Stand 스프라이트 사용: {standPath}");
            }
        }

        // 요구사항: 디버프 시전자 색상 효과 없음
        spriteRenderer.color = Color.white;
    }

    /// <summary>
    /// 디버프 대상 모션: 피격 자세 + 보라색 틴트.
    /// </summary>
    public void PlayDebuffTargetMotion()
    {
        PlayHitMotion();
        var sr = GetPrimarySpriteRenderer();
        if (sr != null)
            sr.color = new Color(0.65f, 0.35f, 0.9f, 1f); // 보라색
    }

    /// <summary>
    /// 죽는 모션을 실행합니다.
    /// </summary>
    public void PlayDeathMotion()
    {
        if (!characterStats.IsActive) return;
        currentMotion = "Death";
        // 실제 스프라이트 변경
        string deathPath = $"{characterStats.data.Sprite}/Death";
        Sprite deathSprite = Resources.Load<Sprite>(deathPath);
        if (deathSprite != null)
        {
            spriteRenderer.sprite = deathSprite;
            Debug.Log($"[MotionController] 죽는 모션 스프라이트 변경: {deathPath}");
        }
        else
        {
            Debug.LogWarning($"[MotionController] {deathPath} 스프라이트를 찾지 못했습니다.");
        }
    }

    public void ResetMotion()
    {
        ResolveCharacterStats();
        if (characterStats == null) return;
        if (characterStats.IsDead) return;

        currentMotion = "Stand";
        var sr = GetPrimarySpriteRenderer();
        if (sr == null || characterStats.data == null) return;

        string standPath = $"{characterStats.data.Sprite}/Stand";
        Sprite standSprite = Resources.Load<Sprite>(standPath);
        if (standSprite != null)
        {
            sr.sprite = standSprite;
            Debug.Log($"[MotionController] 스탠드로 복귀: {standPath}");
        }
        else
        {
            Debug.LogWarning($"[MotionController] {standPath} 스프라이트를 찾지 못했습니다.");
        }

        sr.color = Color.white;
    }

    /// <summary>
    /// 모션 시퀀스를 재생합니다.
    /// </summary>
    private IEnumerator PlayMotionSequence()
    {
        if (!characterStats.IsActive || string.IsNullOrEmpty(currentMotion)) yield break;

        // 현재 스프라이트 저장
        Sprite currentSprite = spriteRenderer.sprite;
        
        // 모션 스프라이트로 변경
        string motionPath = $"{characterStats.data.Sprite}/{currentMotion}";
        Sprite motionSprite = Resources.Load<Sprite>(motionPath);
        if (motionSprite != null)
        {
            spriteRenderer.sprite = motionSprite;
            Debug.Log($"[MotionController] 모션 변경: {currentMotion}");
        }
        else
        {
            Debug.LogWarning($"[MotionController] {motionPath} 모션 스프라이트를 찾지 못했습니다.");
        }
        
        // 모션 지속 시간
        yield return new WaitForSeconds(0.5f);

        // 원래 스프라이트로 복귀
        spriteRenderer.sprite = currentSprite;
        currentMotion = null;  // 모션 필드 초기화
        Debug.Log($"[MotionController] 모션 복귀");
    }

    /// <summary>
    /// 캐릭터를 원래 위치로 되돌립니다.
    /// </summary>
    public void ResetPosition()
    {
        // GameObject가 파괴되었는지 확인
        if (this == null || gameObject == null)
        {
            Debug.LogWarning("[MotionController] GameObject가 파괴되어 위치 복귀 생략");
            return;
        }
        
        // 호출 스택 추적을 위한 디버그 로그
                Debug.Log($"[MotionController] ResetPosition 호출됨: {gameObject.name}, 현재 위치: {transform.position}, 로컬 위치: {transform.localPosition}");
        
        // 캐릭터가 돌아가는 순간 모든 팝업 정리 (예외 오브젝트 제외)
        ClearAllPopupsExceptExceptions();
        
        // 월드 좌표로 원래 위치 복귀 (슬롯의 원래 위치)
        transform.position = transform.parent.position;
        isInBattlePosition = false;
        
        // 스프라이트 위치도 복귀 (spriteRenderer가 null인 경우 대비)
        SpriteRenderer targetSpriteRenderer = spriteRenderer;
        
        // spriteRenderer가 null인 경우 "Sprite" 오브젝트에서 찾기
        if (targetSpriteRenderer == null)
        {
            Transform spriteTransform = transform.Find("Sprite");
            if (spriteTransform != null)
            {
                targetSpriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
            }
        }
        
        // 여전히 null인 경우 GetComponentInChildren 사용
        if (targetSpriteRenderer == null)
        {
            targetSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        if (targetSpriteRenderer != null)
        {
            // SpriteRenderer가 파괴되지 않았는지 확인
            if (targetSpriteRenderer.gameObject != null)
            {
                targetSpriteRenderer.transform.localPosition = Vector3.zero;
                Debug.Log($"[MotionController] {gameObject.name} 캐릭터 및 스프라이트 위치 복귀: (0,0,0)");
            }
            else
            {
                Debug.LogWarning($"[MotionController] {gameObject.name}의 SpriteRenderer가 파괴되어 위치 복귀 실패");
            }
        }
        else
        {
            Debug.LogWarning($"[MotionController] {gameObject.name}에서 SpriteRenderer를 찾을 수 없어 위치 복귀 실패");
        }
    }

    /// <summary>
    /// 예외 오브젝트를 제외한 모든 팝업을 정리합니다.
    /// </summary>
    private void ClearAllPopupsExceptExceptions()
    {
        // BattleUI 하위의 모든 팝업 오브젝트 찾기
        GameObject battleUI = GameObject.Find("BattleUI");
        if (battleUI != null)
        {
            // BattleUI의 모든 자식 오브젝트 중 팝업들 찾기
            Transform[] allChildren = battleUI.GetComponentsInChildren<Transform>();
            
            foreach (Transform child in allChildren)
            {
                if (child == null || child.gameObject == null) continue;
                
                // 예외 오브젝트 체크 (이름으로 구분)
                if (IsExceptionObject(child.gameObject))
                {
                    Debug.Log($"[MotionController] 예외 오브젝트 유지: {child.name}");
                    continue;
                }
                
                // 팝업 오브젝트인지 확인
                if (IsPopupObject(child.gameObject))
                {
                    Debug.Log($"[MotionController] 팝업 제거: {child.name}");
                    Destroy(child.gameObject);
                }
            }
        }
        
        Debug.Log("[MotionController] 캐릭터 복귀 시 팝업 정리 완료");
    }

    /// <summary>
    /// 예외 오브젝트인지 확인합니다.
    /// </summary>
    private bool IsExceptionObject(GameObject obj)
    {
        // 예외 오브젝트 이름들 (필요에 따라 수정)
        string[] exceptionNames = {
            "BuffandDebuffPopup",  // 버프/디버프 팝업 프리팹
            "DamageCount",         // 데미지 카운트 프리팹
            // 추가 예외 오브젝트들...
        };
        
        foreach (string exceptionName in exceptionNames)
        {
            if (obj.name.Contains(exceptionName))
                return true;
        }
        
        return false;
    }

    /// <summary>
    /// 팝업 오브젝트인지 확인합니다.
    /// </summary>
    private bool IsPopupObject(GameObject obj)
    {
        // 팝업 관련 컴포넌트나 이름으로 판별 (리플렉션 사용)
        if (obj.GetComponent(System.Type.GetType("BuffDebuffPopup")) != null) return true;
        if (obj.GetComponent(System.Type.GetType("DamagePopup")) != null) return true;
        if (obj.name.Contains("Popup")) return true;
        if (obj.name.Contains("Effect")) return true;
        
        return false;
    }
}
