using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class CharacterStats : MonoBehaviour
{
    public string Label;
    public int Hp, MaxHp, Atk, Def;
    public float Evasion, Accuracy, CollapseChance; //CollapseChance의 경우 캐릭터 데이터엔 존재하지 않음 한 스테이지 기준에서 관리함
    public int Speed;
    public string[] Skills = new string[4];
    public bool IsDead = false; //캐릭터의 죽음
    public bool IsActive = true; //false가 되면 스킬로 인한 행동불가판정
    public bool IsMyTurn = false; //턴 당사자
    public bool IsPlayer = true; //플레이어블
    public bool TurnChanse = false; //턴이 올때 기회
    public PatternType Pattern;
    public RarityList Rarity;
    
    [Header("붕괴 시스템")]
    [Tooltip("상태이상 피해로 체력이 0이 되어 방치된 상태인지 추적")]
    private bool isNeglected = false; // 상태이상 피해로 체력이 0이 되어 방치된 상태
    public string CharacterId;  // ID 필드 모션 참조용
    public string CharacterFolder; // "Player" 또는 "MobA"
    public string CharacterName;   // "Player" 또는 "MobA"

    public CharacterData data;
    public SpriteRenderer spriteRenderer;
    public HpUIHandler HpUI;
    
    [Header("데스 연출용 UI 참조")]
    [SerializeField] public GameObject hpBarObject; // 체력바 오브젝트

    [Header("상태이상 관리")]
    [Tooltip("현재 적용된 상태이상 프리팹들의 리스트")]
    private List<GameObject> activeEffectPrefabs = new List<GameObject>();
    
    [Tooltip("상태이상 UI가 생성될 영역")]
    [SerializeField] public Transform statusEffectArea;

    public StatusEffectType effectType;
    public GameObject effectPrefab; // 타입별로 다른 프리팹 할당

    [Header("패시브 관리")]
    [Tooltip("현재 적용된 패시브 효과들의 리스트")]
    private List<string> activePassiveIDs = new List<string>();

    /// <summary> 패시브 ID별 자기 턴 시작 유즈 누적(TurnIntervalGrantStatus 등, 임계 도달 시 0으로 리셋).</summary>
    private readonly Dictionary<string, int> passiveOwnerTurnUseAccumulators = new Dictionary<string, int>();

    // 전투 연출 이벤트
    public System.Action<CharacterStats, int, bool, Vector3> OnTakeDamageEvent;
    public System.Action<CharacterStats, int, Vector3> OnHealEvent;
    public System.Action<CharacterStats> OnDeathEvent;
    public System.Action<CharacterStats, int, Vector3> OnBuffEvent; // 버프 이벤트 추가
    public System.Action<CharacterStats> OnCollapseCrisisEvent; // 붕괴 위기 상태 진입 이벤트

    public void SetData(CharacterData data)
    {
        this.data = data;
        bool traceMora = data != null && data.ID == "000007" && DebugTraceFlags.PassiveStatTraceMora;
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // 폴더 경로에서 "Stand" 모션 스프라이트를 기본으로 할당
        if (!string.IsNullOrEmpty(data.Sprite))
        {
            // data.Sprite는 이제 폴더 경로 (예: "UnitSprite/Player")
            string standPath = $"{data.Sprite}/Stand";
            Sprite sprite = Resources.Load<Sprite>(standPath);
            if (sprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"스탠드 스프라이트 로드 실패 또는 SpriteRenderer가 비어 있음: {standPath}");
            }
        }
        Label = data.Label;

        if (traceMora)
        {
            string passives = data.Passives != null ? string.Join(",", data.Passives) : "(null)";
            Debug.Log($"[PassiveTrace][SetData-Before] ID={data.ID} Label={data.Label} BaseStats Hp/MaxHp/Atk/Def={data.Hp}/{data.MaxHp}/{data.Atk}/{data.Def} Passives={passives}");
        }
        // 업그레이드 보너스를 포함한 최종 스탯 값 설정
        Hp = (int)data.GetFinalStatValue(TargetStat.Hp);
        MaxHp = (int)data.GetFinalStatValue(TargetStat.MaxHp);
        Atk = (int)data.GetFinalStatValue(TargetStat.Atk);
        Def = (int)data.GetFinalStatValue(TargetStat.Def);
        Evasion = data.GetFinalStatValue(TargetStat.Evasion);
        Accuracy = data.GetFinalStatValue(TargetStat.Accuracy);
        Speed = (int)data.GetFinalStatValue(TargetStat.Speed);
        CharacterId = data.ID; // CharacterId 설정 추가
        Pattern = data.Pattern; // Pattern 설정 추가 (AI 할당에 필요)
        Rarity = data.Rarity; // Rarity 설정 추가
        CollapseChance = 0f; // 붕괴 확률 초기화 (스테이지 시작 시 0%부터 시작)
        
        // 디버그: Pattern 설정 확인
        Debug.Log($"[CharacterStats] SetData - {Label} (ID: {CharacterId}): Pattern = {Pattern} (CharacterData.Pattern = {data.Pattern})");
        if (data.Skills.Count >= 4)
            Skills = data.Skills.Take(4).ToArray();
        
        // 패시브 효과 적용
        ApplyPassives(data.Passives);

        if (traceMora)
        {
            Debug.Log($"[PassiveTrace][SetData-After] ID={data.ID} RuntimeStats Hp/MaxHp/Atk/Def={Hp}/{MaxHp}/{Atk}/{Def} ActivePassives={string.Join(",", activePassiveIDs)}");
        }
        
        // Debug.Log($"[SetData 완료] ID: {data.ID}, HP: {Hp}, Atk: {Atk}, Sprite: {data.Sprite}");
    }
    public void TakeDamage(int dmg, float attackerAccuracy, SkillData skillData = null, Vector3? attackerPosition = null)
    {
        float CriticalRate = 0.05f;
        // 1. 크리티컬 확률 계산
        if ((Evasion - attackerAccuracy) < 0)
        {
            CriticalRate += Mathf.Abs(Evasion - attackerAccuracy);
        }
        else if ((Evasion - attackerAccuracy) > 0)
        {
            CriticalRate /= 2;
        }
        else
        {
            CriticalRate = 0.05f;
        }

        // 2. 실제 치명타 판정
        bool isCritical = UnityEngine.Random.value < CriticalRate;
        if (isCritical)
        {
            dmg = Mathf.RoundToInt(dmg * 2);
        }

        // === [여기서 피해무시 등 특수 효과 체크] ===
        // StatusEffectController에서 activeEffectPrefabs를 직접 가져와서 체크
        var controller = GetComponent<StatusEffectController>();
        
        if (controller != null)
        {
            var controllerEffects = controller.GetActiveEffectPrefabs();
            foreach (var effect in controllerEffects)
            {
                if (effect == null) continue;
                
                var instance = effect.GetComponent<StatusEffectInstanceReaction>();
                if (instance != null && instance.OnTakeDamage(ref dmg))
                {
                    // 피해가 무시되었으면 블록 효과 표시
                    Vector3 blockAttackerPos = attackerPosition ?? transform.position + Vector3.right * 2f;
                    if (BattleEffectManager.Instance != null)
                    {
                        BattleEffectManager.Instance.PlayBlockEffect(this, blockAttackerPos);
                    }
                    return;
                }
            }
        }
        
        Debug.Log($"[CharacterStats] {Label}: 피해무시 효과 없음. 최종 피해: {dmg}");

        // 3. 실제 체력 감소
        int oldHp = Hp;
        Hp -= dmg;
        Hp = Mathf.Max(0, Hp);
        
        Debug.Log($"[붕괴추적] TakeDamage - {Label}: 체력 {oldHp} → {Hp} (피해: {dmg}, 크리티컬: {isCritical})");

        // === 전투 연출 시스템 연동 ===
        // 이벤트 발생
        Vector3 attackerPos = attackerPosition ?? transform.position + Vector3.right * 2f;
        OnTakeDamageEvent?.Invoke(this, dmg, isCritical, attackerPos);

        // === KDP(넉다운 포인트) 처리 ===
        if (!IsPlayer && data != null && data.MaxKDP > 0 && skillData != null)
        {
            float multiplier = skillData.KnockdownMultiplier;
            if (multiplier > 0f)
            {
                data.KDP += Mathf.RoundToInt(dmg * multiplier);
                if (data.KDP >= data.MaxKDP)
                {
                    data.KDP = 0;
                    TriggerKnockdown();
                }
            }
        }

        HpUI.UpdateHpBar(Hp, MaxHp);
        
        // 체력이 0 이하가 되면 Deathcheck 호출
        if (Hp <= 0)
        {
            Debug.Log($"[붕괴추적] TakeDamage - 체력 0 이하 감지, Deathcheck() 호출 (직접 공격)");
        }
        Deathcheck();
        DeathAction();
    }

    public void Heal(int amount)
    {
        int oldHp = Hp;
        Hp += amount;
        Hp = Mathf.Min(Hp, MaxHp);
        Debug.Log($"[CharacterStats] {Label} 힐: {oldHp} -> {Hp} (힐량: {amount}, 최대체력: {MaxHp})");
        
        // 체력이 회복되면 방치 상태 해제
        if (Hp > 0)
        {
            isNeglected = false;
            Debug.Log($"[붕괴추적] {Label}: 체력 회복으로 방치 상태 해제");
        }
        
        // 1 이상의 회복을 받으면 붕괴 확률 감소 (아군만)
        if (amount >= 1)
        {
            bool isAllyNotMainCharacter = IsPlayer && !string.IsNullOrEmpty(CharacterId) && CharacterId != "000001";
            if (isAllyNotMainCharacter)
            {
                float oldChance = CollapseChance;
                CollapseChance -= 0.05f; // 5% 감소
                CollapseChance = Mathf.Max(CollapseChance, 0f); // 최소 0%
                
                Debug.Log($"[붕괴추적] {Label}: 체력 회복으로 붕괴 확률 감소");
                Debug.Log($"[붕괴추적] CollapseChance 감소: {oldChance:F3} → {CollapseChance:F3} ({(CollapseChance * 100):F1}%) [회복]");
            }
        }
        
        // === 전투 연출 시스템 연동 ===
        // 힐 이벤트 발생 (초록색 표시용)
        Vector3 healerPos = transform.position + Vector3.right * 2f; // 힐러 위치 (기본값)
        OnHealEvent?.Invoke(this, amount, healerPos);
        
        // UI 업데이트
        if (HpUI != null)
            HpUI.UpdateHpBar(Hp, MaxHp);
    }

    /// <summary>
    /// 버프 효과를 적용합니다.
    /// </summary>
    /// <param name="buffType">버프 타입 (예: "공격력", "방어력")</param>
    /// <param name="value">버프 수치</param>
    public void ApplyBuff(string buffType, int value)
    {
        Debug.Log($"[CharacterStats] {Label} 버프 적용: {buffType} +{value}");
        
        // === 전투 연출 시스템 연동 ===
        // 버프 이벤트 발생 (파란색 표시용)
        Vector3 casterPos = transform.position + Vector3.left * 2f; // 캐스터 위치 (기본값)
        OnBuffEvent?.Invoke(this, value, casterPos);
        
        // 버프 타입에 따른 실제 스탯 적용
        ApplyBuffToStats(buffType, value);
    }

    /// <summary>
    /// 버프 타입에 따라 실제 스탯에 적용합니다.
    /// </summary>
    private void ApplyBuffToStats(string buffType, int value)
    {
        switch (buffType)
        {
            case "공격력":
                Atk += value;
                Debug.Log($"[CharacterStats] {Label} 공격력 버프: +{value} (현재: {Atk})");
                break;
            case "방어력":
                Def += value;
                Debug.Log($"[CharacterStats] {Label} 방어력 버프: +{value} (현재: {Def})");
                break;
            case "속도":
                Speed += value;
                Debug.Log($"[CharacterStats] {Label} 속도 버프: +{value} (현재: {Speed})");
                break;
            case "회피율":
                Evasion += value;
                Debug.Log($"[CharacterStats] {Label} 회피율 버프: +{value} (현재: {Evasion})");
                break;
            case "명중률":
                Accuracy += value;
                Debug.Log($"[CharacterStats] {Label} 명중률 버프: +{value} (현재: {Accuracy})");
                break;
            default:
                Debug.Log($"[CharacterStats] {Label} 알 수 없는 버프 타입: {buffType}");
                break;
        }
    }

    public void Deathcheck()
    {
        if (Hp <= 0 && !IsDead)
        {
            Debug.Log($"[붕괴추적] Deathcheck 시작 - {Label} (ID: {CharacterId}, Hp: {Hp}, IsDead: {IsDead}, IsPlayer: {IsPlayer})");
            
            // 아군(주인공 제외)의 경우 붕괴 체크 먼저 수행
            bool isAllyNotMainCharacter = IsPlayer && !string.IsNullOrEmpty(CharacterId) && CharacterId != "000001";
            
            if (isAllyNotMainCharacter)
            {
                Debug.Log($"[붕괴추적] 아군(주인공 제외) 감지 - TryCollapse() 호출");
                Debug.Log($"[붕괴추적] 현재 CollapseChance: {CollapseChance:F2} ({(CollapseChance * 100):F1}%)");
                
                // 붕괴 체크 수행
                TryCollapse();
                
                // 붕괴에 실패하여 빈사 상태로 유지되는 경우
                if (!IsDead)
                {
                    Debug.Log($"[붕괴추적] 붕괴 실패 - 빈사 상태로 유지 (CollapseChance: {CollapseChance:F2})");
                    Debug.Log($"[붕괴추적] OnCollapseCrisisEvent 발생 - 구독자 수: {(OnCollapseCrisisEvent?.GetInvocationList().Length ?? 0)}");
                    
                    // 붕괴 위기 이벤트 발생 (응급 조치 축복 등에서 구독)
                    OnCollapseCrisisEvent?.Invoke(this);
                    
                    // 체력이 0이고 아직 죽지 않았으므로 방치 상태로 표시
                    SetNeglected();
                    
                    Debug.Log($"[붕괴추적] 빈사 상태 유지 완료 - 사망 처리하지 않음");
                    return; // 사망 처리하지 않고 빈사 상태로 유지
                }
                else
                {
                    Debug.Log($"[붕괴추적] 붕괴 성공 - 사망 처리 진행");
                }
            }
            else
            {
                Debug.Log($"[붕괴추적] 아군이 아니거나 주인공 - 즉시 사망 처리");
            }
            
            // 붕괴에 성공했거나, 적/주인공인 경우 사망 처리
            IsDead = true;
            Debug.Log($"[붕괴추적] IsDead = true 설정 완료 - {Label}");
            
            // 턴 블록 파괴 효과 호출
            if (NextTurnIndicatorUI.Instance != null)
            {
                NextTurnIndicatorUI.Instance.DestroyCharacterTurnBlock(this);
            }
            
            UpdateTurnIndicator();
            // 적이 죽을 때 BattleManager에 ID 직접 전달 (플레이어가 아닌 경우만)
            if (!IsPlayer && BattleManager.Instance != null && !string.IsNullOrEmpty(CharacterId))
            {
                BattleManager.Instance.OnEnemyDied(CharacterId);
            }
            // 전투 연출 이벤트 발생
            OnDeathEvent?.Invoke(this);
            // DeathAction()은 외부에서 호출
        }
    }

    private void Update()
    {
        // 보조 안전장치: 체력이 0 이하인데 IsDead가 false인 경우를 감지
        // 주의: 이는 일반적인 데스 체크(TakeDamage 등)가 제대로 작동하지 않은 경우를 위한 백업입니다.
        // 
        // 일반적인 데스 체크 흐름:
        // 1. TakeDamage() 호출 -> 체력 감소 -> Deathcheck() -> DeathAction()
        // 2. 이 경우 Update()에서 체크해도 이미 IsDead가 true이므로 실행되지 않음
        //
        // Update()가 먼저 실행되는 경우:
        // - TakeDamage()가 호출되지 않고 체력이 0이 된 경우 (예: 상태이상으로 인한 피해)
        // - 이 경우 Update()에서 Deathcheck()만 호출하고, DeathAction()은 상태이상 시스템 등에서 호출됨
        //
        // 아군의 붕괴 상태:
        // - 아군이 빈사 상태(Hp <= 0 && !IsDead)로 유지되는 것은 정상적인 상태입니다.
        // - Update()에서 아군의 빈사 상태를 다시 Deathcheck()로 호출하면 반복 체크가 발생하여 즉사할 수 있습니다.
        // - 따라서 Update()는 아군이 아닌 경우(주인공 또는 적)에만 작동합니다.
        if (Hp <= 0 && !IsDead)
        {
            // 아군(주인공 제외)의 빈사 상태는 정상 상태이므로 Update()에서 재체크하지 않음
            bool isAllyNotMainCharacter = IsPlayer && !string.IsNullOrEmpty(CharacterId) && CharacterId != "000001";
            if (!isAllyNotMainCharacter)
            {
                Debug.Log($"[죽음안전장치] {Label}: Update()에서 사망 체크 작동 (TakeDamage()를 거치지 않은 사망)");
                Deathcheck();
                // DeathAction()은 TakeDamage()나 상태이상 시스템 등에서 호출되므로 여기서는 호출하지 않음
            }
            // 아군 빈사 상태는 정상 상태이므로 로그 출력하지 않음 (Update()는 매 프레임 호출되므로)
        }
    }
    public void DeathAction()
    {
        if (IsDead)
        {
            // 모든 상태이상 프리팹 파괴
            foreach (var effect in activeEffectPrefabs)
            {
                if (effect != null)
                    Destroy(effect);
            }
            
            // 전투 연출 시스템을 통한 데스 연출
            // BattleEffectManager에서 연출이 끝난 후 오브젝트를 파괴하도록 함
            // Destroy(gameObject); // 이 부분을 제거하여 연출이 끝날 때까지 대기
        }
    }

    /// <summary>
    /// 새로운 상태이상 프리팹을 생성하고 등록
    /// </summary>
    /// <param name="effectId">상태이상 ID</param>
    /// <param name="duration">지속 턴 수</param>
    /// <param name="value">효과 수치</param>
    public void AddStatusEffectPrefab(StatusEffectData effectData, int duration, int value)
    {
        if (effectData == null)
        {
            Debug.LogWarning($"[CharacterStats] {Label}: 유효하지 않은 상태이상 데이터");
            return;
        }

        if (DebugTraceFlags.PassiveStatusEffectFlow)
            Debug.Log($"[StatusFxTrace] AddStatusEffectPrefab enter {Label} id={effectData.EffectID} type={effectData.effectType} dur={duration} val={value}");

        // StatusEffectController를 통해 타입별로 분기하여 적용
        var controller = GetComponent<StatusEffectController>();
        if (controller == null)
        {
            Debug.LogWarning($"[CharacterStats] {Label}: StatusEffectController가 없습니다.");
            return;
        }

        // 중복: 지속피해(ContinuousDamage)는 3안 — 재적용 시 들어온 수치의 절반(내림)만 합산, 지속은 유지. 그 외 타입은 기존처럼 무시.
        if (controller.HasStatusEffect(effectData.EffectID))
        {
            if (effectData.effectType == StatusEffectType.ContinuousDamage)
            {
                var existing = controller.GetStatusEffectInstance(effectData.EffectID);
                if (existing != null)
                {
                    int before = existing.value;
                    existing.MergeHalfIncomingDamage(value);
                    if (DebugTraceFlags.PassiveStatusEffectFlow)
                        Debug.Log($"[StatusFxTrace] AddStatusEffectPrefab merge CD {Label} id={effectData.EffectID} incoming={value} +{Mathf.FloorToInt(value / 2f)} → value {before}→{existing.value}");
                    else
                        Debug.Log($"[CharacterStats] {Label}: 지속피해 {effectData.effectName} 중첩 합산 (3안) incoming={value} → 총 피해량 {existing.value}");
                }
                return;
            }

            if (DebugTraceFlags.PassiveStatusEffectFlow)
                Debug.Log($"[StatusFxTrace] AddStatusEffectPrefab SKIP duplicate {Label} id={effectData.EffectID}");
            else
                Debug.Log($"[CharacterStats] {Label}: 상태이상 {effectData.effectName} (ID: {effectData.EffectID})이 이미 적용되어 있습니다. 중복 적용 무시.");
            return;
        }

        switch (effectData.effectType)
        {
            case StatusEffectType.ContinuousDamage:
                controller.AddCDamageEffect(effectData, duration, value);
                break;
            case StatusEffectType.Buff:
                if (DebugTraceFlags.PassiveStatusEffectFlow)
                    Debug.Log($"[StatusFxTrace] AddStatusEffectPrefab branch Buff → controller.AddBuffEffect");
                controller.AddBuffEffect(effectData, duration, value);
                break;
            case StatusEffectType.Debuff:
                // 디버프용 메서드가 있다면 여기에 추가
                // controller.AddDeBuffEffect(effectData, duration, value);
                break;
            case StatusEffectType.Stun:
                // 스턴 등 특수효과용 메서드가 있다면 여기에 추가
                break;
            case StatusEffectType.Token:
                controller.AddReactionEffect(effectData, duration, value);
                break;
            default:
                if (DebugTraceFlags.PassiveStatusEffectFlow)
                    Debug.LogWarning($"[StatusFxTrace] AddStatusEffectPrefab unsupported type {effectData.effectType} id={effectData.EffectID}");
                Debug.LogWarning($"[CharacterStats] {Label}: 지원하지 않는 상태이상 타입: {effectData.effectType}");
                break;
        }
    }
    public void TryCollapse()
    {
        Debug.Log($"[붕괴추적] TryCollapse 시작 - {Label} (ID: {CharacterId})");
        
        // 플레이어블 아군만, 그리고 주인공은 예외
        if (!IsPlayer)
        {
            Debug.Log($"[붕괴추적] TryCollapse 종료 - IsPlayer가 false");
            return;
        }
        if (CharacterId == "000001")
        {
            Debug.Log($"[붕괴추적] TryCollapse 종료 - 주인공은 붕괴 체크 불가");
            return;
        }

        if (Hp > 0)
        {
            Debug.Log($"[붕괴추적] TryCollapse 종료 - 체력이 0보다 큼 (Hp: {Hp})");
            return; // 체력이 0 이하일 때만
        }

        float rand = UnityEngine.Random.value; // 0~1
        Debug.Log($"[붕괴추적] 붕괴 확률 체크 - 랜덤값: {rand:F3}, CollapseChance: {CollapseChance:F3} ({(CollapseChance * 100):F1}%)");
        
        if (rand < CollapseChance)
        {
            // 즉시 붕괴(사망 처리)
            Debug.Log($"[붕괴추적] 붕괴 성공! (랜덤값 {rand:F3} < 확률 {CollapseChance:F3})");
            IsDead = true;
            Debug.Log($"[붕괴추적] IsDead = true 설정");
            // 사망 처리 로직 호출
        }
        else
        {
            // 붕괴하지 않고 빈사 상태로 버팀, 확률 증가 (직접 공격으로 인한 붕괴 실패)
            float oldChance = CollapseChance;
            CollapseChance += 0.2f; // 20% 증가
            CollapseChance = Mathf.Min(CollapseChance, 1f); // 최대 100%
            
            Debug.Log($"[붕괴추적] 붕괴 실패 - 빈사 상태로 버팀 (랜덤값 {rand:F3} >= 확률 {oldChance:F3})");
            Debug.Log($"[붕괴추적] CollapseChance 증가: {oldChance:F3} → {CollapseChance:F3} ({(CollapseChance * 100):F1}%) [직접 공격]");
            // 빈사 상태 유지
        }
    }
    
    /// <summary>
    /// 자신의 턴이 시작될 때 방치 체크를 수행합니다.
    /// 체력이 0인 상태로 방치되어 자신의 턴까지 체력을 회복하지 못했다면 붕괴 확률이 증가합니다.
    /// </summary>
    public void CheckNeglectOnTurnStart()
    {
        // 아군(주인공 제외)만 체크
        bool isAllyNotMainCharacter = IsPlayer && !string.IsNullOrEmpty(CharacterId) && CharacterId != "000001";
        if (!isAllyNotMainCharacter) return;
        
        // 체력이 0이고 아직 죽지 않았으며, 방치 상태인 경우
        if (Hp <= 0 && !IsDead && isNeglected)
        {
            float oldChance = CollapseChance;
            CollapseChance += 0.1f; // 10% 증가 (방치 페널티)
            CollapseChance = Mathf.Min(CollapseChance, 1f); // 최대 100%
            
            Debug.Log($"[붕괴추적] {Label}: 방치 체크 - 자신의 턴까지 체력 회복 실패");
            Debug.Log($"[붕괴추적] CollapseChance 증가: {oldChance:F3} → {CollapseChance:F3} ({(CollapseChance * 100):F1}%) [방치]");
            
            // 방치 체크 완료 (다음 턴까지 다시 체력이 0이면 또 체크됨)
            // isNeglected는 체력이 회복될 때까지 유지
        }
    }
    
    /// <summary>
    /// 체력이 0이고 아직 죽지 않은 상태를 방치 상태로 표시합니다.
    /// 어떤 방식으로든 체력이 0이 되어 방치되면 자신의 턴까지 체력을 회복하지 못하면 페널티가 적용됩니다.
    /// </summary>
    public void SetNeglected()
    {
        bool isAllyNotMainCharacter = IsPlayer && !string.IsNullOrEmpty(CharacterId) && CharacterId != "000001";
        if (isAllyNotMainCharacter && Hp <= 0 && !IsDead)
        {
            isNeglected = true;
            Debug.Log($"[붕괴추적] {Label}: 체력 0 도달 - 방치 상태로 표시");
        }
    }

    public void UpdateTurnIndicator()
    {
        if (NextTurnIndicatorUI.Instance != null)
        {
            var turnList = TurnManager.Instance.GetAliveTurnList();
            NextTurnIndicatorUI.Instance.CreateTurnBlocks(turnList);
        }
    }

    // 넉다운(스턴) 처리용 메서드(임시)
    private void TriggerKnockdown()
    {
        // TODO: 스턴/행동불가, 연출 등 실제 처리 추가
    }

    /// <summary>
    /// 자신의 턴이 시작되어 상태이상 정산이 끝난 뒤 호출합니다.
    /// 턴 연동 패시브 확장 절차는 <see cref="PassiveSystemExtensionGuide"/>.
    /// </summary>
    public void InvokePassivesOnOwnerTurnStart()
    {
        if (activePassiveIDs == null || activePassiveIDs.Count == 0) return;

        foreach (string passiveID in activePassiveIDs)
        {
            if (string.IsNullOrEmpty(passiveID)) continue;

            PassiveData passiveData = PassiveLoader.GetByIdStatic(passiveID);
            if (passiveData == null) continue;

            if (PassiveEffectLibrary.TryGetEffect(passiveData.passiveType, out PassiveEffectBase mappedEffect) &&
                mappedEffect != null)
            {
                mappedEffect.OnOwnerTurnStart(this, passiveData);
            }
            else if (passiveData.passiveType == PassiveType.CustomScript &&
                     TryCreateCustomPassiveEffect(passiveData.scriptClass, out PassiveEffectBase customEffect))
            {
                customEffect.OnOwnerTurnStart(this, passiveData);
            }
        }
    }

    /// <summary>
    /// 자기 턴 시작 시 패시브 유즈를 1 올리고, 발동 여부를 반환합니다.
    /// <paramref name="useCountThreshold"/>가 0 이하면 누적 없이 매번 true(상시). 1 이상이면 누적이 임계 이상일 때 true이고 누적은 0으로 리셋합니다.
    /// <paramref name="startUseCount"/>는 해당 패시브 누적을 처음 볼 때의 시작값(이후 발동 리셋 후에는 0부터).
    /// </summary>
    public bool TryTickOwnerTurnPassiveUseAndShouldFire(string passiveId, int useCountThreshold, int startUseCount = 0)
    {
        if (string.IsNullOrEmpty(passiveId)) return false;
        if (useCountThreshold <= 0)
            return true;

        if (!passiveOwnerTurnUseAccumulators.TryGetValue(passiveId, out int acc))
            acc = Mathf.Max(0, startUseCount);
        acc++;
        if (acc >= useCountThreshold)
        {
            passiveOwnerTurnUseAccumulators[passiveId] = 0;
            return true;
        }
        passiveOwnerTurnUseAccumulators[passiveId] = acc;
        return false;
    }

    /// <summary>
    /// 패시브 효과들을 캐릭터에 적용합니다.
    /// </summary>
    /// <param name="passiveIDs">적용할 패시브 ID 리스트</param>
    private void ApplyPassives(List<string> passiveIDs)
    {
        if (passiveIDs == null || passiveIDs.Count == 0) return;

        foreach (string passiveID in passiveIDs)
        {
            if (string.IsNullOrEmpty(passiveID)) continue;
            if (activePassiveIDs.Contains(passiveID)) continue;

            PassiveData passiveData = PassiveLoader.GetByIdStatic(passiveID);
            if (passiveData == null)
            {
                Debug.LogWarning($"[CharacterStats] {Label}: 패시브 데이터를 찾을 수 없습니다. ID={passiveID}");
                continue;
            }

            bool applied = false;

            if (PassiveEffectLibrary.TryGetEffect(passiveData.passiveType, out PassiveEffectBase mappedEffect) &&
                mappedEffect != null)
            {
                mappedEffect.Apply(this, passiveData);
                applied = true;
            }
            else if (passiveData.passiveType == PassiveType.CustomScript &&
                     TryCreateCustomPassiveEffect(passiveData.scriptClass, out PassiveEffectBase customEffect))
            {
                customEffect.Apply(this, passiveData);
                applied = true;
            }
            else if (passiveData.passiveType == PassiveType.None)
            {
                // 스탯부스트형(Type=None)은 CharacterData.GetFinalStatValue에서 상시 반영한다.
                applied = true;
            }

            if (!applied)
            {
                Debug.LogWarning($"[CharacterStats] {Label}: 적용 가능한 패시브 효과가 없습니다. ID={passiveID}, Type={passiveData.passiveType}, Script={passiveData.scriptClass}");
                continue;
            }

            activePassiveIDs.Add(passiveID);
            Debug.Log($"[CharacterStats] {Label}: 패시브 {passiveID} 적용 완료 (Type={passiveData.passiveType})");
        }
    }

    private bool TryCreateCustomPassiveEffect(string scriptClass, out PassiveEffectBase effect)
    {
        effect = null;
        if (string.IsNullOrEmpty(scriptClass)) return false;

        Type type = Type.GetType(scriptClass);
        if (type == null)
        {
            Debug.LogWarning($"[CharacterStats] {Label}: 스크립트 클래스를 찾을 수 없습니다. {scriptClass}");
            return false;
        }

        if (!typeof(PassiveEffectBase).IsAssignableFrom(type))
        {
            Debug.LogWarning($"[CharacterStats] {Label}: PassiveEffectBase를 상속하지 않은 클래스입니다. {scriptClass}");
            return false;
        }

        effect = Activator.CreateInstance(type) as PassiveEffectBase;
        return effect != null;
    }

    /// <summary>
    /// 현재 적용된 패시브 ID 리스트를 반환합니다.
    /// </summary>
    /// <returns>활성 패시브 ID 리스트</returns>
    public List<string> GetActivePassiveIDs()
    {
        return new List<string>(activePassiveIDs);
    }
}
