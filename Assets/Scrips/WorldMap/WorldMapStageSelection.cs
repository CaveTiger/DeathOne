using UnityEngine;

public class WorldMapStageSelection : MonoBehaviour
{
    private Renderer rend;
    private Color originalColor;
    private int originalLayer;
    private static bool isAnyUIOpen = false;  // UI 열림 상태 추적

    public Color hoverColor = new Color(1f, 1f, 0.6f);
    public Color clickColor = Color.red;

    public GameObject stageStarterUI;
    [SerializeField] private StageCameraUI stageCameraUI;

    [Header("이 오브젝트에 대응하는 스테이지 ID")]
    public string stageID;

    [Header("진입 조건 (선택)")]
    [Tooltip("비어있으면 조건 없음. 값이 있으면 이 스테이지의 클리어 여부를 조건으로 사용.")]
    [SerializeField] private string conditionStageId = "";
    [Tooltip("끄면: conditionStageId 클리어 시 진입 가능. 켜면: conditionStageId 미클리어 시 진입 가능.")]
    [SerializeField] private bool allowWhenConditionNotCleared = false;

    [Header("디버그: 클리어 여부 표시")]
    [SerializeField] private bool isCleared;
    [SerializeField] private bool isEntryLockedByCondition;
    [SerializeField] private bool isEntryLockedByProgress;

    private void Start()
    {
        // 씬 복귀 시 static 플래그가 남아 월드맵 클릭이 막히는 것을 방지
        SetUIOpen(false);

        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
        originalLayer = gameObject.layer;

        // StageCameraUI가 없으면 찾아서 할당
        if (stageCameraUI == null)
            stageCameraUI = FindFirstObjectByType<StageCameraUI>();

        // Inspector에 클리어 여부 표시
        isCleared = IsStageCleared();
        isEntryLockedByCondition = IsEntryLockedByCondition();
        isEntryLockedByProgress = IsEntryLockedByProgress();

        // 클리어되었거나 진입 조건으로 잠긴 스테이지는 어둡게 표시
        if ((isCleared || isEntryLockedByCondition || isEntryLockedByProgress) && rend != null)
        {
            rend.material.color = Color.gray;
        }
    }

    private void OnMouseEnter()
    {
        if (ShouldBlockWorldMapInteraction()) return;
        if (IsEntryLockedByProgress()) return;
        if (IsEntryLockedByCondition()) return;
        rend.material.color = hoverColor;
    }

    private void OnMouseExit()
    {
        if (isCleared || IsEntryLockedByProgress() || IsEntryLockedByCondition())
            rend.material.color = Color.gray;
        else
            rend.material.color = originalColor;
    }

    void OnMouseDown()
    {
        if (ShouldBlockWorldMapInteraction()) return;
        if (ShouldBlockByEntryCondition()) return;
        
        rend.material.color = clickColor;
        Debug.Log($"스테이지 클릭됨: {stageID}");
        StageManager.Instance.SelectStage(stageID);

        if (stageCameraUI != null)
            stageCameraUI.OnStageIconClick(transform.position);
    }

    private void OnMouseUp()
    {
        // 다른 UI가 열려 있을 때는 스테이지 선택 처리 무시
        if (ShouldBlockWorldMapInteraction()) return;
        if (ShouldBlockByEntryCondition()) return;

        if (stageStarterUI == null)
        {
            Debug.LogWarning("stageStarterUI가 연결되지 않았습니다!");
            return;
        }
        stageStarterUI.GetComponentInChildren<WorldMapReturnButton>().targetStageSelection = this;
        rend.material.color = hoverColor;
        Debug.Log($"스테이지 클릭 완수: {stageID}");

        bool isOpen = stageStarterUI.activeSelf;

        // 스테이지 데이터 가져오기
        var stage = StageManager.Instance.GetStage(stageID);

        if (isOpen)
        {
            // UI 닫힐 때
            gameObject.layer = originalLayer;
            SetUIOpen(false);
            if (GameManager.Instance != null)
                GameManager.Instance.SetCurrentScreenStateByValue((int)GameManager.ScreenState.WorldMap);

            // 딕셔너리에서 제거
            if (stage != null)
                StageManager.Instance.UnregisterStageBlocks(stage.BlockIDs);
        }
        else
        {
            // UI 열릴 때
            gameObject.layer = LayerMask.NameToLayer("UI");
            SetUIOpen(true);
            if (GameManager.Instance != null)
                GameManager.Instance.SetCurrentScreenStateByValue((int)GameManager.ScreenState.StageSelected);

            // 혹시 이전 스테이지 블록이 남아있다면 정리
            StageManager.Instance.ClearAllBlockStates();

            // 딕셔너리에 등록
            if (stage != null)
                StageManager.Instance.RegisterStageBlocks(stage.BlockIDs);
        }

        stageStarterUI.SetActive(!isOpen);
        Debug.Log(isOpen ? "UI 닫힘" : "UI 열림");
    }

