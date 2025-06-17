using UnityEngine;

public class ArrangeTab : MonoBehaviour
{
    [Header("탭 오브젝트 연결")]
    public GameObject characterTab;
    public GameObject skillTab;
    public GameObject passiveTab;

    public void ShowCharacterTab()
    {
        characterTab.SetActive(true);
        skillTab.SetActive(false);
        passiveTab.SetActive(false);
    }

    public void ShowSkillTab()
    {
        characterTab.SetActive(false);
        skillTab.SetActive(true);
        passiveTab.SetActive(false);
    }

    public void ShowPassiveTab()
    {
        characterTab.SetActive(false);
        skillTab.SetActive(false);
        passiveTab.SetActive(true);
    }
}
