using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardItemUI : MonoBehaviour
{
    [Header("UI 요소들")]
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemTypeText;
    public TextMeshProUGUI itemDescriptionText;
    
    [Header("아이콘 설정")]
    public Sprite skillDefaultIcon;
    public Sprite characterDefaultIcon;
    
    private string currentItemID;
    private string currentItemType;
    
    /// <summary>
    /// 보상 아이템을 설정합니다
    /// </summary>
    /// <param name="itemID">아이템 ID</param>
    /// <param name="itemType">아이템 타입 (스킬/캐릭터)</param>
    public void SetupRewardItem(string itemID, string itemType)
    {
        currentItemID = itemID;
        currentItemType = itemType;
        
        UpdateUI();
    }
    
    /// <summary>
    /// UI를 업데이트합니다
    /// </summary>
    private void UpdateUI()
    {
        if (string.IsNullOrEmpty(currentItemID)) return;
        
        // 아이템 타입 텍스트 설정
        if (itemTypeText != null)
        {
            itemTypeText.text = currentItemType;
        }
        
        // 아이템 타입에 따라 다른 처리
        if (currentItemType == "스킬")
        {
            SetupSkillItem();
        }
        else if (currentItemType == "캐릭터")
        {
            SetupCharacterItem();
        }
    }
    
    /// <summary>
    /// 스킬 아이템을 설정합니다
    /// </summary>
    private void SetupSkillItem()
    {
        // 스킬 데이터에서 정보 가져오기
        if (SkillData.skillDict.TryGetValue(currentItemID, out var skillData))
        {
            // 아이템 이름 설정
            if (itemNameText != null)
            {
                itemNameText.text = skillData.Name;
            }
            
            // 아이템 설명 설정
            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = skillData.Description;
            }
            
            // 아이콘 설정
            if (itemIcon != null)
            {
                // 스킬 아이콘 로드 시도
                string iconPath = $"SkillIcon/{currentItemID}";
                Sprite skillIcon = Resources.Load<Sprite>(iconPath);
                
                if (skillIcon != null)
                {
                    itemIcon.sprite = skillIcon;
                }
                else if (skillDefaultIcon != null)
                {
                    itemIcon.sprite = skillDefaultIcon;
                }
            }
        }
        else
        {
            // 스킬 데이터가 없는 경우 기본값 설정
            if (itemNameText != null)
                itemNameText.text = $"알 수 없는 스킬 ({currentItemID})";
                
            if (itemDescriptionText != null)
                itemDescriptionText.text = "스킬 정보를 찾을 수 없습니다.";
                
            if (itemIcon != null && skillDefaultIcon != null)
                itemIcon.sprite = skillDefaultIcon;
        }
    }
    
    /// <summary>
    /// 캐릭터 아이템을 설정합니다
    /// </summary>
    private void SetupCharacterItem()
    {
        // 캐릭터 데이터에서 정보 가져오기
        if (CharacterData.characterDict.TryGetValue(currentItemID, out var characterData))
        {
            // 아이템 이름 설정
            if (itemNameText != null)
            {
                itemNameText.text = characterData.Label;
            }
            
            // 아이템 설명 설정
            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = $"새로운 캐릭터를 해금했습니다!";
            }
            
            // 아이콘 설정
            if (itemIcon != null)
            {
                // 캐릭터 스프라이트 로드 시도
                string spritePath = $"UnitSprite/{characterData.ID}/Stand";
                Sprite characterSprite = Resources.Load<Sprite>(spritePath);
                
                if (characterSprite != null)
                {
                    itemIcon.sprite = characterSprite;
                }
                else if (characterDefaultIcon != null)
                {
                    itemIcon.sprite = characterDefaultIcon;
                }
            }
        }
        else
        {
            // 캐릭터 데이터가 없는 경우 기본값 설정
            if (itemNameText != null)
                itemNameText.text = $"알 수 없는 캐릭터 ({currentItemID})";
                
            if (itemDescriptionText != null)
                itemDescriptionText.text = "캐릭터 정보를 찾을 수 없습니다.";
                
            if (itemIcon != null && characterDefaultIcon != null)
                itemIcon.sprite = characterDefaultIcon;
        }
    }
    
    /// <summary>
    /// 현재 아이템 ID를 반환합니다
    /// </summary>
    public string GetItemID()
    {
        return currentItemID;
    }
    
    /// <summary>
    /// 현재 아이템 타입을 반환합니다
    /// </summary>
    public string GetItemType()
    {
        return currentItemType;
    }
} 