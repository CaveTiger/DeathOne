using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// 축복 시스템을 관리하는 매니저 클래스
/// </summary>
public class BlessingManager : MonoBehaviour
{
    public static BlessingManager Instance { get; private set; }

    private Dictionary<string, BlessingData> blessingDict = new Dictionary<string, BlessingData>();
    
    // 활성화된 축복 관리 (축복ID -> 활성화된 칸 수)
    private Dictionary<string, int> activeBlessings = new Dictionary<string, int>();
    
    // 전술 축복(라인 3)의 선택된 대상 관리 (축복ID -> 선택된 슬롯 번호)
    // 예: "하나를 위한 모두" 축복이 활성화되어 있고 대상이 2번 슬롯이라면
    // tacticalBlessingTargets["030001"] = 2 형태로 저장
    // 슬롯 번호: 1(주인공), 2, 3, 4
    private Dictionary<string, int> tacticalBlessingTargets = new Dictionary<string, int>();

    [Header("Inspector 확인용 (읽기 전용)")]
    [SerializeField] private List<BlessingData> loadedBlessings = new List<BlessingData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        LoadAllBlessings();
    }

    private void Start()
    {
        // 축복 로드는 BlessingPanel의 OnEnable에서 처리됨
        // 전투 씬에서는 SyncWithGameProgress()를 건너뜀 (전투 시작 시 이미 적용됨)
        // BattleManager가 존재하면 전투 씬으로 판단
        if (BattleManager.Instance == null)
        {
            // 월드맵/스테이지 씬에서만 정수 동기화 수행
            SyncWithGameProgress();
        }
        else
        {
            Debug.Log("[BlessingManager] 전투 씬 감지: SyncWithGameProgress()를 건너뜁니다. (전투 시작 시 이미 적용됨)");
        }
    }

    /// <summary>
    /// Resources 폴더에서 모든 축복 ScriptableObject를 로드합니다.
    /// </summary>
    private void LoadAllBlessings()
    {
        BlessingData[] blessings = Resources.LoadAll<BlessingData>("Data/Blessing");
        blessingDict.Clear();
        loadedBlessings.Clear();
        
        foreach (BlessingData blessing in blessings)
        {
            if (blessing != null && !string.IsNullOrEmpty(blessing.blessingID))
            {
                blessingDict[blessing.blessingID] = blessing;
                loadedBlessings.Add(blessing);
                Debug.Log($"[BlessingManager] 축복 로드: {blessing.blessingID} - {blessing.blessingName}");
            }
        }
        
        Debug.Log($"[BlessingManager] 총 {blessingDict.Count}개의 축복을 로드했습니다.");
    }

    /// <summary>
    /// ID로 축복 데이터를 가져옵니다.
    /// </summary>
    /// <param name="id">축복 ID</param>
    /// <returns>축복 데이터</returns>
    public BlessingData GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        blessingDict.TryGetValue(id, out BlessingData data);
        return data;
    }

    /// <summary>
    /// 특정 라인의 모든 축복을 리스트에 등록된 순서대로 반환합니다. (비트마스크 인덱스 순서)
    /// </summary>
    /// <param name="lineIndex">라인 인덱스 (0~4)</param>
    /// <returns>등록 순서대로 정렬된 축복 데이터 리스트</returns>
    public List<BlessingData> GetBlessingsByLine(int lineIndex)
    {
        List<BlessingData> lineBlessings = new List<BlessingData>();
        
        // loadedBlessings에 등록된 순서대로 필터링 (리스트 등록 순서 유지)
        foreach (var blessing in loadedBlessings)
        {
            if (blessing != null && blessing.lineIndex == lineIndex)
            {
                lineBlessings.Add(blessing);
            }
        }
        
        return lineBlessings;
    }

    /// <summary>
    /// 축복 효과를 캐릭터에 적용합니다.
    /// 주의: 이 메서드는 정수를 소모하지 않습니다. 단순히 스탯 효과만 적용합니다.
    /// 정수 소모는 BlessingPanel.ToggleBlessingInLine()에서만 발생합니다.
    /// </summary>
    /// <param name="character">효과를 적용할 캐릭터</param>
    /// <param name="blessingData">축복 데이터</param>
    public void ApplyBlessing(CharacterStats character, BlessingData blessingData)
    {
        if (character == null || blessingData == null)
        {
            Debug.LogWarning("[BlessingManager] 캐릭터 또는 축복 데이터가 null입니다.");
            return;
        }

        Debug.Log($"[BlessingManager] ApplyBlessing 호출: {blessingData.blessingName} (ID: {blessingData.blessingID}) -> {character.Label}");
        Debug.Log($"[BlessingManager]   정수 소모 없음 (이 메서드는 효과만 적용)");

        // scriptClass가 있으면 특수 효과 사용
        if (!string.IsNullOrEmpty(blessingData.scriptClass))
        {
            ApplySpecialEffect(character, blessingData);
        }
        // scriptClass가 없으면 단순 스탯 효과 사용
        else if (blessingData.targetStat != TargetStat.None)
        {
            ApplyStatEffect(character, blessingData);
        }
        else
        {
            Debug.LogWarning($"[BlessingManager] 축복 {blessingData.blessingID}에 효과 설정이 없습니다.");
        }
    }

    /// <summary>
    /// 리플렉션을 사용하여 특수 효과 클래스를 찾아서 실행합니다.
    /// </summary>
    /// <param name="character">효과를 적용할 캐릭터</param>
    /// <param name="blessingData">축복 데이터</param>
    private void ApplySpecialEffect(CharacterStats character, BlessingData blessingData)
    {
        try
        {
            // scriptClass 문자열로 타입 찾기 (네임스페이스 없이 클래스 이름만 있는 경우 처리)
            Type effectType = Type.GetType(blessingData.scriptClass);
            
            // 네임스페이스 없이 클래스 이름만 있는 경우 현재 어셈블리에서 찾기
            if (effectType == null)
            {
                effectType = Type.GetType(blessingData.scriptClass + ", Assembly-CSharp");
            }
            
            if (effectType == null)
            {
                Debug.LogError($"[BlessingManager] 클래스를 찾을 수 없습니다: {blessingData.scriptClass}");
                return;
            }

            // BlessingEffectBase를 상속받은 클래스인지 확인
            if (!typeof(BlessingEffectBase).IsAssignableFrom(effectType))
            {
                Debug.LogError($"[BlessingManager] {blessingData.scriptClass}는 BlessingEffectBase를 상속받지 않았습니다.");
                return;
            }

            // 인스턴스 생성
            BlessingEffectBase effect = (BlessingEffectBase)Activator.CreateInstance(effectType);
            
            // 효과 적용
            effect.Apply(character, blessingData);
            
            Debug.Log($"[BlessingManager] 특수 효과 적용 완료: {blessingData.scriptClass}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[BlessingManager] 특수 효과 적용 중 오류 발생: {e.Message}");
        }
    }

    /// <summary>
    /// 단순 스탯 효과를 적용합니다.
    /// </summary>
    /// <param name="character">효과를 적용할 캐릭터</param>
    /// <param name="blessingData">축복 데이터</param>
    private void ApplyStatEffect(CharacterStats character, BlessingData blessingData)
    {
        float value = blessingData.value;
        
        switch (blessingData.targetStat)
        {
            // Hp / MaxHp는 축복에서는 동일하게 취급 (정수 스탯이므로 반올림 후 적용)
            case TargetStat.Hp:
            case TargetStat.MaxHp:
                int hpDelta = Mathf.RoundToInt(value);
                character.MaxHp += hpDelta;
                // 현재 체력도 같은 비율로 증가 (체력 비율 유지)
                if (character.MaxHp > 0)
                {
                    float healthRatio = (float)character.Hp / (character.MaxHp - hpDelta);
                    character.Hp = Mathf.RoundToInt(character.MaxHp * healthRatio);
                }
                // UI 업데이트
                if (character.HpUI != null)
                {
                    character.HpUI.UpdateHpBar(character.Hp, character.MaxHp);
                }
                break;
            case TargetStat.Atk:
                character.Atk += Mathf.RoundToInt(value);
                break;
            case TargetStat.Def:
                character.Def += Mathf.RoundToInt(value);
                break;
            case TargetStat.Speed:
                character.Speed += Mathf.RoundToInt(value);
                break;
            case TargetStat.Evasion:
                character.Evasion += value;
                break;
            case TargetStat.Accuracy:
                character.Accuracy += value;
                break;
            default:
                Debug.LogWarning($"[BlessingManager] 알 수 없는 스탯 타입: {blessingData.targetStat}");
                break;
        }
        
        Debug.Log($"[BlessingManager] {character.Label}의 {blessingData.targetStat}이(가) {value} 증가했습니다. (축복: {blessingData.blessingName})");
    }

    #region 활성화된 축복 관리

    /// <summary>
    /// 축복을 토글합니다. (비활성 → 활성: 정수 1 소모, 활성 → 비활성: 정수 1 반환)
    /// </summary>
    /// <param name="blessingID">축복 ID</param>
    /// <returns>토글 성공 여부</returns>
    public bool ToggleBlessing(string blessingID)
    {
        if (string.IsNullOrEmpty(blessingID))
        {
            Debug.LogWarning("[BlessingManager] 축복 ID가 비어있습니다.");
            return false;
        }

        if (!blessingDict.ContainsKey(blessingID))
        {
            Debug.LogWarning($"[BlessingManager] 축복 데이터를 찾을 수 없습니다: {blessingID}");
            return false;
        }

        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("[BlessingManager] GameProgressManager를 찾을 수 없습니다.");
            return false;
        }

        bool isCurrentlyActive = IsBlessingActive(blessingID);
        
        if (isCurrentlyActive)
        {
            // 활성 → 비활성: 정수 1 반환 (총량은 그대로, 현재 보유량만 +1)
            GameProgressManager.Instance.AddEssence(1, false);
            activeBlessings.Remove(blessingID);
            
            // 저장은 BlessingPanel에서 버튼 클릭 직후 수행
            Debug.Log($"[BlessingManager] 축복 비활성화: {blessingID} (정수 1 반환, 총량 변화 없음)");
            return true;
        }
        else
        {
            // 비활성 → 활성: 정수 1 소모 (총량은 그대로, 현재 보유량만 -1)
            if (GameProgressManager.Instance.SpendEssence(1))
            {
                activeBlessings[blessingID] = 1;
                
                // 저장은 BlessingPanel에서 버튼 클릭 직후 수행
                Debug.Log($"[BlessingManager] 축복 활성화: {blessingID} (정수 1 소모, 총량 변화 없음)");
                return true;
            }
            
            Debug.LogWarning("[BlessingManager] 강자의 정수가 부족합니다. (필요: 1)");
            return false;
        }
    }

    /// <summary>
    /// 축복을 활성화합니다. (강자의 정수 소비는 별도 처리 필요)
    /// </summary>
    /// <param name="blessingID">축복 ID</param>
    /// <returns>활성화 성공 여부</returns>
    public bool ActivateBlessing(string blessingID)
    {
        if (string.IsNullOrEmpty(blessingID))
        {
            Debug.LogWarning("[BlessingManager] 축복 ID가 비어있습니다.");
            return false;
        }

        if (!blessingDict.ContainsKey(blessingID))
        {
            Debug.LogWarning($"[BlessingManager] 축복 데이터를 찾을 수 없습니다: {blessingID}");
            return false;
        }

        // 활성화된 칸 수 증가 (1:1 비율)
        if (activeBlessings.ContainsKey(blessingID))
        {
            activeBlessings[blessingID]++;
        }
        else
        {
            activeBlessings[blessingID] = 1;
        }

        Debug.Log($"[BlessingManager] 축복 활성화: {blessingID} (칸 수: {activeBlessings[blessingID]})");
        return true;
    }

    /// <summary>
    /// 축복을 비활성화합니다.
    /// </summary>
    /// <param name="blessingID">축복 ID</param>
    /// <returns>비활성화 성공 여부</returns>
    public bool DeactivateBlessing(string blessingID)
    {
        if (string.IsNullOrEmpty(blessingID))
        {
            Debug.LogWarning("[BlessingManager] 축복 ID가 비어있습니다.");
            return false;
        }

        if (activeBlessings.ContainsKey(blessingID))
        {
            activeBlessings[blessingID]--;
            if (activeBlessings[blessingID] <= 0)
            {
                activeBlessings.Remove(blessingID);
            }
            Debug.Log($"[BlessingManager] 축복 비활성화: {blessingID}");
            return true;
        }

        Debug.LogWarning($"[BlessingManager] 활성화되지 않은 축복입니다: {blessingID}");
        return false;
    }

    /// <summary>
    /// 축복의 활성화 상태를 확인합니다.
    /// </summary>
    /// <param name="blessingID">축복 ID</param>
    /// <returns>활성화된 칸 수 (0이면 비활성화)</returns>
    public int GetBlessingStepCount(string blessingID)
    {
        if (string.IsNullOrEmpty(blessingID)) return 0;
        activeBlessings.TryGetValue(blessingID, out int stepCount);
        return stepCount;
    }

    /// <summary>
    /// 축복이 활성화되어 있는지 확인합니다.
    /// </summary>
    /// <param name="blessingID">축복 ID</param>
    /// <returns>활성화 여부</returns>
    public bool IsBlessingActive(string blessingID)
    {
        return GetBlessingStepCount(blessingID) > 0;
    }

    /// <summary>
    /// 모든 활성화된 축복 정보를 반환합니다.
    /// </summary>
    /// <returns>활성화된 축복 딕셔너리 (축복ID -> 칸 수)</returns>
    public Dictionary<string, int> GetAllActiveBlessings()
    {
        return new Dictionary<string, int>(activeBlessings);
    }

    /// <summary>
    /// 활성화된 축복 정보를 설정합니다. (GameProgressManager에서 로드 시 사용)
    /// </summary>
    /// <param name="blessings">활성화된 축복 딕셔너리</param>
    public void SetActiveBlessings(Dictionary<string, int> blessings)
    {
        if (blessings == null)
        {
            activeBlessings.Clear();
            return;
        }

        activeBlessings = new Dictionary<string, int>(blessings);
        Debug.Log($"[BlessingManager] 활성화된 축복 정보 설정 완료: {activeBlessings.Count}개");
    }

    /// <summary>
    /// GameProgressManager의 정수와 활성화된 축복 수를 동기화합니다.
    /// 활성화된 축복 수 = 소모된 정수 수 (1:1 비율)이므로, 이 관계를 유지하도록 보정합니다.
    /// 
    /// 검증 규칙:
    /// 1. totalEssence는 절대 -값이 될 수 없음 (0 이상 보장)
    /// 2. totalEssence == currentEssence + 활성화된 축복 개수
    /// 3. 불일치 시 자동 보정 (초과 축복은 비활성화, 부족한 경우는 totalEssence 보정)
    /// </summary>
    public void SyncWithGameProgress()
    {
        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("[BlessingManager] GameProgressManager를 찾을 수 없습니다.");
            return;
        }

        int currentEssence = GameProgressManager.Instance.GetEssence();
        int totalEssence = GameProgressManager.Instance.GetTotalEssence();
        int activeBlessingCount = GetTotalActiveBlessingCount();

        // 1. totalEssence가 -값이 되지 않도록 보장
        if (totalEssence < 0)
        {
            Debug.LogWarning($"[BlessingManager] totalEssence가 음수입니다 ({totalEssence}). 0으로 보정합니다.");
            // totalEssence를 최소한 currentEssence + activeBlessingCount로 설정
            int minTotal = currentEssence + activeBlessingCount;
            totalEssence = Mathf.Max(0, minTotal);
            
            // GameProgressManager에 직접 접근하여 수정 (리플렉션 사용)
            try
            {
                var currentSaveData = GameProgressManager.Instance.CurrentSaveData;
                currentSaveData.totalEssence = totalEssence;
                Debug.Log($"[BlessingManager] 저장 포인트 #4: SyncWithGameProgress()에서 totalEssence 보정 후 저장 호출");
                GameProgressManager.Instance.SaveGameProgress(GameProgressManager.Instance.CurrentSlot);
            }
            catch (Exception e)
            {
                Debug.LogError($"[BlessingManager] totalEssence 보정 중 오류: {e.Message}");
            }
        }

        // 2. 이론상 관계: totalEssence = currentEssence + activeBlessingCount
        int expectedActiveFromTotal = totalEssence - currentEssence;

        // currentEssence가 totalEssence보다 큰 경우는 말이 안 되므로 보정
        if (expectedActiveFromTotal < 0)
        {
            Debug.LogWarning($"[BlessingManager] 정수 총량 불일치 감지 - totalEssence({totalEssence}) < currentEssence({currentEssence}). totalEssence를 보정합니다.");
            
            // totalEssence를 currentEssence + activeBlessingCount로 보정
            int correctedTotal = currentEssence + activeBlessingCount;
            try
            {
                var currentSaveData = GameProgressManager.Instance.CurrentSaveData;
                currentSaveData.totalEssence = correctedTotal;
                Debug.Log($"[BlessingManager] 저장 포인트 #5: SyncWithGameProgress()에서 totalEssence 불일치 보정 후 저장 호출");
                GameProgressManager.Instance.SaveGameProgress(GameProgressManager.Instance.CurrentSlot);
                Debug.Log($"[BlessingManager] totalEssence 보정 완료: {totalEssence} → {correctedTotal}");
                totalEssence = correctedTotal;
                expectedActiveFromTotal = activeBlessingCount;
            }
            catch (Exception e)
            {
                Debug.LogError($"[BlessingManager] totalEssence 보정 중 오류: {e.Message}");
                return;
            }
        }

        // 3. 활성 축복 수가 이론상 가능한 수(expectedActiveFromTotal)를 초과하면
        // 초과분을 비활성화하되, 정수는 돌려주지 않는다.
        if (activeBlessingCount > expectedActiveFromTotal)
        {
            int overCount = activeBlessingCount - expectedActiveFromTotal;
            Debug.LogWarning($"[BlessingManager] 활성 축복 수가 총량 기준을 초과했습니다. 초과분 {overCount}개를 정수 환불 없이 비활성화합니다. (totalEssence: {totalEssence}, currentEssence: {currentEssence}, active: {activeBlessingCount})");

            // Dictionary를 수정해야 하므로 ToList()로 복사 후 순회
            var keys = new List<string>(activeBlessings.Keys);
            foreach (var key in keys)
            {
                if (overCount <= 0) break;

                int value = activeBlessings[key];
                int removeAmount = Mathf.Min(value, overCount);
                value -= removeAmount;
                overCount -= removeAmount;

                if (value > 0)
                {
                    activeBlessings[key] = value;
                }
                else
                {
                    activeBlessings.Remove(key);
                }
            }

            // 보정 후 저장
            Debug.Log($"[BlessingManager] 저장 포인트 #3: SyncWithGameProgress()에서 초과 축복 비활성화 후 저장 호출");
            GameProgressManager.Instance.SaveGameProgress(GameProgressManager.Instance.CurrentSlot);

            int fixedActiveCount = GetTotalActiveBlessingCount();
            Debug.Log($"[BlessingManager] 보정 후 활성 축복 수: {fixedActiveCount} (이론상 수: {expectedActiveFromTotal})");
        }
        else if (activeBlessingCount < expectedActiveFromTotal)
        {
            // 활성 축복 수가 기대값보다 적은 경우 (누락 가능성)
            // 이는 정상적인 상황일 수 있음 (아직 축복을 다 찍지 않았거나, 환불 후 재투자 전)
            Debug.Log($"[BlessingManager] 정수/축복 상태 - total: {totalEssence}, current: {currentEssence}, active: {activeBlessingCount}, 기대 active: {expectedActiveFromTotal} (정상 또는 미투자 상태)");
        }
        else
        {
            // 완벽하게 일치
            Debug.Log($"[BlessingManager] 정수/축복 상태 정상 - total: {totalEssence}, current: {currentEssence}, active: {activeBlessingCount}");
        }
    }

    /// <summary>
    /// 정수 총량 검증을 수행합니다.
    /// totalEssence == essence + 활성화된 축복 개수 관계를 검증하고, 불일치 시 로그를 남깁니다.
    /// </summary>
    public void ValidateEssenceTotal()
    {
        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("[BlessingManager] GameProgressManager를 찾을 수 없습니다.");
            return;
        }

        int currentEssence = GameProgressManager.Instance.GetEssence();
        int totalEssence = GameProgressManager.Instance.GetTotalEssence();
        int activeBlessingCount = GetTotalActiveBlessingCount();
        int expectedTotal = currentEssence + activeBlessingCount;

        Debug.Log($"[BlessingManager] [검증] totalEssence: {totalEssence}, essence: {currentEssence}, 활성축복: {activeBlessingCount}, 기대총량: {expectedTotal}");

        if (totalEssence < 0)
        {
            Debug.LogError($"[BlessingManager] [검증 실패] totalEssence가 음수입니다: {totalEssence}");
        }
        else if (totalEssence != expectedTotal)
        {
            Debug.LogWarning($"[BlessingManager] [검증 불일치] totalEssence({totalEssence}) != essence({currentEssence}) + 활성축복({activeBlessingCount}) = {expectedTotal}");
        }
        else
        {
            Debug.Log($"[BlessingManager] [검증 성공] 정수 총량이 정상입니다.");
        }
    }

    /// <summary>
    /// 활성화된 축복의 총 개수를 반환합니다.
    /// </summary>
    /// <returns>활성화된 축복의 총 개수</returns>
    private int GetTotalActiveBlessingCount()
    {
        int count = 0;
        foreach (var entry in activeBlessings)
        {
            count += entry.Value; // 각 축복의 활성화된 칸 수 합산
        }
        return count;
    }

    /// <summary>
    /// 전투 시작 시 활성화된 축복을 주인공 캐릭터에 적용합니다.
    /// </summary>
    /// <param name="mainCharacter">주인공 캐릭터 (1번 슬롯)</param>
    public void ApplyActiveBlessingsToCharacter(CharacterStats mainCharacter)
    {
        if (mainCharacter == null)
        {
            Debug.LogWarning("[BlessingManager] 캐릭터가 null입니다.");
            return;
        }

        if (activeBlessings == null || activeBlessings.Count == 0)
        {
            Debug.Log("[BlessingManager] 활성화된 축복이 없습니다.");
            return;
        }

        foreach (var blessingEntry in activeBlessings)
        {
            string blessingID = blessingEntry.Key;
            int stepCount = blessingEntry.Value;

            // 칸 수가 0보다 크면 활성화된 것으로 간주
            if (stepCount > 0)
            {
                BlessingData blessingData = GetById(blessingID);
                if (blessingData != null)
                {
                    ApplyBlessing(mainCharacter, blessingData);
                    Debug.Log($"[BlessingManager] 축복 적용: {blessingData.blessingName} (ID: {blessingID}, 칸 수: {stepCount})");
                }
                else
                {
                    Debug.LogWarning($"[BlessingManager] 축복 데이터를 찾을 수 없습니다: {blessingID}");
                }
            }
        }
    }

    #endregion

    #region 전술 축복 대상 관리

    /// <summary>
    /// 전술 축복의 선택된 대상을 설정합니다.
    /// </summary>
    /// <param name="blessingID">축복 ID</param>
    /// <param name="slotNumber">선택된 슬롯 번호 (1~4)</param>
    public void SetTacticalBlessingTarget(string blessingID, int slotNumber)
    {
        if (string.IsNullOrEmpty(blessingID))
        {
            Debug.LogWarning("[BlessingManager] 축복 ID가 비어있습니다.");
            return;
        }

        if (slotNumber < 1 || slotNumber > 4)
        {
            Debug.LogWarning($"[BlessingManager] 슬롯 번호가 유효하지 않습니다: {slotNumber} (1~4 범위여야 함)");
            return;
        }

        tacticalBlessingTargets[blessingID] = slotNumber;
        Debug.Log($"[BlessingManager] 전술 축복 대상 설정: {blessingID} -> 슬롯 {slotNumber}");
    }

    /// <summary>
    /// 전술 축복의 선택된 대상을 조회합니다.
    /// </summary>
    /// <param name="blessingID">축복 ID</param>
    /// <returns>선택된 슬롯 번호 (1~4, 없으면 0)</returns>
    public int GetTacticalBlessingTarget(string blessingID)
    {
        if (string.IsNullOrEmpty(blessingID))
        {
            return 0;
        }

        tacticalBlessingTargets.TryGetValue(blessingID, out int slotNumber);
        return slotNumber;
    }

    /// <summary>
    /// 전술 축복의 선택된 대상을 제거합니다.
    /// </summary>
    /// <param name="blessingID">축복 ID</param>
    public void ClearTacticalBlessingTarget(string blessingID)
    {
        if (string.IsNullOrEmpty(blessingID))
        {
            return;
        }

        if (tacticalBlessingTargets.Remove(blessingID))
        {
            Debug.Log($"[BlessingManager] 전술 축복 대상 제거: {blessingID}");
        }
    }

    /// <summary>
    /// 모든 전술 축복 대상 정보를 반환합니다.
    /// </summary>
    /// <returns>전술 축복 대상 딕셔너리 (축복ID -> 슬롯 번호)</returns>
    public Dictionary<string, int> GetAllTacticalBlessingTargets()
    {
        return new Dictionary<string, int>(tacticalBlessingTargets);
    }

    /// <summary>
    /// 전술 축복 대상 정보를 설정합니다. (GameProgressManager에서 로드 시 사용)
    /// </summary>
    /// <param name="targets">전술 축복 대상 딕셔너리 (축복ID -> 슬롯 번호)</param>
    public void SetTacticalBlessingTargets(Dictionary<string, int> targets)
    {
        if (targets == null)
        {
            tacticalBlessingTargets.Clear();
            return;
        }

        tacticalBlessingTargets = new Dictionary<string, int>(targets);
        Debug.Log($"[BlessingManager] 전술 축복 대상 정보 설정 완료: {tacticalBlessingTargets.Count}개");
    }

    #endregion
}

