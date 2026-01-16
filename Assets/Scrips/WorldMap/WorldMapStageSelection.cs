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

    [Header("디버그: 클리어 여부 표시")]
    [SerializeField] private bool isCleared;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
        originalLayer = gameObject.layer;

        // StageCameraUI가 없으면 찾아서 할당
        if (stageCameraUI == null)
            stageCameraUI = FindFirstObjectByType<StageCameraUI>();

        // Inspector에 클리어 여부 표시
        isCleared = IsStageCleared();

        // 클리어된 스테이지는 회색으로 표시 (임시처리)
        if (isCleared && rend != null)
        {
            rend.material.color = Color.gray;
        }
    }

    private void OnMouseEnter()
    {
        if (isAnyUIOpen) return;
        rend.material.color = hoverColor;
    }

    private void OnMouseExit()
    {
        rend.material.color = originalColor;
    }

    void OnMouseDown()
    {
        if (isAnyUIOpen) return;
        
        rend.material.color = clickColor;
        Debug.Log($"스테이지 클릭됨: {stageID}");
        StageManager.Instance.SelectStage(stageID);

        if (stageCameraUI != null)
            stageCameraUI.OnStageIconClick(transform.position);
    }

    private void OnMouseUp()
    {
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
            SetAllStageButtonColliders(true);
            gameObject.layer = originalLayer;
            isAnyUIOpen = false;

            // 딕셔너리에서 제거
            if (stage != null)
                StageManager.Instance.UnregisterStageBlocks(stage.BlockIDs);
        }
        else
        {
            // UI 열릴 때
            SetAllStageButtonColliders(false);
            gameObject.layer = LayerMask.NameToLayer("UI");
            isAnyUIOpen = true;

            // 혹시 이전 스테이지 블록이 남아있다면 정리
            StageManager.Instance.ClearAllBlockStates();

            // 딕셔너리에 등록
            if (stage != null)
                StageManager.Instance.RegisterStageBlocks(stage.BlockIDs);
        }

        stageStarterUI.SetActive(!isOpen);
        Debug.Log(isOpen ? "UI 닫힘" : "UI 열림");
    }

    // 모든 Stage 버튼의 Collider를 일괄로 켜거나 끄는 static 메서드 추가
    public static void SetAllStageButtonColliders(bool enabled)
    {
        foreach (var btn in FindObjectsByType<WorldMapStageSelection>(FindObjectsSortMode.None))
        {
            var col = btn.GetComponent<Collider2D>();
            if (col != null) col.enabled = enabled;
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

        // 월드맵 버튼 Collider 다시 활성화
        WorldMapStageSelection.SetAllStageButtonColliders(true);
        isAnyUIOpen = false;
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
}
