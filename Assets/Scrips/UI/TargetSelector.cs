using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetSelector : MonoBehaviour
{
    public static TargetSelector Instance { get; private set; }

    

    public CharacterStats CurrentTarget;
    public GameObject SelectedTarget;
    [SerializeField] private RectTransform targetMarker;
    public RectTransform targetMarkerImage;
    //���� Ÿ�ٰ� ���ӿ�����Ʈ�μ� Ÿ���� �������� ���û��·� �д�.
    //���� ���� ������Ʈ�� �������� �ʴ´� �װ� ���� �ɷ� ����.

    void Awake()
    {
        Instance = this;
        if (targetMarker == null)
        {
            // �ڽ� �߿��� "TargetMarker"��� �̸��� ���� UI ������Ʈ �ڵ� �Ҵ�
            Transform found = transform.Find("TargetMarker"); // ��� ���� ����
            if (found != null)
                targetMarker = found.GetComponent<RectTransform>();
        }
       
    }
    public void AutoSelectTarget(List<CharacterStats> enemySlots)
    {
        foreach (var enemy in enemySlots)
        {
            if (enemy != null && enemy.Hp > 0)
            {
                CurrentTarget = enemy;
                SelectedTarget = enemy.gameObject;
                return;
            }
        }

        CurrentTarget = null; // ���� �׾��� ���
    }
    public void SelectByClick(CharacterStats clicked) //��� Ÿ���� ���������� �����ϱ� ����
    {
        Debug.Log($"[DEBUG] ���õ� ���: {clicked.name}, �±�: {clicked.tag}, ü��: {clicked.Hp}");
        if (clicked == null || clicked.Hp <= 0)
        {
            Debug.Log("Ÿ�� ��ȿ (null �Ǵ� ü�� 0����)");
            return;
        }
        if (!clicked.CompareTag("Enemy"))
        {
            Debug.Log("���� �ƴ� Ÿ�� ����");
            return;
        }
        Debug.Log($"�� Ŭ����:{clicked.name}");
        CurrentTarget = clicked;
        SelectedTarget = clicked.gameObject;
        SetTarget(clicked);
    }
    public CharacterStats GetCurrentTarget() //��� Ŭ���� Ÿ�� ������ ��ų�� ������ ��
    {
        if (CurrentTarget != null && CurrentTarget.Hp > 0)
            return CurrentTarget;

        return null; // �׾��ų� �������� ���� ���
    }

    public void ClearTarget() => CurrentTarget = null;
    public void SetTarget(CharacterStats newTarget)
    {
        if (newTarget == null || newTarget.Hp <= 0)
        {
            targetMarker.gameObject.SetActive(false);
            CurrentTarget = null;
            return;
        }

        CurrentTarget = newTarget;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(CurrentTarget.transform.position);
        screenPos.y += 200f;
        targetMarkerImage.position = screenPos;
        targetMarkerImage.gameObject.SetActive(true);
    }
}
