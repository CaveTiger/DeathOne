using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageBlockSelection : MonoBehaviour
{
    private Renderer rend;
    private Color originalColor;

    public Color hoverColor = new Color(1f, 1f, 0.6f);
    public Color clickColor = Color.red;

    public GameObject StageBlock;

    public string blockType;

    [Header("�� ������Ʈ�� �����ϴ� ��� ID")]
    public string blockID;

    [Header("�� ���� ����� �� ���")]
    public List<string> enemyID = new List<string>();

    [Header("���� �ƽ�")]
    public string frontCutID;
    public string backCutID;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;

        //�����ǰ� �˸´� ������ �ֱ�
        if (string.IsNullOrEmpty(blockID)) return;
        //ID�� ������ ���� �����͸� �ҷ��´�.
        if (StageManager.Instance.stageBlockDict.TryGetValue(blockID, out var data))
        {
            blockType = data.BlockType;
            enemyID = new List<string>(data.EnemyIDs);
            frontCutID = data.FrontCutID;
            backCutID = data.BackCutID;

            Debug.Log($"[{blockID}] ��� ������ ���� �Ϸ�");
        }
        else
        {
            Debug.LogWarning($"[{blockID}] �� �ش��ϴ� ��� �����͸� ã�� �� �����ϴ�.");
        }
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
        Debug.Log($"����������� Ŭ����: {blockID}");
        GetInstance().GetBlock(blockID);
    }

    private static StageManager GetInstance()
    {
        return StageManager.Instance;
    }

    private void OnMouseUp()
    {
        rend.material.color = hoverColor; // Ŭ�� �� �ٽ� hover ����
        Debug.Log($"����������� Ŭ�� �ϼ�: {blockID}");

        if (blockID != null)
        {
            SpawnManager.Instance.enemyIDs = this.enemyID;
            SceneManager.LoadScene("TestBattle");
        }
        else
        {
            Debug.LogWarning("�������� ID�� ������ ����");
        }
    }
}
