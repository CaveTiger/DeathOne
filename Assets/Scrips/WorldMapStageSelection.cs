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
        rend.material.color = hoverColor; //���콺�� ��ġ�� ��
    }
    private void OnMouseExit()
    {
        rend.material.color = originalColor; //���콺�� ��ġ�� ��������
    }

    void OnMouseDown()
    {
        rend.material.color = clickColor;
        Debug.Log($"�������� Ŭ����: {stageID}");
        // �� �̵� or ���� ���� ǥ�� ��
    }
    private void OnMouseUp()
    {
        rend.material.color = hoverColor; // Ŭ�� �� �ٽ� hover ����
        Debug.Log($"�������� Ŭ�� �ϼ�: {stageID}");
        
        if (stageStarterUI == null) //����������Ÿ�Ͱ� ���� ��� ���� ����
        {
            Debug.LogWarning("stageStarterUI�� ������� �ʾҽ��ϴ�!");
            return;
        }
        else if (stageStarterUI.activeSelf)
        {
            stageStarterUI.SetActive(false);
            Debug.Log("UI ����");
        }
        else if (stageStarterUI != null) //����������Ÿ�Ͱ� ���� �ƴ�
        {
            stageStarterUI.SetActive(true); // �� ���⼭ UI Ȱ��ȭ
        }
        
        else
        {
            Debug.Log("���������ʴ�.");
        }
        }
}
