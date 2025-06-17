using UnityEngine;

public class WorldMapReturnButton : MonoBehaviour
{
    [SerializeField] public WorldMapStageSelection targetStageSelection;

    /// <summary>
    /// UI 버튼에서 호출: 월드맵 복귀 기능 실행
    /// </summary>
    public void OnReturnToWorldMapButton()
    {
        if (targetStageSelection != null)
        {
            targetStageSelection.ReturnToWorldMap();
        }
        else
        {
            Debug.LogWarning("WorldMapStageSelection 참조가 연결되지 않았습니다.");
        }
    }
} 