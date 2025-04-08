using UnityEngine;

public class WorldMapStageSelection : MonoBehaviour
{
    private Renderer rend;
    private Color originalColor;

    public Color hoverColor = new Color(1f, 1f, 0.6f);
    public Color clickColor = Color.red;

    public GameObject stageStarterUI;

    public string stageID;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }
    private void OnMouseEnter()
    {
        rend.material.color = hoverColor; //마우스가 위치에 들어감
    }
    private void OnMouseExit()
    {
        rend.material.color = originalColor; //마우스가 위치를 빠져나옴
    }

    void OnMouseDown()
    {
        rend.material.color = clickColor;
        Debug.Log($"스테이지 클릭됨: {stageID}");
        // 씬 이동 or 세부 정보 표시 등
    }
    private void OnMouseUp()
    {
        rend.material.color = hoverColor; // 클릭 후 다시 hover 상태
        Debug.Log($"스테이지 클릭 완수: {stageID}");
        
        if (stageStarterUI == null) //스테이지스타터가 없음 경고를 위한 이프
        {
            Debug.LogWarning("stageStarterUI가 연결되지 않았습니다!");
            return;
        }
        else if (stageStarterUI.activeSelf)
        {
            stageStarterUI.SetActive(false);
            Debug.Log("UI 닫음");
        }
        else if (stageStarterUI != null) //스테이지스타터가 널이 아님
        {
            stageStarterUI.SetActive(true); // ← 여기서 UI 활성화
        }
        
        else
        {
            Debug.Log("열려있지않다.");
        }
        }
}
