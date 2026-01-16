using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 축복 선택 UI 패널.
/// - Unity 기본 Button을 사용하며, 이 패널이 모든 축복 토글/정수/비주얼을 관리한다.
/// - Button의 OnClick 이벤트에 BlessingData를 직접 연결하면 됩니다.
/// - 라인(총 5개 예정) 개념을 가지고 있으며, 라인별 선택 제한을 설정할 수 있습니다.
/// </summary>
public class BlessingPanel : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private TextMeshProUGUI essenceText; // 현재 강자의 정수 표시 텍스트

    [Header("라인별 최대 선택 개수 설정 (총 5개 라인)")]
    [Tooltip("라인 0의 최대 선택 가능 개수 (예: 권능은 1개, 일반 축복은 무제한 등)")]
    [SerializeField] private int maxSelectLine0 = 1;
    
    [Tooltip("라인 1의 최대 선택 가능 개수")]
    [SerializeField] private int maxSelectLine1 = 1;
    
    [Tooltip("라인 2의 최대 선택 가능 개수")]
    [SerializeField] private int maxSelectLine2 = 1;
    
    [Tooltip("라인 3의 최대 선택 가능 개수")]
    [SerializeField] private int maxSelectLine3 = 1;
    
    [Tooltip("라인 4의 최대 선택 가능 개수")]
    [SerializeField] private int maxSelectLine4 = 1;

    [Header("라인별 활성화된 축복 관리 (라인별 비트마스크 방식)")]
    [Tooltip("각 라인(0~4)별로 비트마스크 문자열로 저장 (예: \"10010\" = 1번과 4번 축복 활성화)")]
    [SerializeField] private string[] activeBlessingByLine = new string[5] { "", "", "", "", "" };

    [System.Serializable]
    public class BlessingButtonEntry
    {
        public Button button;
        public BlessingData blessingData;
    }

    [Header("버튼 비주얼 업데이트용 (Inspector에서 연결)")]
    [Tooltip("각 축복 버튼과 BlessingData를 연결해주세요. 초기화 시 활성화 상태에 따라 비주얼이 자동 업데이트됩니다.")]
    [SerializeField] private List<BlessingButtonEntry> blessingButtons = new List<BlessingButtonEntry>();

    // TODO(조커 축복 확장 계획):
    //  - "조커" 축복을 통해 특정 라인의 제한을 +1 증가시킬 수 있음
    //  - 예: 조커 사용 시 해당 라인의 maxSelect가 2로 늘어나고, 나머지 라인들이 잠김
    //  - 구현 시:
    //    1. 조커 축복 ID 확인 (예: blessingID == "090XXX")
    //    2. 조커 활성화 시: 해당 라인의 maxSelect를 동적으로 +1, 다른 라인들의 버튼을 비활성화
    //    3. 조커 비활성화 시: 원래 maxSelect로 복구, 다른 라인들의 버튼을 다시 활성화
    //  - 현재는 기본 제한만 적용, 조커 로직은 추후 확장 예정

    private void Awake()
    {
        // activeBlessingByLine 배열 초기화
        if (activeBlessingByLine == null || activeBlessingByLine.Length != 5)
        {
            activeBlessingByLine = new string[5] { "", "", "", "", "" };
        }
        
        // 각 라인의 문자열이 null이면 빈 문자열로 초기화
        for (int i = 0; i < 5; i++)
        {
            if (activeBlessingByLine[i] == null)
            {
                activeBlessingByLine[i] = "";
            }
        }
    }

    private void OnEnable()
    {
        RefreshEssenceText();
        
        // 전투 씬에서는 축복 로드를 건너뜀 (전투 시작 시 이미 적용됨)
        // BattleManager가 존재하면 전투 씬으로 판단
        if (BattleManager.Instance == null)
        {
            // 저장된 축복 정보 로드 (월드맵/스테이지 씬에서만)
            LoadActiveBlessingsFromSave();
        }
        else
        {
            // 전투 씬에서는 BlessingManager의 activeBlessings를 그대로 사용
            // (이미 전투 시작 시 적용되었으므로 다시 로드할 필요 없음)
            Debug.Log("[BlessingPanel] 전투 씬 감지: 축복 로드를 건너뜁니다. (이미 전투 시작 시 적용됨)");
        }
        
        // 모든 버튼의 비주얼 업데이트 (활성화된 축복 반투명 처리)
        RefreshAllButtonVisuals();
    }

    /// <summary>
    /// Inspector에서 Button의 OnClick 이벤트에 연결할 수 있는 public 메서드.
    /// 버튼의 OnClick에 이 메서드를 연결하고, BlessingData ScriptableObject를 파라미터로 드래그 앤 드롭하세요.
    /// </summary>
    /// <param name="blessingData">클릭된 버튼의 축복 데이터</param>
    public void OnBlessingButtonClicked(BlessingData blessingData)
    {
        if (blessingData == null)
        {
            Debug.LogWarning("[BlessingPanel] 축복 데이터가 null입니다.");
            return;
        }

        if (BlessingManager.Instance == null)
        {
            Debug.LogWarning("[BlessingPanel] BlessingManager.Instance가 null입니다.");
            return;
        }

        string id = blessingData.blessingID;
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[BlessingPanel] 축복 ID가 설정되지 않았습니다.");
            return;
        }

        int lineIndex = blessingData.lineIndex;

        // 현재 활성화 상태 확인 (라인별 배열에서 확인)
        bool isCurrentlyActive = IsBlessingActiveInLine(id, lineIndex);

        // 비활성화하는 경우는 제한 체크 불필요 (항상 가능)
        if (!isCurrentlyActive)
        {
            // 활성화하려는 경우: 라인 제한 체크
            int maxSelect = GetMaxSelectForLine(lineIndex);
            int currentActiveCount = GetActiveCountForLine(lineIndex);

            if (currentActiveCount >= maxSelect)
            {
                Debug.LogWarning($"[BlessingPanel] 라인 {lineIndex}의 최대 선택 개수({maxSelect})에 도달했습니다.");
                return;
            }
        }

        // 축복 토글 (라인별 배열 관리)
        bool result = ToggleBlessingInLine(id, lineIndex);

        if (!result)
        {
            Debug.LogWarning($"[BlessingPanel] 축복 토글 실패: {id}");
        }
        else
        {
            // 임시 연출: 현재 클릭된 버튼의 반투명 처리/복원
            UpdateCurrentButtonVisual(id, lineIndex);
            
            // 버튼 클릭 직후 즉시 저장
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.SaveGameProgress(GameProgressManager.Instance.CurrentSlot);
                Debug.Log($"[BlessingPanel] 축복 토글 후 즉시 저장 완료: {id}");
            }
        }

        // 정수 텍스트 갱신
        RefreshEssenceText();
    }

    /// <summary>
    /// 특정 라인의 최대 선택 가능 개수를 반환합니다.
    /// </summary>
    private int GetMaxSelectForLine(int lineIndex)
    {
        switch (lineIndex)
        {
            case 0: return maxSelectLine0;
            case 1: return maxSelectLine1;
            case 2: return maxSelectLine2;
            case 3: return maxSelectLine3;
            case 4: return maxSelectLine4;
            default: return 1;
        }
    }

    /// <summary>
    /// 특정 라인에 현재 활성화된 축복의 개수를 반환합니다. (비트마스크에서 1의 개수)
    /// </summary>
    private int GetActiveCountForLine(int lineIndex)
    {
        if (lineIndex < 0 || lineIndex >= 5) return 0;
        if (string.IsNullOrEmpty(activeBlessingByLine[lineIndex])) return 0;
        
        // 비트마스크 문자열에서 '1'의 개수 세기
        int count = 0;
        foreach (char c in activeBlessingByLine[lineIndex])
        {
            if (c == '1') count++;
        }
        return count;
    }

    /// <summary>
    /// 특정 축복이 해당 라인에서 활성화되어 있는지 확인합니다. (비트마스크 방식)
    /// </summary>
    private bool IsBlessingActiveInLine(string blessingID, int lineIndex)
    {
        if (lineIndex < 0 || lineIndex >= 5) return false;
        if (string.IsNullOrEmpty(activeBlessingByLine[lineIndex])) return false;
        if (BlessingManager.Instance == null) return false;
        
        // 해당 라인의 모든 축복 가져오기 (정렬된 순서)
        var lineBlessings = BlessingManager.Instance.GetBlessingsByLine(lineIndex);
        
        // blessingID의 인덱스 찾기
        int blessingIndex = -1;
        for (int i = 0; i < lineBlessings.Count; i++)
        {
            if (lineBlessings[i].blessingID == blessingID)
            {
                blessingIndex = i;
                break;
            }
        }
        
        if (blessingIndex < 0) return false;
        
        // 비트마스크 문자열에서 해당 인덱스의 값 확인
        string bitmask = activeBlessingByLine[lineIndex];
        if (blessingIndex >= bitmask.Length) return false;
        
        return bitmask[blessingIndex] == '1';
    }

    /// <summary>
    /// 라인별 축복을 토글합니다. (라인별 비트마스크 방식)
    /// </summary>
    private bool ToggleBlessingInLine(string blessingID, int lineIndex)
    {
        if (lineIndex < 0 || lineIndex >= 5)
        {
            Debug.LogWarning($"[BlessingPanel] 잘못된 라인 인덱스: {lineIndex}");
            return false;
        }

        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("[BlessingPanel] GameProgressManager.Instance가 null입니다.");
            return false;
        }

        if (BlessingManager.Instance == null)
        {
            Debug.LogWarning("[BlessingPanel] BlessingManager.Instance가 null입니다.");
            return false;
        }

        // 해당 라인의 모든 축복 가져오기 (정렬된 순서)
        var lineBlessings = BlessingManager.Instance.GetBlessingsByLine(lineIndex);
        
        // blessingID의 인덱스 찾기
        int blessingIndex = -1;
        for (int i = 0; i < lineBlessings.Count; i++)
        {
            if (lineBlessings[i].blessingID == blessingID)
            {
                blessingIndex = i;
                break;
            }
        }
        
        if (blessingIndex < 0)
        {
            Debug.LogWarning($"[BlessingPanel] 라인 {lineIndex}에서 축복 {blessingID}를 찾을 수 없습니다.");
            return false;
        }

        // 비트마스크 문자열 초기화 (필요한 길이만큼)
        string bitmask = activeBlessingByLine[lineIndex] ?? "";
        if (bitmask.Length <= blessingIndex)
        {
            // 비트마스크 문자열을 필요한 길이만큼 확장 (0으로 채움)
            bitmask = bitmask.PadRight(blessingIndex + 1, '0');
        }

        // 현재 활성화 상태 확인
        bool isCurrentlyActive = bitmask[blessingIndex] == '1';

        if (isCurrentlyActive)
        {
            // 활성 → 비활성: 정수 1 반환
            GameProgressManager.Instance.AddEssence(1, false);
            
            // 비트마스크에서 해당 비트를 0으로 변경
            char[] bitmaskArray = bitmask.ToCharArray();
            bitmaskArray[blessingIndex] = '0';
            activeBlessingByLine[lineIndex] = new string(bitmaskArray);
            
            // BlessingManager에도 반영 (효과 적용용)
            BlessingManager.Instance.DeactivateBlessing(blessingID);
            
            Debug.Log($"[BlessingPanel] 축복 비활성화: {blessingID} (라인 {lineIndex}, 인덱스 {blessingIndex}, 정수 1 반환)");
            
            // 저장 포인트 #1: 버튼 클릭 직후 즉시 저장
            if (GameProgressManager.Instance != null)
            {
                Debug.Log($"[BlessingPanel] 저장 포인트 #1: 버튼 클릭 직후 저장 호출");
                GameProgressManager.Instance.SaveGameProgress(GameProgressManager.Instance.CurrentSlot);
                
                // 저장 후 확인
                var savedData = GameProgressManager.Instance.CurrentSaveData;
                if (savedData != null && savedData.activeBlessingByLine != null && savedData.activeBlessingByLine.Length > lineIndex)
                {
                    string savedBitmask = savedData.activeBlessingByLine[lineIndex] ?? "";
                    Debug.Log($"[BlessingPanel] 저장 확인: 라인 {lineIndex} = \"{savedBitmask}\" (현재 activeBlessingByLine = \"{activeBlessingByLine[lineIndex]}\")");
                }
            }
            
            return true;
        }
        else
        {
            // 비활성 → 활성: 정수 1 소모
            if (GameProgressManager.Instance.SpendEssence(1))
            {
                // 비트마스크에서 해당 비트를 1로 변경
                char[] bitmaskArray = bitmask.ToCharArray();
                bitmaskArray[blessingIndex] = '1';
                activeBlessingByLine[lineIndex] = new string(bitmaskArray);
                
                // BlessingManager에도 반영 (효과 적용용)
                BlessingManager.Instance.ActivateBlessing(blessingID);
                
                Debug.Log($"[BlessingPanel] 축복 활성화: {blessingID} (라인 {lineIndex}, 인덱스 {blessingIndex}, 정수 1 소모)");
                
                // 저장 포인트 #1: 버튼 클릭 직후 즉시 저장
                if (GameProgressManager.Instance != null)
                {
                    Debug.Log($"[BlessingPanel] 저장 포인트 #1: 버튼 클릭 직후 저장 호출");
                    GameProgressManager.Instance.SaveGameProgress(GameProgressManager.Instance.CurrentSlot);
                    
                    // 저장 후 확인
                    var savedData = GameProgressManager.Instance.CurrentSaveData;
                    if (savedData != null && savedData.activeBlessingByLine != null && savedData.activeBlessingByLine.Length > lineIndex)
                    {
                        string savedBitmask = savedData.activeBlessingByLine[lineIndex] ?? "";
                        Debug.Log($"[BlessingPanel] 저장 확인: 라인 {lineIndex} = \"{savedBitmask}\" (현재 activeBlessingByLine = \"{activeBlessingByLine[lineIndex]}\")");
                    }
                }
                
                return true;
            }
            
            Debug.LogWarning("[BlessingPanel] 강자의 정수가 부족합니다. (필요: 1)");
            return false;
        }
    }

    /// <summary>
    /// GameProgressManager에서 저장된 축복 정보를 로드합니다. (비트마스크 방식)
    /// </summary>
    private void LoadActiveBlessingsFromSave()
    {
        if (GameProgressManager.Instance == null) return;
        if (BlessingManager.Instance == null) return;

        var saveData = GameProgressManager.Instance.CurrentSaveData;
        if (saveData != null && saveData.activeBlessingByLine != null && saveData.activeBlessingByLine.Length == 5)
        {
            // 저장된 라인별 비트마스크 문자열 복사
            for (int i = 0; i < 5; i++)
            {
                activeBlessingByLine[i] = saveData.activeBlessingByLine[i] ?? "";
                
                // 비트마스크를 파싱하여 BlessingManager에도 반영 (효과 적용용)
                if (!string.IsNullOrEmpty(activeBlessingByLine[i]))
                {
                    var lineBlessings = BlessingManager.Instance.GetBlessingsByLine(i);
                    string bitmask = activeBlessingByLine[i];
                    
                    for (int j = 0; j < bitmask.Length && j < lineBlessings.Count; j++)
                    {
                        if (bitmask[j] == '1')
                        {
                            string blessingID = lineBlessings[j].blessingID;
                            if (!string.IsNullOrEmpty(blessingID))
                            {
                                BlessingManager.Instance.ActivateBlessing(blessingID);
                            }
                        }
                    }
                }
            }
            
            Debug.Log($"[BlessingPanel] 저장된 축복 정보 로드 완료 (비트마스크 방식)");
        }
    }

    /// <summary>
    /// 라인별 활성화된 축복 정보를 반환합니다. (GameProgressManager에서 저장 시 사용)
    /// </summary>
    public string[] GetActiveBlessingByLine()
    {
        return activeBlessingByLine;
    }

    /// <summary>
    /// 정수 텍스트를 갱신합니다.
    /// </summary>
    private void RefreshEssenceText()
    {
        if (GameProgressManager.Instance != null && essenceText != null)
        {
            int essence = GameProgressManager.Instance.GetEssence();
            essenceText.text = essence.ToString();
        }
    }

    /// <summary>
    /// 현재 클릭된 버튼의 비주얼을 갱신합니다. (활성화 시 반투명 처리)
    /// </summary>
    /// <param name="blessingId">토글된 축복 ID</param>
    /// <param name="lineIndex">축복이 속한 라인 인덱스</param>
    private void UpdateCurrentButtonVisual(string blessingId, int lineIndex)
    {
        if (EventSystem.current == null) return;

        GameObject currentButtonObj = EventSystem.current.currentSelectedGameObject;
        if (currentButtonObj == null) return;

        UpdateButtonVisual(currentButtonObj, blessingId, lineIndex);
    }

    /// <summary>
    /// 특정 버튼의 비주얼을 업데이트합니다. (활성화 시 반투명 처리)
    /// </summary>
    /// <param name="buttonObj">버튼 GameObject</param>
    /// <param name="blessingId">축복 ID</param>
    /// <param name="lineIndex">축복이 속한 라인 인덱스</param>
    private void UpdateButtonVisual(GameObject buttonObj, string blessingId, int lineIndex)
    {
        if (buttonObj == null) return;

        // 버튼에 연결된 축복이 현재 활성 상태인지 확인 (라인별 배열에서 확인)
        bool isActive = IsBlessingActiveInLine(blessingId, lineIndex);

        // 기본 이미지(alpha만 조정)
        Image image = buttonObj.GetComponent<Image>();
        if (image == null)
        {
            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                image = btn.targetGraphic as Image;
            }
        }

        if (image != null)
        {
            Color c = image.color;
            c.a = isActive ? 0.5f : 1f; // 활성: 반투명, 비활성: 원래 불투명
            image.color = c;
        }
    }

    /// <summary>
    /// 모든 버튼의 비주얼을 업데이트합니다. (OnEnable에서 호출)
    /// </summary>
    private void RefreshAllButtonVisuals()
    {
        if (blessingButtons == null || blessingButtons.Count == 0)
        {
            Debug.LogWarning("[BlessingPanel] blessingButtons 리스트가 비어있습니다. Inspector에서 버튼과 BlessingData를 연결해주세요.");
            return;
        }

        // 전투 씬에서는 BlessingManager의 activeBlessings를 기반으로 비주얼 업데이트
        bool isBattleScene = BattleManager.Instance != null;
        
        foreach (var entry in blessingButtons)
        {
            if (entry == null || entry.button == null || entry.blessingData == null) continue;

            bool isActive;
            if (isBattleScene)
            {
                // 전투 씬: BlessingManager의 activeBlessings 확인
                if (BlessingManager.Instance != null)
                {
                    var activeBlessings = BlessingManager.Instance.GetAllActiveBlessings();
                    isActive = activeBlessings != null && activeBlessings.ContainsKey(entry.blessingData.blessingID) && activeBlessings[entry.blessingData.blessingID] > 0;
                }
                else
                {
                    isActive = false;
                }
            }
            else
            {
                // 월드맵/스테이지 씬: activeBlessingByLine 배열 확인
                isActive = IsBlessingActiveInLine(entry.blessingData.blessingID, entry.blessingData.lineIndex);
            }

            // 비주얼 업데이트
            Image image = null;
            if (entry.button.targetGraphic != null)
            {
                image = entry.button.targetGraphic as Image;
            }
            else
            {
                image = entry.button.GetComponent<Image>();
                if (image == null)
                {
                    image = entry.button.GetComponentInChildren<Image>();
                }
            }

            if (image != null)
            {
                Color c = image.color;
                c.a = isActive ? 0.5f : 1f; // 활성: 반투명, 비활성: 원래 불투명
                image.color = c;
            }
        }
    }
}
