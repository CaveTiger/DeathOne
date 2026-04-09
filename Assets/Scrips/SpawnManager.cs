using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using System.Linq;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    public GameObject characterPrefab;
    public CharacterLoader loader; // XML 파싱하는 컴포넌트
    public CharacterData data;

    [Header("플레이어")]
    public string playerID = "000001";  // 항상 등장하는 플레이어

    // 슬롯 기반 단방향 전달: 스킬ID 배열 저장용
    public string[] partySkillIDs = new string[4] { "", "", "", "" };

    [Header("아군 유닛")]
    public List<string> allyIDs = new List<string>();
    public List<CharacterData> allyPartyData = new List<CharacterData>(); // 파티 슬롯에서 넘겨받은 캐릭터 데이터

    [Header("저장받은 적")]
    public List<string> enemyIDs = new List<string>();

    public GameObject enemyPrefab;   // 기본 적 프리팹

    public string currentBlockID; // 현재 전투 중인 블록 ID

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 슬롯 기반 단방향 전달: 현재 파티 정보를 디버그 출력
    /// </summary>
    public void LogPartyData(string context)
    {
        // 파티 정보 디버그 출력 (필요시 주석 해제)
    }
}
