using UnityEngine;

public class HpUIHandler : MonoBehaviour
{
    public Transform target; // 따라갈 캐릭터
    public SpriteRenderer hpBarFront;
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    public void UpdateHpBarPosition()
    {
        if (target != null)
            transform.position = target.position + offset;
    }
    public void UpdateHpBar(int hp, int maxHp)
    {
        float ratio = Mathf.Clamp01((float)hp / maxHp);
        if (hpBarFront != null)
        {
        hpBarFront.transform.localScale = new Vector3(ratio, 1, 1);
        float barWidth = 1f;
        hpBarFront.transform.localPosition = new Vector3((ratio - 1) * barWidth * 0.5f, 0, 0);
    }
    }
}
