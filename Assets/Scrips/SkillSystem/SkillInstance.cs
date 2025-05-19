using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.GraphicsBuffer;

public class SkillInstance : MonoBehaviour
{
    [SerializeField] private string skillID; //스킬 ID

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
    [SerializeField] private GameObject csaterObject;

    public void SetSkillData(SkillData data)
    {
        skillData = data;
        // UI 등 갱신 코드 추가 가능
    }

    public void SetCaster(CharacterStats newCaster)
    {
        caster = newCaster;
    }

    private void Awake()
    {
        if (SkillData.skillDict.TryGetValue(skillID, out skillData))
        {
            groupName = skillData.Group;
            cooldownTime = skillData.Cooldown;
            isActive = true;
            currentCooldown = 0f;

            Debug.Log($"SkillSlot: {skillID} 스킬 데이터 불러오기 성공");
        }
        else
        {
            Debug.LogError($"SkillSlot: {skillID}에 해당하는 스킬 데이터를 찾지 못했습니다.");
        }
    }
    private void Start()
    {
        GameObject player = GameObject.Find("Unit_000001");
        if (player != null)
        {
            caster = player.GetComponent<CharacterStats>();
            //Debug.Log($"[SkillInstance] 임시 캐스터 연결 성공: {caster?.Label}");
        }
        else
        {
            Debug.LogWarning("[SkillInstance] 임시 캐스터를 찾지 못했습니다.");
        }
    }
    public void UpdateTarget()
    {
        target = TargetSelector.Instance.GetCurrentTarget();
    }
    public void UseSkill()
    {
        if (caster == null || !caster.IsMyTurn)
        {
            Debug.LogWarning("[UseSkill] 지금은 내 턴이 아닙니다. 스킬 발동 중지.");
            return;
        }
        if (!isActive || currentCooldown > 0 || skillData == null) return;

        UpdateTarget();

        // SkillTarget에 따라 target을 올바르게 지정
        if (skillData.SkillTarget == "Me")
        {
            target = caster;
        }
        // "Ally"는 선택한 아군(본인 제외), "AllAllies"는 SkillManager에서 처리

        if (target == null)
        {
            Debug.LogWarning("[UseSkill] 타겟이 없습니다. 스킬 발동 중지.");
            return;
        }

        bool success = SkillManager.Instance.UseSkill(skillData, caster, target);

        if (success)
        {
            TurnManager.Instance.EndTurn();
        }
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


