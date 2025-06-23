using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.GraphicsBuffer;
using System.Collections;

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
        if (skillData != null)
        {
            groupName = skillData.Group;
            cooldownTime = skillData.Cooldown;
            isActive = true;
            currentCooldown = 0f;

            UpdateSkillUI();
            
            Debug.Log($"[SkillInstance] 스킬 데이터 설정 완료: {skillData.Name}");
        }
    }

    private void UpdateSkillUI()
    {
        if (skillData == null) return;

        // 이름 업데이트
        if (skillNameText != null)
        {
            skillNameText.text = skillData.Name;
        }

        // 아이콘 업데이트
        if (skillIconImage != null && !string.IsNullOrEmpty(skillData.Icon))
        {
            Sprite iconSprite = Resources.Load<Sprite>(skillData.Icon);
            if (iconSprite != null)
            {
                skillIconImage.sprite = iconSprite;
            }
            else
            {
                Debug.LogWarning($"[SkillInstance] 스킬 아이콘을 찾을 수 없습니다: {skillData.Icon}");
            }
        }
    }

    public void SetCaster(CharacterStats newCaster)
    {
        caster = newCaster;
    }

    private void Awake()
    {
        // skillID가 Inspector에서 설정된 경우에만 로드
        if (!string.IsNullOrEmpty(skillID) && SkillData.skillDict.TryGetValue(skillID, out skillData))
        {
            groupName = skillData.Group;
            cooldownTime = skillData.Cooldown;
            isActive = true;
            currentCooldown = 0f;
            UpdateSkillUI();

            Debug.Log($"SkillSlot: {skillID} 스킬 데이터 불러오기 성공");
        }
        else
        {
            // skillID가 비어있으면 SetSkillData로 나중에 설정될 예정
            Debug.Log("[SkillInstance] skillID가 비어있거나 스킬 데이터를 찾지 못했습니다. SetSkillData로 설정될 예정입니다.");
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

        SkillManager.Instance.UseSkill(skillData, caster, target, skillData);
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


