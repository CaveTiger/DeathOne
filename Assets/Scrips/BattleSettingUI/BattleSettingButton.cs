using UnityEngine;

public class BattleSettingButton : MonoBehaviour
{
    [Header("탭 콘텐츠 패널")]
    [SerializeField] private GameObject characterTabPanel;
    [SerializeField] private GameObject skillTabPanel;
    [SerializeField] private GameObject blessingTabPanel;

    private void Start()
    {
        // 시작 시 캐릭터 탭을 기본으로 표시
        ShowCharacterTab();
    }

    public void ShowCharacterTab()
    {
        if (characterTabPanel != null) characterTabPanel.SetActive(true);
        if (skillTabPanel != null) skillTabPanel.SetActive(false);
        if (blessingTabPanel != null) blessingTabPanel.SetActive(false);
    }

    public void ShowSkillTab()
    {
        if (characterTabPanel != null) characterTabPanel.SetActive(false);
        if (skillTabPanel != null) skillTabPanel.SetActive(true);
        if (blessingTabPanel != null) blessingTabPanel.SetActive(false);
    }

    public void ShowBlessingTab()
    {
        if (characterTabPanel != null) characterTabPanel.SetActive(false);
        if (skillTabPanel != null) skillTabPanel.SetActive(false);
        if (blessingTabPanel != null) blessingTabPanel.SetActive(true);
    }
} 