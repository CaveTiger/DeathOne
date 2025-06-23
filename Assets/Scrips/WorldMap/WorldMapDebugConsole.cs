using UnityEngine;
using UnityEngine.UI; // InputField와 Button을 사용하기 위해 필요
using TMPro; // TextMeshPro InputField를 사용할 경우 필요

public class WorldMapDebugConsole : MonoBehaviour
{
    [Header("Debug Console UI")]
    [SerializeField] private GameObject consolePanel; // 디버그 콘솔 전체 패널
    [SerializeField] private TMP_InputField characterIdInput;
    [SerializeField] private TMP_InputField skillIdInput;

    void Update()
    {
        // Ctrl + D 키를 누르면 콘솔 패널을 토글
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
        {
            ToggleConsole();
        }
    }

    public void ToggleConsole()
    {
        if (consolePanel != null)
        {
            consolePanel.SetActive(!consolePanel.activeSelf);
        }
    }

    // 캐릭터 추가 버튼에 연결할 메서드
    public void OnClick_AddCharacter()
    {
        string charId = characterIdInput.text;
        if (string.IsNullOrEmpty(charId))
        {
            Debug.LogWarning("[DebugConsole] 캐릭터 ID를 입력하세요.");
            return;
        }

        GameProgressManager.Instance.AddCharacterToInventory(charId, 1);
        Debug.Log($"[DebugConsole] 캐릭터 {charId}를 인벤토리에 추가했습니다.");
        characterIdInput.text = ""; // 입력 필드 초기화
    }

    // 스킬 해금 버튼에 연결할 메서드
    public void OnClick_UnlockSkill()
    {
        string skillId = skillIdInput.text;
        if (string.IsNullOrEmpty(skillId))
        {
            Debug.LogWarning("[DebugConsole] 스킬 ID를 입력하세요.");
            return;
        }

        GameProgressManager.Instance.UnlockSkill(skillId);
        Debug.Log($"[DebugConsole] 스킬 {skillId}를 해금했습니다.");
        skillIdInput.text = ""; // 입력 필드 초기화
    }
} 