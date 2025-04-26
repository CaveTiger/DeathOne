using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    public GameObject characterPrefab;
    public CharacterLoader loader; // XML �Ľ��ϴ� ������Ʈ
    public CharacterData data;

    [Header("�÷��̾�")]
    public string playerID = "000001";  // �׻� �����ϴ� �÷��̾�

    [Header("�Ʊ� ����")]
    public List<string> allyIDs = new List<string>();

    [Header("������� ��")]
    public List<string> enemyIDs = new List<string>();

    public GameObject enemyPrefab;   // �⺻ �� ������
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // �ߺ� ����
            return;
        }
        Instance = this;
    }

}
