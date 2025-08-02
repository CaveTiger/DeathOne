using UnityEngine;
using System.Collections;

public class CharacterMotionController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CharacterStats characterStats;
    private string currentMotion;  // 현재 모션을 저장할 필드 추가

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
        PlayBasicMotion(motionType);
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

    /// <summary>
    /// 피격 모션을 실행합니다.
    /// </summary>
    public void PlayHitMotion()
    {
        if (!characterStats.IsActive) return;
        currentMotion = "Hit";
        // 실제 스프라이트 변경
        string hitPath = $"{characterStats.data.Sprite}/Hit";
        Sprite hitSprite = Resources.Load<Sprite>(hitPath);
        if (hitSprite != null)
        {
            spriteRenderer.sprite = hitSprite;
            Debug.Log($"[MotionController] 피격 모션 스프라이트 변경: {hitPath}");
        }
        else
        {
            Debug.LogWarning($"[MotionController] {hitPath} 스프라이트를 찾지 못했습니다.");
        }

        // === 빨간색으로 색상 변경 ===
        spriteRenderer.color = Color.red;
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
        currentMotion = "Stand";
        // Stand 스프라이트로 실제로 변경
        string standPath = $"{characterStats.data.Sprite}/Stand";
        Sprite standSprite = Resources.Load<Sprite>(standPath);
        if (standSprite != null)
        {
            spriteRenderer.sprite = standSprite;
            Debug.Log($"[MotionController] 스탠드로 복귀: {standPath}");
        }
        else
        {
            Debug.LogWarning($"[MotionController] {standPath} 스프라이트를 찾지 못했습니다.");
        }

        // === 색상도 원래대로 복구 ===
        spriteRenderer.color = Color.white;
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
}
