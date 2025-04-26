using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.GraphicsBuffer;

public class SkillInstance : MonoBehaviour
{
    private string skillID; //스킬 ID
    private bool isActive; //스킬 사용가능 여부
    private float cooldownTime;
    private float currentCooldown;
    private string groupName;
    private SkillData skillData;
    private int slotIndex;  // 슬롯의 순서를 지정하는 인덱스
    private CharacterStats caster;
    private CharacterStats target;

    [SerializeField] private Image skillIconImage;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private TextMeshProUGUI skillNameText;

    public void Initialize(string id, string group = "", int index = -1, CharacterStats owner = null)
    {
        skillID = id;
        groupName = group;
        slotIndex = index;  // 슬롯 인덱스 설정
        isActive = true;
        currentCooldown = 0f;
        caster = owner;

        if (target == null)
        {
            Debug.LogWarning("타겟이 지정되지 않았습니다.");
            return;
        }
        if (SkillData.skillDict.TryGetValue(skillID, out skillData))
        {
            cooldownTime = skillData.Cooldown;
        }
        else
        {
            Debug.LogError($"[SkillSlotData] 스킬 ID {skillID}에 해당하는 SkillData를 찾을 수 없습니다.");
        }
    }
    public void UpdateTarget()
    {
        target = TargetSelector.Instance.GetCurrentTarget();
    }
    public void UseSkill()
    {
        Debug.Log("스킬 눌림");

        if (!isActive || currentCooldown > 0 || skillData == null) return;

        // 스킬 사용 로직
        Debug.Log($"[SkillSlotData] 스킬 사용: {skillData.Name} (그룹: {groupName})");
        Debug.Log($"데미지: {skillData.DamageMin} ~ {skillData.DamageMax}");
        Debug.Log($"타겟: {skillData.SkillTarget}");
        Debug.Log($"모션: {skillData.Motion}");
        Debug.Log($"효과: {string.Join(", ", skillData.SkillEffects)}");
        SkillManager.Instance.UseSkill(skillData, caster,target);
        // 쿨다운 시작
        currentCooldown = cooldownTime;
        cooldownImage.fillAmount = 1f;
    }

    public void SetGroup(string newGroupName) //그룹을 지정하기
    {
        groupName = newGroupName;
    }

    public string GetGroup() //그룹을 반환하기
    {
        return groupName;
    }

    public int GetSlotIndex() //그룹을 반환하기
    {
        return slotIndex;  // 지정된 슬롯 인덱스 반환
    }

    public void SetSlotIndex(int index)
    {
        slotIndex = index;  // 슬롯 인덱스 설정
    }

    public SkillData GetSkillData()
    {
        return skillData;
    }
}


