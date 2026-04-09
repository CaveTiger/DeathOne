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
    // [SerializeField] private CharacterInfoEnemy enemyInfoUI; // 인스펙터에서 할당 - 임시 주석처리
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
    
    void Update()
    {
        // 전투 중이면 타겟셀렉터 숨기기
        if (IsInCombatAction())
        {
            if (targetMarker != null && targetMarker.gameObject.activeSelf)
            {
                HideSelector();
            }
        }
    }
    
    /// <summary>
    /// 전투 행동 수행 중인지 확인 (BattleUIManager의 IsInBattleMode 플래그 사용)
    /// </summary>
    /// <returns>true면 전투 행동 수행 중 (타겟셀렉터 비활성화), false면 통상 상태 (타겟셀렉터 활성화)</returns>
    private bool IsInCombatAction()
    {
        // BattleUIManager 싱글톤을 통해 전투 모드 상태 확인
        if (BattleUIManager.Instance == null)
        {
            // BattleUIManager가 없으면 통상 상태로 간주
            return false;
        }
        
        // IsInBattleMode 플래그로 전투 상태 확인
        // ChangeUIBattle()에서 true로 설정, ChangeUINormal()에서 false로 설정
        return BattleUIManager.Instance.IsInBattleMode;
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
        // 전투 중이면 타겟 선택 불가
        if (IsInCombatAction())
        {
            return;
        }
        
        //Debug.Log($"[DEBUG] 선택된 대상: {clicked.name}, 태그: {clicked.tag}, 체력: {clicked.Hp}");
        // 빈사(Hp≤0, IsDead=false) 아군 케어용으로 체력 0도 선택 허용. 사망 처리된 대상만 제외.
        if (clicked == null || clicked.IsDead)
        {
            Debug.Log("타겟 무효 (null 또는 사망 처리됨)");
            return;
        }
        //Debug.Log($"적 클릭됨:{clicked.name}");
        CurrentTarget = clicked;
        SelectedTarget = clicked.gameObject;
        SetTarget(clicked);
    }
    public CharacterStats GetCurrentTarget() //얘는 클릭후 타겟 정보를 스킬로 보내는 애
    {
        if (CurrentTarget != null && !CurrentTarget.IsDead)
            return CurrentTarget;

        return null; // 사망했거나 지정되지 않은 경우
    }

    public void ClearTarget() => CurrentTarget = null;
    public void SetTarget(CharacterStats newTarget, bool ignoreCombatLock = false)
    {
        // 전투 중이면 타겟 설정 불가
        if (!ignoreCombatLock && IsInCombatAction())
        {
            return;
        }
        
        if (newTarget == null || newTarget.IsDead)
        {
            targetMarker.gameObject.SetActive(false);
            CurrentTarget = null;
            // if (enemyInfoUI != null) enemyInfoUI.HideInfo(); // 임시 주석처리
            NotifySelectedTargetInfoUI();
            return;
        }

        CurrentTarget = newTarget;

        // UI 정보 갱신
        // if (enemyInfoUI != null) enemyInfoUI.SetCharacterStats(CurrentTarget); // 임시 주석처리

        // 월드 공간에서 직접 위치 설정
        Vector3 worldPos = CurrentTarget.transform.position + new Vector3(0, 2f, 0);
        targetMarkerImage.position = worldPos;
        targetMarker.gameObject.SetActive(true);
        NotifySelectedTargetInfoUI();
    }
    public void HideSelector()//얘는 턴쪽에서 불러올 메서드
    {
        targetMarker.gameObject.SetActive(false);
        CurrentTarget = null;
        NotifySelectedTargetInfoUI();
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
            SetTarget(firstTarget, true);
        }
        else
        {
            Debug.LogWarning("타겟팅 가능한 적이 없음");
            targetMarkerImage.gameObject.SetActive(false); // 마커 감추기
            NotifySelectedTargetInfoUI();
        }
    }

    /// <summary>
    /// 선택 대상 역할로 지정된 CharacterInfo UI에 현재 타겟을 전달합니다.
    /// </summary>
    private void NotifySelectedTargetInfoUI()
    {
        var infos = FindObjectsByType<CharacterInfo>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var target = GetCurrentTarget();

        foreach (var info in infos)
        {
            if (info == null || !info.IsSelectedTargetInfo()) continue;

            if (target != null)
                info.SetCharacterStats(target);
            // UI는 항상 켜두는 정책이므로 target이 null일 때는 숨기지 않음
        }
    }
}
