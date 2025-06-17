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
        if (!isInBattlePosition && battlePoint != null)
        {
            transform.position = battlePoint.position;
            isInBattlePosition = true;
            Debug.Log($"[MotionController] 전투 위치로 이동: {battlePoint.name}");
        }
        else if (battlePoint == null)
        {
            Debug.LogWarning("[MotionController] 전투 위치 오브젝트가 설정되지 않았습니다.");
        }
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
        // 실제 스프라이트 변경
        string motionPath = $"{characterStats.data.Sprite}/{motionType}";
        Sprite motionSprite = Resources.Load<Sprite>(motionPath);
        if (motionSprite != null)
        {
            spriteRenderer.sprite = motionSprite;
            Debug.Log($"[MotionController] 모션 스프라이트 변경: {motionPath}");
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
        transform.localPosition = Vector3.zero;
        isInBattlePosition = false;
        Debug.Log("[MotionController] 원래 위치로 복귀: (0,0,0)");
    }
}
