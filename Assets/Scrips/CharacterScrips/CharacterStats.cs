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

    // 전투 연출 이벤트
    public System.Action<CharacterStats, int, bool, Vector3> OnTakeDamageEvent;
    public System.Action<CharacterStats, int, Vector3> OnHealEvent;
    public System.Action<CharacterStats> OnDeathEvent;

    public void SetData(CharacterData data)
    {
        this.data = data;
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
        Hp = data.Hp;
        MaxHp = data.MaxHp;
        Atk = data.Atk;
        Def = data.Def;
        Evasion = data.EvasionRate;
        Accuracy = data.Accuracy;
        Speed = data.Speed;
        CharacterId = data.ID; // CharacterId 설정 추가
        if (data.Skills.Count >= 4)
            Skills = data.Skills.Take(4).ToArray();
        
        // 패시브 효과 적용
        ApplyPassives(data.Passives);
        
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
        bool damageBlocked = false;
        
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
                    damageBlocked = true;
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
        Hp -= dmg;
        Hp = Mathf.Max(0, Hp);

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
        Deathcheck();
        DeathAction();
    }

    public void Heal(int amount)
    {
        int oldHp = Hp;
        Hp += amount;
        Hp = Mathf.Min(Hp, MaxHp);
        Debug.Log($"[CharacterStats] {Label} 힐: {oldHp} -> {Hp} (힐량: {amount}, 최대체력: {MaxHp})");
        
        // === 전투 연출 시스템 연동 ===
        // 힐 이벤트 발생 (초록색 표시용)
        Vector3 healerPos = transform.position + Vector3.right * 2f; // 힐러 위치 (기본값)
        OnHealEvent?.Invoke(this, amount, healerPos);
        
        // UI 업데이트
        if (HpUI != null)
            HpUI.UpdateHpBar(Hp, MaxHp);
    }

    public void Deathcheck()
    {
        if (Hp <= 0 && !IsDead)
        {
            IsDead = true;
            
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

        // StatusEffectController를 통해 타입별로 분기하여 적용
        var controller = GetComponent<StatusEffectController>();
        if (controller == null)
        {
            Debug.LogWarning($"[CharacterStats] {Label}: StatusEffectController가 없습니다.");
            return;
        }

        // 중복 체크: 같은 상태이상이 이미 적용되어 있으면 새로 생성하지 않음
        if (controller.HasStatusEffect(effectData.EffectID))
        {
            Debug.Log($"[CharacterStats] {Label}: 상태이상 {effectData.effectName} (ID: {effectData.EffectID})이 이미 적용되어 있습니다. 중복 적용 무시.");
            return;
        }

        switch (effectData.effectType)
        {
            case StatusEffectType.ContinuousDamage:
                controller.AddCDamageEffect(effectData, duration, value);
                break;
            case StatusEffectType.Buff:
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
                Debug.LogWarning($"[CharacterStats] {Label}: 지원하지 않는 상태이상 타입: {effectData.effectType}");
                break;
        }
    }
    public void TryCollapse()
    {
        // 플레이어블 아군만, 그리고 주인공은 예외
        if (!IsPlayer) return;
        if (CharacterId == "000001") return;

        if (Hp > 0) return; // 체력이 0 이하일 때만

        float rand = UnityEngine.Random.value; // 0~1
        if (rand < CollapseChance)
        {
            // 즉시 붕괴(사망 처리)
            IsDead = true;
            // 사망 처리 로직 호출
        }
        else
        {
            // 붕괴하지 않고 빈사 상태로 버팀, 확률 증가
            CollapseChance += 0.2f; // 20% 증가
            CollapseChance = Mathf.Min(CollapseChance, 1f); // 최대 100%
            // 빈사 상태 유지
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
    /// 패시브 효과들을 캐릭터에 적용합니다.
    /// </summary>
    /// <param name="passiveIDs">적용할 패시브 ID 리스트</param>
    private void ApplyPassives(List<string> passiveIDs)
    {
        if (passiveIDs == null || passiveIDs.Count == 0) return;

        foreach (string passiveID in passiveIDs)
        {
            if (string.IsNullOrEmpty(passiveID)) continue;

            // 패시브 ID를 활성 리스트에 추가 (임시)
            activePassiveIDs.Add(passiveID);
            Debug.Log($"[CharacterStats] {Label}: 패시브 {passiveID}가 적용되었습니다.");
        }
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
