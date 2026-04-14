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

    [Header("플레이어 스킬 슬롯(분할 필드)")]
    public string playerSkillSlot1 = "";
    public string playerSkillSlot2 = "";
    public string playerSkillSlot3 = "";
    public string playerSkillSlot4 = "";

    // 하위 호환용 배열(디버그/레거시 참조). 분할 필드와 항상 동기화한다.
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

    public void SetPartySkillIDs(string[] skillIDs)
    {
        if (skillIDs == null || skillIDs.Length < 4)
            return;

        playerSkillSlot1 = skillIDs[0] ?? "";
        playerSkillSlot2 = skillIDs[1] ?? "";
        playerSkillSlot3 = skillIDs[2] ?? "";
        playerSkillSlot4 = skillIDs[3] ?? "";
        SyncArrayFromSlots();
    }

    public string[] GetPartySkillIDs()
    {
        SyncArrayFromSlots();
        return (string[])partySkillIDs.Clone();
    }

    private void SyncArrayFromSlots()
    {
        if (partySkillIDs == null || partySkillIDs.Length != 4)
            partySkillIDs = new string[4] { "", "", "", "" };
        partySkillIDs[0] = playerSkillSlot1 ?? "";
        partySkillIDs[1] = playerSkillSlot2 ?? "";
        partySkillIDs[2] = playerSkillSlot3 ?? "";
        partySkillIDs[3] = playerSkillSlot4 ?? "";
    }
}
