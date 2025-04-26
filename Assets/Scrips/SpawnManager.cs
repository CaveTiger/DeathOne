using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    public GameObject characterPrefab;
    public CharacterLoader loader; // XML 파싱하는 컴포넌트
    public CharacterData data;

    [Header("플레이어")]
    public string playerID = "000001";  // 항상 등장하는 플레이어

    [Header("아군 유닛")]
    public List<string> allyIDs = new List<string>();

    [Header("저장받은 적")]
    public List<string> enemyIDs = new List<string>();

    public GameObject enemyPrefab;   // 기본 적 프리팹
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 방지
            return;
        }
        Instance = this;
    }

}
