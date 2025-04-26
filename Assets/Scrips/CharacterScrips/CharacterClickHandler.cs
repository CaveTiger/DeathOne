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
        Debug.Log($"Ŭ����: {gameObject.name}, ü��: {stats?.Hp}");

        if (stats != null)
        {
            FindObjectOfType<TargetSelector>()?.SelectByClick(stats);
        }
        else
        {
            Debug.LogWarning("CharacterStats�� ������� ����!");
        }
    }
}
