using System;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public string Label;
    public int Hp, MaxHp, Atk, Def;
    public float Evasion, Accuracy;
    public int Speed;
    public string[] Skills = new string[4];

    public CharacterData data;
    public SpriteRenderer spriteRenderer;

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
        Debug.Log($"[SetData 완료] ID: {data.ID}, HP: {Hp}, Atk: {Atk}, Sprite: {data.Sprite}");
    }
    public void TakeDamage(int dmg, float attackerAccuracy)
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

        // 3. 실제 치명타 판정
        bool isCritical = UnityEngine.Random.value < CriticalRate;

        if (isCritical)
        {
            dmg = Mathf.RoundToInt(dmg * 2); // 배율은 원하는 만큼 (1.5x, 2x 등)
            Hp -= dmg;
            Hp = Mathf.Max(0, Hp);
            Debug.Log($"치명타피해 {dmg} → 현재 체력 {Hp}");
        }
        else
        {
            Hp -= dmg;
            Hp = Mathf.Max(0, Hp);
            Debug.Log($"피해 {dmg} → 현재 체력 {Hp}");
        }

        
    }

    public void Heal(int amount)
    {
        Hp += amount;
        Hp = Mathf.Min(Hp, MaxHp);
        Debug.Log($"회복 {amount} → 현재 체력 {Hp}");
    }
}
