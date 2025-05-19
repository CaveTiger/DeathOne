using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class CharacterStats : MonoBehaviour
{
    public string Label;
    public int Hp, MaxHp, Atk, Def;
    public float Evasion, Accuracy;
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

    [Header("상태이상 관리")]
    [Tooltip("현재 적용된 상태이상 프리팹들의 리스트")]
    private List<GameObject> activeEffectPrefabs = new List<GameObject>();
    
    [Tooltip("상태이상 UI가 생성될 영역")]
    [SerializeField] private Transform statusEffectArea;

    public void SetData(CharacterData data)
    {
        this.data = data;
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        // 스프라이트 적용
        if (!string.IsNullOrEmpty(data.Sprite))
        {
            Sprite sprite = Resources.Load<Sprite>(data.Sprite);
            if (sprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
                Debug.Log("스프라이트 로드됨");
            }
            else
            {
                Debug.LogWarning($"스프라이트 로드 실패 또는 SpriteRenderer가 비어 있음: {data.Sprite}");
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
        if (data.Skills.Count >= 4)
            Skills = data.Skills.Take(4).ToArray();
        // Debug.Log($"[SetData 완료] ID: {data.ID}, HP: {Hp}, Atk: {Atk}, Sprite: {data.Sprite}");
    }
    public void TakeDamage(int dmg, float attackerAccuracy, SkillData skillData = null)
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
        foreach (var effect in activeEffectPrefabs)
        {
            if (effect == null) continue;
            var instance = effect.GetComponent<StatusEffectInstance>();
            if (instance != null && instance.OnTakeDamage(ref dmg))
            {
                // 피해가 무시되었으면 더 이상 처리하지 않고 return
                return;
            }
        }

        // 3. 실제 체력 감소
        Hp -= dmg;
        Hp = Mathf.Max(0, Hp);
        Debug.Log(isCritical ? $"치명타피해 {dmg} → 현재 체력 {Hp}" : $"피해 {dmg} → 현재 체력 {Hp}");

        StartCoroutine(HitEffect());
        HpUI.UpdateHpBar(Hp, MaxHp);
        Deathcheck();
        DeathAction();
    }

    public void Heal(int amount)
    {
        Hp += amount;
        Hp = Mathf.Min(Hp, MaxHp);
        Debug.Log($"회복 {amount} → 현재 체력 {Hp}");
    }

    public void Deathcheck()
    {
        if (Hp <= 0 && !IsDead)
        {
            IsDead = true;
            Debug.Log($"{Label} 사망 처리");
            // 죽음 연출, 이벤트 등 추가 가능
            // DeathAction()은 외부에서 호출
        }
    }
    public void DeathAction()
    {
        if (IsDead)
        {
            // 죽음 연출(애니메이션 등) 후 Destroy 호출
            Destroy(gameObject);
        }
    }
    private IEnumerator HitEffect()
    {
        SpriteRenderer sr = spriteRenderer; // 이미 있는 필드로 가정
        Color original = sr.color;

        sr.color = Color.red; // 빨간색 피격 표시
        yield return new WaitForSeconds(1.2f); // 지속 시간
        sr.color = original;
    }
    /// <summary>
    /// 새로운 상태이상 프리팹을 생성하고 등록
    /// </summary>
    /// <param name="effectData">상태이상 데이터</param>
    /// <param name="duration">지속 턴 수</param>
    /// <param name="value">효과 수치</param>
    public void AddStatusEffectPrefab(StatusEffectData effectData, int duration, int value)
    {
        Debug.Log($"[AddStatusEffectPrefab] {Label}에게 {effectData?.effectName} 적용, duration: {duration}, value: {value}");
        if (effectData == null)
        {
            Debug.LogWarning($"[CharacterStats] {Label}: 유효하지 않은 상태이상 데이터");
            return;
        }

        // 이미 같은 효과가 있는지 확인
        var existingEffect = activeEffectPrefabs.Find(e => e != null && e.GetComponent<StatusEffectInstance>()?.EffectData == effectData);
            
        if (existingEffect != null)
        {
            // 중첩 처리
            var effectInstance = existingEffect.GetComponent<StatusEffectInstance>();
            effectInstance.Initialize(effectData, effectInstance.remainingTurns + duration, 
                                    effectInstance.value + value, this);
            Debug.Log($"[CharacterStats] {Label}: {effectData.effectName} 중첩 적용 (지속: {duration}턴, 수치: {value})");
        }
        else
        {
            // 새로운 프리팹 생성
            if (effectData.effectPrefab != null && statusEffectArea != null)
            {
                GameObject effectObj = Instantiate(effectData.effectPrefab, statusEffectArea);
                // 프리팹 이름을 효과명+ID 등으로 변경
                effectObj.name = $"StatusEffect_{effectData.effectName}_{effectData.EffectID}";
                var effectInstance = effectObj.GetComponent<StatusEffectInstance>();
                effectInstance.Initialize(effectData, duration, value, this);
                activeEffectPrefabs.Add(effectObj);
                Debug.Log($"[CharacterStats] {Label}: {effectData.effectName} 적용 (지속: {duration}턴, 수치: {value})");
            }
            else
            {
                Debug.LogWarning($"[CharacterStats] {Label}: 상태이상 프리팹 또는 영역이 없음");
            }
        }
    }
    public void ApplyStatusEffectsOnTurnStart()
    {
        // 1. 파괴된 오브젝트(null) 정리
        activeEffectPrefabs.RemoveAll(e => e == null);

        // 2. 순회하며 null 체크
        foreach (var effect in activeEffectPrefabs)
        {
            if (effect == null) continue;
            var instance = effect.GetComponent<StatusEffectInstance>();
            if (instance == null) continue;

            // 상태이상 효과 적용
            bool effectApplied = instance.OnTurnStart();
            
            // 효과가 적용된 경우에만 턴 감소
            if (effectApplied)
            {
                instance.OnTurnEnd();
            }
        }
    }
}