    /// <summary>
    /// UI 열림 상태를 설정하고 월드맵 상호작용을 제어합니다.
    /// </summary>
    /// <param name="isOpen">UI가 열려있는지 여부</param>
    public static void SetUIOpen(bool isOpen)
    {
        isAnyUIOpen = isOpen;
        SetAllStageButtonColliders(!isOpen); // UI가 열려있으면 Collider 비활성화
        
        // WorldMapRoot가 있다면 활성화/비활성화 처리
        WorldMapRoot root = FindFirstObjectByType<WorldMapRoot>();
        if (root != null)
        {
            root.gameObject.SetActive(!isOpen);
        }
    }

    // 모든 Stage 버튼의 Collider를 일괄로 켜거나 끄는 static 메서드
    public static void SetAllStageButtonColliders(bool enabled)
    {
        foreach (var btn in FindObjectsByType<WorldMapStageSelection>(FindObjectsSortMode.None))
        {
            var col = btn.GetComponent<Collider2D>();
            if (col != null) col.enabled = enabled;
            
            // 3D Collider도 처리
            var col3D = btn.GetComponent<Collider>();
            if (col3D != null) col3D.enabled = enabled;
        }
    }

    void Update()
    {
        if (isAnyUIOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToWorldMap();
        }
    }

    public void ReturnToWorldMap()
    {
        // 카메라 줌아웃
        if (stageCameraUI != null)
            stageCameraUI.ResetCamera();

        // 선택 해제
        StageManager.Instance.ClearSelectedStage();

        // UI 닫기
        stageStarterUI.SetActive(false);

        // 월드맵 상태 복구
        SetUIOpen(false);
        if (GameManager.Instance != null)
            GameManager.Instance.SetCurrentScreenStateByValue((int)GameManager.ScreenState.WorldMap);
    }

    /// <summary>
    /// 이 스테이지가 클리어되었는지 영구적으로 확인
    /// </summary>
    public bool IsStageCleared()
    {
        // StageManager의 stageProgressDict에서 클리어 여부 확인
        if (StageManager.Instance == null || string.IsNullOrEmpty(stageID)) return false;
        if (StageManager.Instance.stageProgressDict != null && StageManager.Instance.stageProgressDict.TryGetValue(stageID, out var progress))
        {
            return progress.isCleared;
        }
        return false;
    }

    /// <summary>
    /// 월드맵 상호작용 차단 조건.
    /// - 현재 화면 상태가 WorldMap이 아닐 때는 반드시 차단
    /// - 기존 UI 열림 플래그도 함께 고려
    /// </summary>
    private bool ShouldBlockWorldMapInteraction()
    {
        if (GameManager.Instance == null) return true;
        if (!GameManager.Instance.IsCurrentScreenState(GameManager.ScreenState.WorldMap)) return true;
        if (isAnyUIOpen) return true;
        return false;
    }

    /// <summary>
    /// 인스펙터 조건 필드(conditionStageId) 기반 진입 차단.
    /// </summary>
    private bool ShouldBlockByEntryCondition()
    {
        if (IsEntryLockedByProgress())
        {
            Debug.Log($"[WorldMapStageSelection] 진행도 조건 미충족(잠금): stage={stageID}");
            return true;
        }

        if (!IsEntryLockedByCondition()) return false;

        string conditionText = allowWhenConditionNotCleared ? "미클리어일 때만 진입 가능" : "클리어 시 진입 가능";
        Debug.Log($"[WorldMapStageSelection] 진입 조건 미충족: stage={stageID}, condition={conditionStageId}, rule={conditionText}");
        return true;
    }

    private bool IsEntryLockedByCondition()
    {
        if (string.IsNullOrWhiteSpace(conditionStageId)) return false;
        if (StageManager.Instance == null) return false;

        bool isConditionCleared = StageManager.Instance.IsStageCleared(conditionStageId);
        bool allow = allowWhenConditionNotCleared ? !isConditionCleared : isConditionCleared;
        isEntryLockedByCondition = !allow;
        return isEntryLockedByCondition;
    }

    private bool IsEntryLockedByProgress()
    {
        if (StageManager.Instance == null || string.IsNullOrWhiteSpace(stageID))
        {
            isEntryLockedByProgress = false;
            return false;
        }

        bool canEnter = StageManager.Instance.CanEnterStage(stageID);
        isEntryLockedByProgress = !canEnter;
        return isEntryLockedByProgress;
    }
}
