using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TargetSelector : MonoBehaviour
{
    public static TargetSelector Instance { get; private set; }

    

    public CharacterStats CurrentTarget;
    public GameObject SelectedTarget;
    [SerializeField] private RectTransform targetMarker;
    public RectTransform targetMarkerImage;
    //현재 타겟과 게임오브젝트로서 타겟을 이중으로 선택상태로 둔다.
    //이중 게임 오브젝트가 감지되지 않는담 그걸 죽은 걸로 본다.

    void Awake()
    {
        Instance = this;
        if (targetMarker == null)
        {
            // 자식 중에서 "TargetMarker"라는 이름을 가진 UI 오브젝트 자동 할당
            Transform found = transform.Find("TargetMarker"); // 경로 수정 가능
            if (found != null)
                targetMarker = found.GetComponent<RectTransform>();
        }
    }
    public void AutoSelectTarget(List<CharacterStats> enemySlots)
    {
        foreach (var enemy in enemySlots)
        {
            if (enemy != null && enemy.gameObject != null)
            {
                CurrentTarget = enemy;
                SelectedTarget = enemy.gameObject;
                return;
            }
        }

        CurrentTarget = null; // 전부 죽었을 경우
    }
    public void SelectByClick(CharacterStats clicked) //얘는 타겟을 본격적으로 지정하기 위해
    {
        //Debug.Log($"[DEBUG] 선택된 대상: {clicked.name}, 태그: {clicked.tag}, 체력: {clicked.Hp}");
        if (clicked == null || clicked.Hp <= 0)
        {
            Debug.Log("타겟 무효 (null 또는 체력 0이하)");
            return;
        }
        //Debug.Log($"적 클릭됨:{clicked.name}");
        CurrentTarget = clicked;
        SelectedTarget = clicked.gameObject;
        SetTarget(clicked);
    }
    public CharacterStats GetCurrentTarget() //얘는 클릭후 타겟 정보를 스킬로 보내는 애
    {
        if (CurrentTarget != null && CurrentTarget.Hp > 0)
            return CurrentTarget;

        return null; // 죽었거나 지정되지 않은 경우
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
        targetMarker.gameObject.SetActive(true);
    }
    public void HideSelector()//얘는 턴쪽에서 불러올 메서드
    {
        targetMarker.gameObject.SetActive(false);
        CurrentTarget = null;
    }

    public void ShowSelector()
    {
        targetMarker.gameObject.SetActive(true);
    }

    public void AutoSelectFirstEnemy()
    {
        CharacterStats firstTarget = TurnManager.Instance.allSlots
        .Select(slot => slot.currentCharacter)
        .FirstOrDefault(c =>
            c != null &&
            !c.Equals(null) &&
            c.IsPlayer == false &&
            !c.IsDead);

        if (firstTarget != null)
        {
            //Debug.Log($"[자동 타겟팅 대상] {firstTarget.name}");
            SetTarget(firstTarget);
        }
        else
        {
            Debug.LogWarning("타겟팅 가능한 적이 없음");
            targetMarkerImage.gameObject.SetActive(false); // 마커 감추기
        }
    }
}
