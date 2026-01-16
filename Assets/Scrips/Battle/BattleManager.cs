using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using System;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

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

    // 전투 결과 데이터
    public static BattleResultData LastBattleResult { get; set; }

    // 스테이지 보상 데이터
    public static StageRewardData CurrentStageReward { get; private set; }

    private List<string> rewardTargetEnemyIDs = new List<string>();

    // 싱글톤을 하지 않는다. 배매는 오로지 실행자 역할만을 맡는다.
    void Start()
    {
        StartBattle();
    }

    void SpawnAllUnits()
    {
        var spawn = SpawnManager.Instance;
        
        for (int i = 0; i < spawn.allyPartyData.Count && i < 1 + allySlots.Length; i++)
        {
            Transform targetSlot = (i == 0) ? playerSlot : allySlots[i - 1];
            
            // 1번 슬롯은 주인공이 아닐 때만 플립, 나머지는 모두 플립
            bool flipX;
            if (i == 0)
            {
                // 1번 슬롯: 주인공(000001)이 아니면 플립
                flipX = (spawn.allyPartyData[i].ID != "000001");
            }
            else
            {
                // 2~4번 슬롯: 모두 플립
                flipX = true;
            }
            
            if (targetSlot != null)
            {
                string[] skillIDs = (i == 0) ? spawn.partySkillIDs : null;
                GameObject obj = Instantiate(SpawnManager.Instance.characterPrefab, targetSlot.position, Quaternion.identity);
                obj.name = $"Unit_{spawn.allyPartyData[i].ID}";
                obj.transform.SetParent(targetSlot);
                obj.transform.localPosition = Vector3.zero;

                var unit = obj.GetComponent<CharacterStats>();
                if (unit != null)
                {
                    unit.SetData(spawn.allyPartyData[i]);
                    obj.transform.localScale = Vector3.one * spawn.allyPartyData[i].Scale;
                    // 플레이어(0번)라면 슬롯 UI에서 선택한 스킬ID를 복사
                    if (i == 0 && skillIDs != null && skillIDs.Length == 4)
                    {
                        unit.Skills = (string[])skillIDs.Clone();
                    }
                    // flipX 적용 (아군만)
                    var spriteTr = obj.transform.Find("Sprite");
                    if (spriteTr != null)
                    {
                        var spriteRenderer = spriteTr.GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null)
                            spriteRenderer.flipX = flipX;
                    }
                    allCharacters.Add(unit);
                }
                else
                {
                    Debug.LogWarning("CharacterStats 컴포넌트가 프리팹에 없습니다.");
                }
            }
        }
        
        // 적군 유닛 생성
        Debug.Log($"[캐릭터리워드] 적 생성 시작 - enemyIDs.Count: {spawn.enemyIDs.Count}");
        for (int i = 0; i < spawn.enemyIDs.Count && i < enemySlots.Length; i++)
        {
            Debug.Log($"[캐릭터리워드] 적 생성 중 - enemyIDs[{i}]: {spawn.enemyIDs[i]}");
            SpawnUnitEnemy(spawn.enemyIDs[i], enemySlots[i]);
        }
        Debug.Log($"[캐릭터리워드] 적 생성 완료 - 생성된 적 수: {spawn.enemyIDs.Count}");
    }

    void SpawnUnit(string id, Transform slot)
    {
        if (!CharacterData.characterDict.TryGetValue(id, out var data))
        {
            Debug.LogError($"[SlotBased][BattleManager] ID {id} 에 해당하는 캐릭터 데이터 없음.");
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
            Debug.LogError($"[SlotBased][BattleManager] ID {id} 에 해당하는 캐릭터 데이터 없음.");
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

        // 패턴에 따라 AI 컴포넌트 할당
        if (obj.GetComponent<EnemyAIController>() == null)
        {
            var characterStats = obj.GetComponent<CharacterStats>();
            if (characterStats != null)
            {
                characterStats.SetData(data);
                
                // 패턴에 따라 다른 AI 컴포넌트 할당
                AssignAIComponent(obj, characterStats);
            }
            else
            {
                // CharacterStats가 없으면 기본 AI 할당
                obj.AddComponent<DefaultEnemyAIController>();
                Debug.LogWarning("[BattleManager] CharacterStats가 없어 기본 AI 할당");
            }
        }

        var unit = obj.GetComponent<CharacterStats>();
        if (unit != null)
        {
            unit.IsPlayer = false;
            // SetData는 AI 할당 부분에서 이미 호출됨
            obj.transform.localScale = Vector3.one * data.Scale;
            allCharacters.Add(unit);
            // 보상용 enemyID 저장
            rewardTargetEnemyIDs.Add(id);
        }
        else
        {
            Debug.LogWarning("CharacterStats 컴포넌트가 프리팹에 없습니다.");
        }
    }

    // 슬롯 기반 단방향 전달: CharacterData와 skillIDs를 받아 아군/플레이어 생성
    void SpawnUnitWithData(CharacterData data, string[] skillIDs, Transform slot, bool flipX)
    {
        if (data == null)
        {
            Debug.LogError("[SlotBased][BattleManager] CharacterData가 null입니다.");
            return;
        }
        
        GameObject obj = Instantiate(SpawnManager.Instance.characterPrefab, slot.position, Quaternion.identity);
        obj.name = $"Unit_{data.ID}";
        obj.transform.SetParent(slot);
        obj.transform.localPosition = Vector3.zero;

        var unit = obj.GetComponent<CharacterStats>();
        if (unit != null)
        {
            unit.SetData(data);
            obj.transform.localScale = Vector3.one * data.Scale;
            
            // 슬롯 기반 단방향 전달: skillIDs를 그대로 복사
            if (skillIDs != null && skillIDs.Length == 4)
            {
                unit.Skills = (string[])skillIDs.Clone();
                Debug.Log($"[SlotBased][BattleManager] unit.Skills 할당: {string.Join(",", unit.Skills)}");
            }
            else
            {
                Debug.Log($"[SlotBased][BattleManager] skillIDs가 null이거나 길이가 4가 아님: {skillIDs?.Length ?? 0}");
            }
            
            // 체력 보존 적용 (ID 기준)
            if (StageSetting.Instance != null)
            {
                int savedHp = StageSetting.Instance.inStageData.GetHP(data.ID);
                if (savedHp > 0)
                {
                    unit.Hp = savedHp;
                }
            }
            
            // flipX 적용 (아군만)
            var spriteTr = obj.transform.Find("Sprite");
            if (spriteTr != null)
            {
                var spriteRenderer = spriteTr.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                    spriteRenderer.flipX = flipX;
            }
            
            allCharacters.Add(unit);
        }
        else
        {
            Debug.LogWarning("CharacterStats 컴포넌트가 프리팹에 없습니다.");
        }
    }

    public void StartBattle()
    {
        allCharacters = new List<CharacterStats>();
        
        // 스테이지 보상 데이터 설정
        SetupStageReward();
        
        SpawnAllUnits();
        CreateAllSkillButtons(allCharacters.Where(c => c.IsPlayer).ToList());
        
        // 축복 효과 적용 (1번 슬롯 주인공에게만)
        ApplyBlessingsToMainCharacter();
        
        // BattleEffectManager는 자체적으로 캐릭터 이벤트를 구독합니다
        
        // StartCoroutine(TimelineManager.Instance.StartTimeline()); // 임시 주석처리
    }

    /// <summary>
    /// 1번 슬롯(주인공)에 활성화된 축복 효과를 적용합니다.
    /// </summary>
    /// <remarks>
    /// - 축복 ScriptableObject의 applyToAllAllies가 false인 경우: 1번 슬롯 주인공에게만 적용
    /// - applyToAllAllies가 true인 경우: 전투 중 아군 전체(allCharacters 중 IsPlayer == true)에 적용
    /// </remarks>
    private void ApplyBlessingsToMainCharacter()
    {
        // 1번 슬롯의 캐릭터 찾기 (playerSlot의 첫 번째 캐릭터)
        CharacterStats mainCharacter = null;
        if (playerSlot != null && playerSlot.childCount > 0)
        {
            mainCharacter = playerSlot.GetChild(0).GetComponent<CharacterStats>();
        }

        if (mainCharacter == null)
        {
            Debug.LogWarning("[BattleManager] 1번 슬롯의 주인공 캐릭터를 찾을 수 없습니다.");
            return;
        }

        // BlessingManager에서 활성화된 축복 적용
        if (BlessingManager.Instance == null)
        {
            Debug.LogWarning("[BattleManager] BlessingManager를 찾을 수 없습니다.");
            return;
        }

        var activeBlessings = BlessingManager.Instance.GetAllActiveBlessings();
        if (activeBlessings == null || activeBlessings.Count == 0)
        {
            Debug.Log("[BattleManager] 활성화된 축복이 없어 적용을 생략합니다.");
            return;
        }

        Debug.Log($"[BattleManager] 전투 시작 시 축복 적용 시작: 활성화된 축복 {activeBlessings.Count}개");
        
        // 정수 상태 확인 (적용 전)
        int essenceBefore = 0;
        int totalEssenceBefore = 0;
        if (GameProgressManager.Instance != null)
        {
            essenceBefore = GameProgressManager.Instance.GetEssence();
            totalEssenceBefore = GameProgressManager.Instance.GetTotalEssence();
            Debug.Log($"[BattleManager] 축복 적용 전 정수 상태: 현재={essenceBefore}, 총량={totalEssenceBefore}");
        }

        // 현재 전투에 참여 중인 아군 목록 (주인공 포함)
        var allyCharacters = allCharacters.Where(c => c.IsPlayer).ToList();

        foreach (var entry in activeBlessings)
        {
            if (entry.Value <= 0) continue; // 칸 수가 0이면 스킵

            var blessingData = BlessingManager.Instance.GetById(entry.Key);
            if (blessingData == null) continue;

            // 적용 범위에 따라 대상 결정
            if (blessingData.applyToAllAllies)
            {
                foreach (var ally in allyCharacters)
                {
                    if (ally == null) continue;
                    BlessingManager.Instance.ApplyBlessing(ally, blessingData);
                }
            }
            else
            {
                BlessingManager.Instance.ApplyBlessing(mainCharacter, blessingData);
            }
        }
        
        // 정수 상태 확인 (적용 후)
        if (GameProgressManager.Instance != null)
        {
            int essenceAfter = GameProgressManager.Instance.GetEssence();
            int totalEssenceAfter = GameProgressManager.Instance.GetTotalEssence();
            Debug.Log($"[BattleManager] 축복 적용 후 정수 상태: 현재={essenceAfter}, 총량={totalEssenceAfter}");
            
            if (essenceBefore != essenceAfter || totalEssenceBefore != totalEssenceAfter)
            {
                Debug.LogError($"[BattleManager] ⚠️ 축복 적용 중 정수가 변경되었습니다! (적용 전: {essenceBefore}/{totalEssenceBefore}, 적용 후: {essenceAfter}/{totalEssenceAfter})");
            }
        }
    }



    public void SavePartyStatusToStageSetting()
    {
        if (StageSetting.Instance == null) return;

        foreach (var character in allCharacters.Where(c => c.IsPlayer))
        {
            // StageSetting.Instance.inStageData.SetHP(character.ID, character.Hp); // 임시 주석처리 (ID, SetHP)
        }
    }

    public void EndBattle(bool isVictory)
    {

        
        SavePartyStatusToStageSetting();
        
        // 전투 결과 데이터 수집
        CollectBattleResultData(isVictory);
    }



    private void CollectBattleResultData(bool isVictory)
    {
        LastBattleResult = new BattleResultData
        {
            isVictory = isVictory,
            stageID = StageManager.Instance?.SelectedStageID ?? "",
            isCleared = false, // 기본값
            soulDustGained = 0, // 영혼먼지 초기화
            essenceGained = 0   // 강자의 정수 초기화
        };

        // 이미 클리어된 스테이지인지 먼저 확인
        bool isAlreadyCleared = StageManager.Instance?.IsStageCleared(LastBattleResult.stageID) ?? false;
        
        if (isAlreadyCleared)
        {
            Debug.Log($"[캐릭터리워드] 이미 클리어된 스테이지: {LastBattleResult.stageID} - 보상 수집 중단");
            LastBattleResult.isCleared = false;
            // return 제거 - 보상만 중단하고 전투 종료는 계속 진행
        }

        // 실제 사망한 적 오브젝트 기준으로 캐릭터/스킬 보상 통합 정산
        Debug.Log("[캐릭터리워드] allCharacters 기반 사망한 적/스킬 보상 통합 정산 시작");
        if (isVictory)
        {
            foreach (var enemy in allCharacters.Where(c => !c.IsPlayer && c.IsDead))
            {
                if (!string.IsNullOrEmpty(enemy.CharacterId) && !LastBattleResult.deadEnemyIDs.Contains(enemy.CharacterId))
                {
                    LastBattleResult.deadEnemyIDs.Add(enemy.CharacterId);
                    Debug.Log($"[캐릭터리워드] 사망한 적 추가됨: {enemy.Label} (ID: {enemy.CharacterId})");
                }
                else
                {
                    Debug.Log($"[캐릭터리워드] 사망한 적 추가 실패: {enemy.Label} (ID: {enemy.CharacterId}) - ID가 비어있거나 중복");
                }
                
                foreach (var skillID in enemy.Skills)
                {
                    if (!string.IsNullOrEmpty(skillID) && !LastBattleResult.unlockedSkillIDs.Contains(skillID))
                    {
                        LastBattleResult.unlockedSkillIDs.Add(skillID);
                        Debug.Log($"[캐릭터리워드] 스킬 해금: {skillID}");
                    }
                }
            }
        }
        Debug.Log($"[캐릭터리워드] 사망한 적 수집 완료 - 총 {LastBattleResult.deadEnemyIDs.Count}명");
        Debug.Log($"[캐릭터리워드] 해금된 스킬 수집 완료 - 총 {LastBattleResult.unlockedSkillIDs.Count}개");

        // 사망한 아군 캐릭터 수집 (기존 방식 유지)
        foreach (var character in allCharacters.Where(c => c.IsPlayer && c.IsDead))
        {
            LastBattleResult.deadAllyIDs.Add(character.CharacterId);
        }

        // 승리 시 보상 처리
        if (isVictory)
        {
            ProcessVictoryRewards();
        }
        // 패배 시에는 deadEnemyIDs, unlockedSkillIDs 모두 비운 상태로 둠
    }

    public void OnEnemyDied(string id)
    {
        // OnEnemyDied는 더 이상 사용하지 않음
        // CollectBattleResultData에서 한 번에 처리하므로 중복 방지
        Debug.Log($"[캐릭터리워드] OnEnemyDied 호출됨 (무시됨): {id}");
    }

    private void ProcessVictoryRewards()
    {
        // CollectBattleResultData에서 이미 클리어 여부를 확인했으므로
        // 여기서는 단순히 스테이지 클리어로 인정
        LastBattleResult.isCleared = true;
        Debug.Log("[캐릭터리워드] ProcessVictoryRewards 완료 - 스테이지 클리어 처리");
    }

    private void CollectUnlockedSkills()
    {
        foreach (var character in allCharacters.Where(c => !c.IsPlayer && c.IsDead))
        {
            // 사망한 적이 소지했던 스킬들을 해금 목록에 추가
            foreach (var skillID in character.Skills)
            {
                if (!string.IsNullOrEmpty(skillID) && !LastBattleResult.unlockedSkillIDs.Contains(skillID))
                {
                    LastBattleResult.unlockedSkillIDs.Add(skillID);
                }
            }
        }
    }

    private void SetupStageReward()
    {
        string currentStageID = StageManager.Instance?.SelectedStageID ?? "";
        
        // 이미 클리어된 스테이지인지 확인
        bool isAlreadyCleared = StageManager.Instance?.IsStageCleared(currentStageID) ?? false;
        
        CurrentStageReward = new StageRewardData
        {
            stageID = currentStageID,
            isAlreadyCleared = isAlreadyCleared
        };

        // XML에서 파싱된 적 정보를 기반으로 보상 설정
        SetupStageSpecificRewards(currentStageID);
    }

    private void SetupStageSpecificRewards(string stageID)
    {
        // 현재 스테이지의 블록 정보에서 적 ID 리스트 가져오기 (참고용)
        var stageData = StageManager.Instance?.GetStage(stageID);
        if (stageData != null && stageData.BlockIDs.Count > 0)
        {
            var blockData = StageManager.Instance?.GetBlock(stageData.BlockIDs[0]);
            if (blockData != null)
            {
                CurrentStageReward.enemyIDs = new List<string>(blockData.EnemyIDs);
            }
        }
        
        // 고정 보상 완전 제거 - 오직 실제 전투 결과만 사용
        CurrentStageReward.unlockableSkills = new List<string>();
        CurrentStageReward.unlockableCharacters = new List<string>();
    }

    public void CreateAllSkillButtons(List<CharacterStats> partyMembers)
    {
        if (skillSetRoot == null || skillButtonPrefab == null) return;

        // 기존 스킬 버튼들만 제거 (이름이 SkillButton으로 시작하는 오브젝트만)
        for (int i = skillSetRoot.childCount - 1; i >= 0; i--)
        {
            var child = skillSetRoot.GetChild(i);
            if (child.name.StartsWith("SkillButton"))
            {
                Destroy(child.gameObject);
            }
        }

        // 각 파티 멤버의 스킬 버튼 생성
        foreach (var member in partyMembers)
        {
            for (int i = 0; i < member.Skills.Length; i++)
            {
                if (!string.IsNullOrEmpty(member.Skills[i]))
                {
                    // SkillSlotN 오브젝트 찾기
                    var slotName = $"SkillSlot{i+1}";
                    var slotTransform = skillSetRoot.Find(slotName);
                    if (slotTransform == null)
                    {
                        Debug.LogWarning($"{slotName}을(를) 찾을 수 없습니다.");
                        continue;
                    }

                    GameObject buttonObj = Instantiate(skillButtonPrefab, slotTransform);
                    buttonObj.name = $"SkillButton_{member.name}_{i}";
                    var skillInstance = buttonObj.GetComponent<SkillInstance>();
                    if (skillInstance != null)
                    {
                        skillInstance.skillID = member.Skills[i];
                        skillInstance.SetSlotIndex(i);
                        skillInstance.SetCaster(member);
                        if (SkillData.skillDict.TryGetValue(member.Skills[i], out var skillData))
                        {
                            skillInstance.SetSkillData(skillData);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 패턴에 따라 적절한 AI 컴포넌트를 할당합니다
    /// </summary>
    private void AssignAIComponent(GameObject obj, CharacterStats characterStats)
    {
        switch (characterStats.Pattern)
        {
            case PatternType.Default:
                obj.AddComponent<DefaultEnemyAIController>();
                Debug.Log($"[BattleManager] {characterStats.Label}에 DefaultEnemyAIController 할당");
                break;
            case PatternType.Adelia:
                obj.AddComponent<AdeliaEnemyAIController>();
                Debug.Log($"[BattleManager] {characterStats.Label}에 AdeliaEnemyAIController 할당");
                break;
            default:
                obj.AddComponent<DefaultEnemyAIController>();
                Debug.Log($"[BattleManager] {characterStats.Label}에 기본 DefaultEnemyAIController 할당 (패턴: {characterStats.Pattern})");
                break;
        }
    }
}

