using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Added for .Select()

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    // 등급별 영혼먼지 보상량
    private static readonly Dictionary<RarityList, int> SoulDustRewardByRarity = new()
    {
        { RarityList.Normal, 2 },
        { RarityList.Rare, 5 },
        { RarityList.Uniqu, 10 },
        { RarityList.Legend, 25 }
        // One 등급은 주인공 전용이므로 무시
    };

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
    /// 전투 결과 데이터를 받아 보상 지급/해금/저장 처리의 메인 엔트리 포인트
    /// </summary>
    public void ProcessBattleReward(BattleResultData result)
    {
        // 이중 안전장치: 이미 클리어된 스테이지의 보상 데이터 제거
        if (result != null && !string.IsNullOrEmpty(result.stageID))
        {
            bool isAlreadyCleared = StageManager.Instance?.IsStageCleared(result.stageID) ?? false;
            if (isAlreadyCleared)
            {
                Debug.Log($"[RewardManager] 이중 안전장치: 이미 클리어된 스테이지 {result.stageID}의 보상 데이터 제거");
                result.deadEnemyIDs.Clear();
                result.unlockedSkillIDs.Clear();
                result.unlockedCharacterIDs.Clear();
                result.soulDustGained = 0;
                result.essenceGained = 0;
                result.isCleared = false;
                return; // 보상 지급 중단
            }
        }

        GrantRewards(result);
        SaveRewards(result);

        // 보상 UI 표시 (월드맵에 RewardUIManager가 있을 때만)
        if (RewardUIManager.Instance != null)
        {
            RewardUIManager.Instance.ShowReward(result);
        }
        else
        {
            Debug.LogWarning("[RewardManager] RewardUIManager 인스턴스가 존재하지 않습니다. (월드맵 외에서는 무시 가능)");
        }
    }

    /// <summary>
    /// 보상 지급/해금 처리 (스킬, 캐릭터, 재화 등)
    /// </summary>
    private void GrantRewards(BattleResultData result)
    {
        if (result == null)
        {
            Debug.LogError("[RewardManager] result가 null입니다!");
            return;
        }
        
        if (result.deadEnemyIDs == null)
        {
            Debug.LogError("[RewardManager] result.deadEnemyIDs가 null입니다!");
            return;
        }

        // [캐릭터리워드][중복체크] deadEnemyIDs와 인벤토리 전체 출력
        Debug.Log($"[캐릭터리워드][중복체크] deadEnemyIDs: {string.Join(",", result.deadEnemyIDs)}");
        Debug.Log($"[캐릭터리워드][중복체크] 인벤토리 IDs: {string.Join(",", GameProgressManager.Instance.CharacterInventory.Select(c => c.ID))}");

        // 1. deadEnemyIDs를 순회하며 등급별 먼지 보상 누적 (중복 제거)
        int totalSoulDust = 0;
        Debug.Log($"[영혼먼지][보상] 영혼먼지 계산 시작 - deadEnemyIDs.Count: {result.deadEnemyIDs.Count}");
        
        // 중복 제거를 위해 HashSet 사용
        HashSet<string> uniqueEnemyIDs = new HashSet<string>(result.deadEnemyIDs);
        Debug.Log($"[영혼먼지][보상] 중복 제거 후 uniqueEnemyIDs.Count: {uniqueEnemyIDs.Count}");
        
        if (uniqueEnemyIDs.Count > 0)
        {
            foreach (var enemyID in uniqueEnemyIDs)
            {
                Debug.Log($"[영혼먼지][보상] 적 {enemyID} 영혼먼지 계산 중...");
                
                if (CharacterData.characterDict.TryGetValue(enemyID, out var charData))
                {
                    Debug.Log($"[영혼먼지][보상] 적 {enemyID} 등급: {charData.Rarity}");
                    
                    if (charData.Rarity == RarityList.One) 
                    {
                        Debug.Log($"[영혼먼지][보상] 적 {enemyID}는 주인공 등급이므로 영혼먼지 제외");
                        continue; // 주인공 등급은 무시
                    }
                    
                    if (SoulDustRewardByRarity.TryGetValue(charData.Rarity, out int dust))
                    {
                        totalSoulDust += dust;
                        Debug.Log($"[영혼먼지][보상] 적 {enemyID} 영혼먼지 +{dust} (누적: {totalSoulDust})");
                    }
                    else
                    {
                        Debug.LogWarning($"[영혼먼지][보상] 적 {enemyID}의 등급 {charData.Rarity}에 대한 영혼먼지 보상량이 정의되지 않음");
                    }
                }
                else
                {
                    Debug.LogError($"[영혼먼지][보상] CharacterData.characterDict에서 적 {enemyID}를 찾을 수 없습니다!");
                }
            }
        }
        else
        {
            Debug.LogWarning("[RewardManager] deadEnemyIDs가 비어있어 영혼먼지 보상이 0입니다.");
        }
        
        result.soulDustGained = totalSoulDust;
        Debug.Log($"[RewardManager] 최종 영혼먼지 보상: {totalSoulDust}");

        // 2. 사망한 적 캐릭터 해금 및 인벤토리 추가 (중복 없이)
        foreach (var enemyID in result.deadEnemyIDs)
        {
            // [캐릭터리워드][중복체크] 인벤토리 내 모든 캐릭터와 비교
            foreach (var c in GameProgressManager.Instance.CharacterInventory)
                Debug.Log($"[캐릭터리워드][중복체크] 인벤토리 캐릭터: {c.ID}, IsUnlocked: {c.IsUnlocked}");
            Debug.Log($"[캐릭터리워드][중복체크] deadEnemyID: {enemyID}");

            // 캐릭터 해금 처리
            GameProgressManager.Instance.UnlockCharacter(enemyID);
            
            // 인벤토리에 추가 (AddCharacterToInventory 내부에서 중복 체크 및 저장 처리)
            GameProgressManager.Instance.AddCharacterToInventory(enemyID, 1);
            Debug.Log($"[RewardManager] 캐릭터 해금 및 인벤토리 추가 완료: {enemyID}");
        }

        // 3. 스킬 해금
        if (result.unlockedSkillIDs != null && result.unlockedSkillIDs.Count > 0)
        {
            foreach (var skillID in result.unlockedSkillIDs)
            {
                GameProgressManager.Instance.UnlockSkill(skillID);
            }
        }

        // 4. 영혼먼지 등 재화 지급
        Debug.Log($"[영혼먼지][보상] 재화 지급 시작 - soulDustGained: {result.soulDustGained}, essenceGained: {result.essenceGained}");
        
        if (result.soulDustGained > 0)
        {
            Debug.Log($"[영혼먼지][보상] 영혼먼지 지급 시작: +{result.soulDustGained}");
            GameProgressManager.Instance.AddSoulDust(result.soulDustGained);
            Debug.Log($"[영혼먼지][보상] 영혼먼지 지급 완료: +{result.soulDustGained}");
        }
        else
        {
            Debug.LogWarning("[영혼먼지][보상] 영혼먼지 보상이 0이므로 지급하지 않음");
        }
        
        if (result.essenceGained > 0)
        {
            Debug.Log($"[RewardManager] 강자의 정수 지급: +{result.essenceGained}");
            GameProgressManager.Instance.AddEssence(result.essenceGained);
        }
        else
        {
            Debug.LogWarning("[RewardManager] 강자의 정수 보상이 0이므로 지급하지 않음");
        }

        // 5. 월드맵 재화 UI 업데이트
        UpdateWorldMapCurrencyUI();

        // 5. (선택) 인벤토리/스킬탭 UI 즉시 새로고침
        if (CharacterInventoryTab.Instance != null)
            CharacterInventoryTab.Instance.RefreshInventory();
        if (SkillInventoryTab.Instance != null)
            SkillInventoryTab.Instance.RefreshSkillInventory();
    }

    /// <summary>
    /// 보상 관련 데이터 영구 저장
    /// </summary>
    private void SaveRewards(BattleResultData result)
    {
        // GameProgressManager를 통해 현재 슬롯의 데이터를 저장
        // UnlockCharacter, AddCharacterToInventory, UnlockSkill, AddItem 등에서 이미 SaveGameProgress가 호출되므로
        // 여기서는 추가 저장이 필요하지 않을 수 있지만, 확실성을 위해 한 번 더 저장
        GameProgressManager.Instance.SaveGameProgress(GameProgressManager.Instance.CurrentSlot);
        
        // 스테이지 클리어 정보 저장 (StageManager를 통해)
        if (result.isCleared && !string.IsNullOrEmpty(result.stageID))
        {
            StageManager.Instance?.MarkStageAsCleared(result.stageID);
        }
        
        Debug.Log($"[RewardManager] 보상 데이터 저장 완료 (슬롯: {GameProgressManager.Instance.CurrentSlot})");
    }

    /// <summary>
    /// (선택) 보상 UI 연동, 연출 등
    /// </summary>
    public void ShowRewardUI(BattleResultData result)
    {
        // 보상 UI 표시 로직
    }

    /// <summary>
    /// 월드맵의 재화 UI를 업데이트합니다.
    /// </summary>
    private void UpdateWorldMapCurrencyUI()
    {
        // 월드맵 씬에서만 작동
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SampleScene")
        {
            var currencyUI = FindObjectOfType<WorldMapCurrencyUI>();
            if (currencyUI != null)
            {
                currencyUI.OnCurrencyChanged();
                Debug.Log("[RewardManager] 월드맵 재화 UI 업데이트 완료");
            }
            else
            {
                Debug.LogWarning("[RewardManager] WorldMapCurrencyUI를 찾을 수 없습니다.");
            }
        }
    }
} 