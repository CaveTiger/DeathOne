using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("플레이어 슬롯")]
    public Transform playerSlot;

    [Header("아군 슬롯")]
    public Transform[] allySlots;

    [Header("작성될 적 목록")]
    public Transform[] enemySlots;

    public List<CharacterStats> allCharacters; // 전투에 참여하는 모든 캐릭터
    public Transform skillSlotParent; // 스킬 슬롯 UI 부모 오브젝트
    public GameObject skillInstancePrefab; // SkillInstance 프리팹
    public Transform skillSetRoot; // SkillSetRoot 오브젝트
    public GameObject skillButtonPrefab; // 스킬 버튼 프리팹

    //얜 싱글톤을 하지 않는다. 배매는 오로지 실행자 역할만을 맡는다.
    void Start()
    {
        StartBattle();
    }

    void SpawnAllUnits()
    {
        var spawn = SpawnManager.Instance;
        var dict = CharacterData.characterDict;

        // 1. 플레이어 유닛
        if (playerSlot == null)
        {
            Debug.LogError("[Spawn] playerSlot이 할당되지 않았습니다!");
        }
        else if (!CharacterData.characterDict.ContainsKey(spawn.playerID))
        {
            Debug.LogError($"[Spawn] 캐릭터 딕셔너리에 {spawn.playerID} 없음");
        }
        else
        {
            Debug.Log("[Spawn] 플레이어 생성 시도 중");
            SpawnUnit(spawn.playerID, playerSlot);
        }

        // 2. 아군 유닛
        for (int i = 0; i < spawn.allyIDs.Count && i < allySlots.Length; i++)
        {
            SpawnUnit(spawn.allyIDs[i], allySlots[i]);
        }

        // 3. 적군 유닛
        for (int i = 0; i < spawn.enemyIDs.Count && i < enemySlots.Length; i++)
        {
            SpawnUnitEnemy(spawn.enemyIDs[i], enemySlots[i]);
        }
        Debug.Log($"[Spawn] 플레이어 ID: {spawn.playerID}");
    }

    void SpawnUnit(string id, Transform slot)
    {
        if (!CharacterData.characterDict.TryGetValue(id, out var data))
        {
            Debug.LogError($"[Spawn] ID {id} 에 해당하는 캐릭터 데이터 없음.");
            return;
        }

        GameObject obj = Instantiate(SpawnManager.Instance.characterPrefab, slot.position, Quaternion.identity);
        obj.name = $"Unit_{id}";
        obj.transform.SetParent(slot);
        obj.transform.localPosition = Vector3.zero;

        var unit = obj.GetComponent<CharacterStats>();
        if (unit != null)
        {
            unit.SetData(data);
            obj.transform.localScale = Vector3.one * data.Scale;

            // ★ 체력 보존 적용
            if (StageSetting.Instance != null)
            {
                int savedHp = StageSetting.Instance.inStageData.GetHP(id);
                if (savedHp > 0)
                {
                    unit.Hp = savedHp;
                }
            }

            // allCharacters 리스트에 추가
            allCharacters.Add(unit);
            Debug.Log($"[Spawn] 플레이어/아군 추가됨: {unit.Label} (총 {allCharacters.Count}명)");
        }
        else
        {
            Debug.LogWarning("CharacterStats 컴포넌트가 프리팹에 없습니다.");
        }
    }

    void SpawnUnitEnemy(string id, Transform slot)
    {
        if (!CharacterData.characterDict.TryGetValue(id, out var data))
        {
            Debug.LogError($"[Spawn] ID {id} 에 해당하는 캐릭터 데이터 없음.");
            return;
        }

        GameObject obj = Instantiate(SpawnManager.Instance.characterPrefab, slot.position, Quaternion.identity);
        obj.name = $"Unit_{id}";
        obj.transform.SetParent(slot);
        obj.transform.localPosition = Vector3.zero;
        obj.tag = "Enemy";

        var sprite = obj.transform.Find("Sprite");
        if (sprite != null)
            sprite.gameObject.tag = "Enemy";

        if (obj.GetComponent<EnemyAIController>() == null)
        {
            obj.AddComponent<EnemyAIController>();
        }

        var unit = obj.GetComponent<CharacterStats>();
        if (unit != null)
        {
            unit.IsPlayer = false;
            unit.SetData(data);
            obj.transform.localScale = Vector3.one * data.Scale;

            // allCharacters 리스트에 추가
            allCharacters.Add(unit);
            Debug.Log($"[Spawn] 적군 추가됨: {unit.Label} (총 {allCharacters.Count}명)");
        }
        else
        {
            Debug.LogWarning("CharacterStats 컴포넌트가 프리팹에 없습니다.");
        }
    }

    public void StartBattle()
    {
        // allCharacters 리스트 초기화
        allCharacters.Clear();
        
        Debug.Log($"[Check] 캐릭터 딕셔너리 Count: {CharacterData.characterDict.Count}");
        SpawnAllUnits();

        // 스킬 버튼 UI 생성
        CreateAllSkillButtons(allCharacters);

        Debug.Log($"이게 전투 시작부분  SkillDict 현재 등록된 스킬 수: {SkillData.skillDict.Count}");

        foreach (var kvp in SkillData.skillDict)
        {
            Debug.Log($"SkillDict Key: {kvp.Key} / Skill 이름: {kvp.Value.Name}");
        }
    }

    public void SavePartyStatusToStageSetting()
    {
        // 플레이어 및 아군만 저장
        foreach (var character in allCharacters)
        {
            if (character.IsPlayer) // 플레이어/아군만
            {
                StageSetting.Instance.inStageData.AddOrUpdate(
                    character.Label, // id
                    character.Hp,
                    character.MaxHp
                );
            }
        }
        Debug.Log("[BattleManager] 파티원 HP 정보 StageSetting에 저장 완료");
    }

    public void EndBattle(bool isVictory)
    {
        if (isVictory)
        {
            // 전투 승리 시 체력 데이터 저장
            SavePartyStatusToStageSetting();
            
            // 스테이지 블록 클리어 처리
            if (SpawnManager.Instance != null && !string.IsNullOrEmpty(SpawnManager.Instance.currentBlockID))
            {
                StageManager.Instance.SetBlockCleared(StageManager.Instance.SelectedStageID, SpawnManager.Instance.currentBlockID);
            }
        }
        
        // 전투 종료 후 씬 전환
        UnityEngine.SceneManagement.SceneManager.LoadScene("WorldMap");
    }

    public void CreateAllSkillButtons(List<CharacterStats> partyMembers)
    {
        // [진단용] skillSetRoot가 할당되었는지 확인
        if (skillSetRoot == null)
        {
            Debug.LogError("[SkillGen-Error] 'Skill Set Root' 변수가 비어있습니다! BattleManager 인스펙터에서 할당해야 합니다.");
            return;
        }
        
        Debug.Log($"[SkillGen] 파티원 수: {partyMembers.Count}");
        for (int slotIdx = 0; slotIdx < 4; slotIdx++)
        {
            Transform slot = skillSetRoot.Find($"SkillSlot{slotIdx+1}");
            if (slot == null)
            {
                Debug.LogWarning($"[SkillGen] SkillSlot{slotIdx+1}를 찾을 수 없습니다.");
                continue;
            }

            // 기존 버튼 삭제(필요시)
            foreach (Transform child in slot) Destroy(child.gameObject);

            foreach (var character in partyMembers)
            {
                // 플레이어/아군만 스킬 버튼 생성
                if (!character.IsPlayer) continue;

                // [진단용 로그] 캐릭터의 전체 스킬 목록을 출력
                Debug.Log($"[SkillGen-Check] 캐릭터: {character.Label}, 스킬 목록: [{string.Join(", ", character.Skills)}]");
                
                string skillId = character.Skills[slotIdx];
                Debug.Log($"[SkillGen] 캐릭터: {character.Label}, 슬롯: {slotIdx}, 스킬ID: {skillId}");
                if (!string.IsNullOrEmpty(skillId) && SkillData.skillDict.TryGetValue(skillId, out var skillData))
                {
                    GameObject btn = Instantiate(skillButtonPrefab, slot);
                    var skillInstance = btn.GetComponent<SkillInstance>();
                    skillInstance.SetSkillData(skillData);
                    skillInstance.SetCaster(character);
                    skillInstance.SetSlotIndex(slotIdx);
                    btn.SetActive(true);
                    Debug.Log($"[SkillGen] 버튼 생성 완료: {character.Label} - {skillData.Name}");
                }
            }
        }
    }
}
