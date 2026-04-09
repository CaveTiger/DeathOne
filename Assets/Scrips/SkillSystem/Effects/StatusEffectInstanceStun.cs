using UnityEngine;

/// <summary>
/// 기절(행동불가) 토큰: 지속 턴·수치 없이 프리팹 1개 = 다음 본인 턴 1회 스킵 시 소모.
/// </summary>
public class StatusEffectInstanceStun : MonoBehaviour
{
    [SerializeField] private StatusEffectData effectData;
    public StatusEffectData EffectData => effectData;

    public bool isActive = true;

    public void Initialize(StatusEffectData data)
    {
        effectData = data;

        var iconRenderer = GetComponent<SpriteRenderer>();
        if (iconRenderer == null)
            iconRenderer = GetComponentInChildren<SpriteRenderer>();
        if (iconRenderer != null && data != null)
        {
            Sprite icon = data.GetIcon();
            if (icon != null)
                iconRenderer.sprite = icon;
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            var box = gameObject.AddComponent<BoxCollider2D>();
            if (iconRenderer != null && iconRenderer.sprite != null)
                box.size = iconRenderer.sprite.bounds.size;
            else
                box.size = new Vector2(0.5f, 0.5f);
            box.isTrigger = true;
        }
    }
}
