using UnityEngine;

public class StatusEffectInstanceBuff : MonoBehaviour
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

    public void awake()
    {
        // if (owner != null)
        // {
        //     owner.Def += value;
        //     Debug.Log($"[StatusEffectInstanceBuff] {owner.Label}의 방어력이 {value}만큼 증가! 현재 DEF: {owner.Def}");
        // }
    }

    public void Start()
    {

    }
    
    /// <summary>
    /// 버프 효과 해제(스탯 원복)
    /// </summary>
    public void RemoveEffect()
    {
        if (owner == null) return;

        var statField = owner.GetType().GetField(effectData.statType.ToString());
        if (statField != null)
        {
            int current = (int)statField.GetValue(owner);
            statField.SetValue(owner, current - value);
        }
        else
        {
            Debug.LogWarning($"[StatusEffectInstanceBuff] {effectData.statType} 필드를 찾을 수 없습니다.");
        }
    }

    public void OnTurnEnd()
    {
        if (owner == null) return;
        remainingTurns--;
        if (remainingTurns <= 0)
        {
            // 효과 해제
            RemoveEffect();
            Destroy(this.gameObject); // 자신 파괴
        }
    }

    public void Initialize(StatusEffectData effectData, int duration, int value, CharacterStats owner)
    {
        this.effectData = effectData;
        this.remainingTurns = duration;
        this.value = value;
        this.owner = owner;

        // 효과 즉시 적용
        if (owner == null || owner.Equals(null)) return;
        ApplyEffect();
    }

    public void ApplyEffect()
    {
        if (owner == null) return;

        // statType의 이름과 동일한 필드를 찾아서 값을 증가
        var statField = owner.GetType().GetField(effectData.statType.ToString());
        if (statField != null)
        {
            int current = (int)statField.GetValue(owner);
            statField.SetValue(owner, current + value);
        }
        else
        {
            Debug.LogWarning($"[StatusEffectInstanceBuff] {effectData.statType} 필드를 찾을 수 없습니다.");
        }
    }
}
