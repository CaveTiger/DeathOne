using UnityEngine;

public class StatusEffectInstanceReaction : MonoBehaviour
{
    [Header("상태이상 기본 정보")]
    [SerializeField] private StatusEffectData effectData;   // 인스펙터에서 연결
    public StatusEffectData EffectData => effectData;

    [Tooltip("효과가 지속될 남은 턴 수")]
    public int remainingTurns;        // 남은 턴 수

    [Tooltip("상태이상의 수치 (피해량, 회복량, 버프 수치 등)")]
    public int value;                 // 피해량 등

    [Tooltip("현재 상태이상이 활성화되어 있는지 여부")]
    public bool isActive = true;       // 효과 활성 여부

    [Tooltip("이 상태이상이 적용된 캐릭터")]
    public CharacterStats owner;       // 상태이상 소유자

    [Header("팝업 관련")]
    [Tooltip("상태이상 팝업 핸들러")]
    public StatusPopupHandler popupHandler;

    public int triggerCount; // 인스턴스별로 관리

    public void Initialize(StatusEffectData data, int duration, int value, CharacterStats owner)
    {
        this.effectData = data;
        this.remainingTurns = duration;
        this.value = value;
        this.owner = owner;
        this.triggerCount = data.maxTriggerCount;
    }

    public virtual bool OnTakeDamage(ref int damage)
    {
        // 예시: 피해무시
        if (effectData != null && effectData.EffectID == "021002" && triggerCount > 0)
        {
            triggerCount--;
            Debug.Log($"[피해무시] {owner.Label}가 피해를 무시했습니다! 남은 횟수: {triggerCount}");
            damage = 0;
            if (triggerCount <= 0)
            {
                remainingTurns = 0;
                // 필요하다면 효과 해제 로직 호출
                Destroy(this.gameObject);
            }
            return true;
        }
        // 기본은 아무 효과 없음
        return false;
    }
}

