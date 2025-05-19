using UnityEngine;

public class CharacterClickHandler : MonoBehaviour
{
    public CharacterStats stats;
    void Awake()
    {
        if (stats == null)
            stats = GetComponent<CharacterStats>();
    }
    void OnMouseDown()
    {
        //Debug.Log($"클릭됨: {gameObject.name}, 체력: {stats?.Hp}");

        if (stats != null)
        {
            FindFirstObjectByType<TargetSelector>()?.SelectByClick(stats);
        }
        else
        {
            Debug.LogWarning("CharacterStats가 연결되지 않음!");
        }
    }
}
