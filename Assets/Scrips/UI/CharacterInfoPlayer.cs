using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoPlayer : CharacterInfo
{
    [Header("Player Specific UI")]
    [SerializeField] protected Text collapseChanceText; // 붕괴확률 표시용 텍스트

    public float CollapseChance { get; protected set; }
    private CharacterStats lastUpdatedStats; // 마지막으로 업데이트된 스탯 저장

    void Update()
    {
        UpdateInfo();
        ShowInfo();
    }

    public override void ShowInfo()
    {
        base.ShowInfo();
        if (collapseChanceText != null)
            collapseChanceText.text = $"{CollapseChance:P0}"; // 퍼센트로 표시
    }
} 