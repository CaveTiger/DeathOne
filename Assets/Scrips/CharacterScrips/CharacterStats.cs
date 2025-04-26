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
        // ��������Ʈ ����
        if (!string.IsNullOrEmpty(data.Sprite))
        {
            Sprite sprite = Resources.Load<Sprite>(data.Sprite);
            if (sprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
                Debug.Log("��������Ʈ �ε��");
            }
            else
            {
                Debug.LogWarning($"��������Ʈ �ε� ���� �Ǵ� SpriteRenderer�� ��� ����: {data.Sprite}");
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
        Debug.Log($"[SetData �Ϸ�] ID: {data.ID}, HP: {Hp}, Atk: {Atk}, Sprite: {data.Sprite}");
    }
    public void TakeDamage(int dmg, float attackerAccuracy)
    {
        float CriticalRate = 0.05f;
        // 1. ũ��Ƽ�� Ȯ�� ���
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

        // 3. ���� ġ��Ÿ ����
        bool isCritical = UnityEngine.Random.value < CriticalRate;

        if (isCritical)
        {
            dmg = Mathf.RoundToInt(dmg * 2); // ������ ���ϴ� ��ŭ (1.5x, 2x ��)
            Hp -= dmg;
            Hp = Mathf.Max(0, Hp);
            Debug.Log($"ġ��Ÿ���� {dmg} �� ���� ü�� {Hp}");
        }
        else
        {
            Hp -= dmg;
            Hp = Mathf.Max(0, Hp);
            Debug.Log($"���� {dmg} �� ���� ü�� {Hp}");
        }

        
    }

    public void Heal(int amount)
    {
        Hp += amount;
        Hp = Mathf.Min(Hp, MaxHp);
        Debug.Log($"ȸ�� {amount} �� ���� ü�� {Hp}");
    }
}
