using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public string Label;
    public int Hp, Atk, Def;
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
        Atk = data.Atk;
        Def = data.Def;
        Evasion = data.EvasionRate;
        Accuracy = data.Accuracy;
        Speed = data.Speed;
        if (data.Skills.Count >= 4)
            Skills = data.Skills.Take(4).ToArray();
        Debug.Log($"[SetData 완료] ID: {data.ID}, HP: {Hp}, Atk: {Atk}, Sprite: {data.Sprite}");
    }

}
