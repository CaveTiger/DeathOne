using UnityEngine;

/// <summary>
/// 월드맵의 상위 루트에 붙일 단순 스크립트.
/// - 월드맵 버튼(스테이지 아이콘)들의 부모 오브젝트에 붙여 사용
/// - 추후 월드맵 공통 기능이 필요할 때 여기서 확장
/// </summary>
public class WorldMapRoot : MonoBehaviour
{
    [Header("월드맵 설명 (기능은 아직 없음)")]
    [TextArea]
    [SerializeField] private string description =
        "월드맵 관련 공통 기능을 모을 루트 스크립트.\n" +
        "- 이 오브젝트를 월드맵 버튼들의 상위(Parent)에 두고 사용하세요.\n" +
        "- 나중에 월드맵 전용 상태 관리나 연출이 필요하면 여기서 확장하면 됩니다.";

    /// <summary>
    /// 월드맵 루트 오브젝트를 비활성화합니다. (버튼 연결용)
    /// </summary>
    public void CloseWorldMap()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 월드맵 루트 오브젝트를 활성화합니다. (버튼 연결용)
    /// </summary>
    public void OpenWorldMap()
    {
        gameObject.SetActive(true);
    }
}


