using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 업그레이드 스탯 조작 패널
/// 각 스탯(HP, ATK, DEF, Speed 등)의 아이콘, 현재 값, 사용량을 표시하고 관리합니다.
/// </summary>
public class UpgradeStat : MonoBehaviour
{
    private const int HpStepPerClick = 10;
    private const float RateStatStepPerClick = 0.05f;

    [Header("스탯 설정")]
    [Tooltip("이 패널이 관리할 스탯 타입")]
    [SerializeField] private TargetStat statType = TargetStat.None;

    [Header("UI 요소")]
    [Tooltip("스탯 아이콘 이미지")]
    [SerializeField] private Image icon;

    [Tooltip("현재 스탯 값 표시 텍스트")]
    [SerializeField] private TextMeshProUGUI value;

    [Tooltip("사용량 표시 텍스트")]
    [SerializeField] private TextMeshProUGUI usage;

    [Header("대상 캐릭터")]
    [Tooltip("업그레이드할 대상 캐릭터 데이터 (CharacterUpgradePannel에서 설정)")]
    private CharacterData targetCharacter;

    /// <summary>
    /// 대상 캐릭터를 설정합니다.
    /// </summary>
    /// <param name="character">대상 캐릭터 데이터</param>
    public void SetTargetCharacter(CharacterData character)
    {
        targetCharacter = character;
        UpdateDisplay();
    }

    /// <summary>
    /// 스탯을 증가시킵니다. (버튼 연결용)
    /// </summary>
    public void IncreaseStat()
    {
        if (targetCharacter == null)
        {
            Debug.LogWarning("[UpgradeStat] 대상 캐릭터가 설정되지 않았습니다.");
            return;
        }

        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("[UpgradeStat] GameProgressManager.Instance가 null입니다.");
            return;
        }

        // 이번 단계 업그레이드에 필요한 영혼먼지 코스트 계산 (누진: 1,2,3,...)
        int cost = GetNextUpgradeCost();
        if (cost <= 0)
        {
            Debug.LogWarning($"[UpgradeStat] 잘못된 코스트 계산 결과: {cost}");
            return;
        }

        // 등급 한도 및 보유 영혼먼지 검증 + 투자량 누적
        if (!GameProgressManager.Instance.TryInvestSoulDust(targetCharacter, cost))
        {
            // 한도 초과 또는 영혼먼지 부족
            return;
        }

        // 실제 스탯 증가 적용
        if (ApplyStatChange(1))
            NotifySoulDustDisplayIfPossible();
    }

    /// <summary>
    /// 스탯을 감소시킵니다. (버튼 연결용)
    /// </summary>
    public void DecreaseStat()
    {
        if (targetCharacter == null)
        {
            Debug.LogWarning("[UpgradeStat] 대상 캐릭터가 설정되지 않았습니다.");
            return;
        }

        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("[UpgradeStat] GameProgressManager.Instance가 null입니다.");
            return;
        }

        // 마지막으로 올린 1단계에 지불했던 비용 (Increase 시점: 보너스가 n이면 비용 n)
        int refund = GetRefundForRemovingLastStep();
        if (refund <= 0)
            return;

        if (!ApplyStatChange(-1))
            return;

        GameProgressManager.Instance.RefundSoulDustInvestment(targetCharacter, refund);
        NotifySoulDustDisplayIfPossible();
    }

    /// <summary>
    /// 스탯 변경을 적용합니다. (upgradeBonus 필드만 조작)
    /// </summary>
    /// <param name="delta">변경량 (양수: 증가, 음수: 감소)</param>
    /// <returns>적용 성공 여부</returns>
    private bool ApplyStatChange(int delta)
    {
        if (targetCharacter == null) return false;

        // upgradeBonus 필드만 조작 (기본 스탯은 유지)
        switch (statType)
        {
            case TargetStat.Hp:
            case TargetStat.MaxHp:
                // 업그레이드 UI에서는 HP와 MaxHP를 동일한 스탯으로 취급
                // 실제 업그레이드는 MaxHP 보너스만 사용하고, HP는 항상 MaxHP 최종값으로 맞춘다.
                targetCharacter.upgradeMaxHpBonus += delta * HpStepPerClick;

                // 최소값 보장: 최종 MaxHP가 1 미만이면 변경 취소
                if (targetCharacter.GetFinalStatValue(TargetStat.MaxHp) < 1)
                {
                    targetCharacter.upgradeMaxHpBonus -= delta * HpStepPerClick;
                    return false;
                }

                // 업그레이드 패널은 전투 밖에서만 쓰이므로,
                // 업그레이드 후에는 항상 HP를 최종 MaxHP 값으로 동기화(풀피)한다.
                int finalMaxHp = (int)targetCharacter.GetFinalStatValue(TargetStat.MaxHp);
                targetCharacter.Hp = finalMaxHp;
                break;

            case TargetStat.Atk:
                targetCharacter.upgradeAtkBonus += delta;
                // 최소값 보장
                if (targetCharacter.GetFinalStatValue(TargetStat.Atk) < 0)
                {
                    targetCharacter.upgradeAtkBonus -= delta; // 변경 취소
                    return false;
                }
                break;

            case TargetStat.Def:
                targetCharacter.upgradeDefBonus += delta;
                // 최소값 보장
                if (targetCharacter.GetFinalStatValue(TargetStat.Def) < 0)
                {
                    targetCharacter.upgradeDefBonus -= delta; // 변경 취소
                    return false;
                }
                break;

            case TargetStat.Speed:
                targetCharacter.upgradeSpeedBonus += delta;
                // 최소값 보장
                if (targetCharacter.GetFinalStatValue(TargetStat.Speed) < 0)
                {
                    targetCharacter.upgradeSpeedBonus -= delta; // 변경 취소
                    return false;
                }
                break;

            case TargetStat.Evasion:
                targetCharacter.upgradeEvasionBonus += delta * RateStatStepPerClick;
                // 최소값 보장
                if (targetCharacter.GetFinalStatValue(TargetStat.Evasion) < 0)
                {
                    targetCharacter.upgradeEvasionBonus -= delta * RateStatStepPerClick; // 변경 취소
                    return false;
                }
                break;

            case TargetStat.Accuracy:
                targetCharacter.upgradeAccuracyBonus += delta * RateStatStepPerClick;
                // 최소값 보장
                if (targetCharacter.GetFinalStatValue(TargetStat.Accuracy) < 0)
                {
                    targetCharacter.upgradeAccuracyBonus -= delta * RateStatStepPerClick; // 변경 취소
                    return false;
                }
                break;

            default:
                Debug.LogWarning($"[UpgradeStat] 지원하지 않는 스탯 타입: {statType}");
                return false;
        }

        // UI 업데이트
        UpdateDisplay();
        return true;
    }

    private void NotifySoulDustDisplayIfPossible()
    {
        var panel = GetComponentInParent<CharacterUpgradePannel>();
        if (panel != null)
            panel.UpdateSoulDustDisplay();
    }

    /// <summary>
    /// 현재 업그레이드 보너스를 기준으로, 다음 1단계 업그레이드에 필요한 영혼먼지 코스트를 계산합니다.
    /// 예: 보너스가 0이면 1, 보너스가 1이면 2, ... (누진 비용)
    /// </summary>
    /// <returns>다음 단계 업그레이드 비용</returns>
    private int GetNextUpgradeCost()
    {
        if (targetCharacter == null) return 0;

        int steps = GetCurrentStepCount();
        return steps + 1; // n번째 → 비용 n+1 (1,2,3,...)
    }

    /// <summary>
    /// HP/MaxHP는 MaxHp 보너스 기준으로 코스트/환급을 맞춤.
    /// </summary>
    private TargetStat GetBonusStatForCost()
    {
        if (statType == TargetStat.Hp || statType == TargetStat.MaxHp)
            return TargetStat.MaxHp;
        return statType;
    }

    /// <summary>
    /// 현재 업그레이드 보너스를 '클릭 단계'로 환산합니다.
    /// - HP(=MaxHp 보너스): 클릭당 +10
    /// - Evasion/Accuracy: 클릭당 +0.05
    /// - 나머지: 클릭당 +1
    /// </summary>
    private int GetCurrentStepCount()
    {
        if (targetCharacter == null) return 0;

        float currentBonus = targetCharacter.GetUpgradeBonus(GetBonusStatForCost());
        if (currentBonus < 0f) currentBonus = 0f;

        float stepSize = 1f;
        if (statType == TargetStat.Hp || statType == TargetStat.MaxHp)
            stepSize = HpStepPerClick;
        else if (statType == TargetStat.Evasion || statType == TargetStat.Accuracy)
            stepSize = RateStatStepPerClick;

        float ratio = currentBonus / Mathf.Max(0.000001f, stepSize);

        // 부동소수점 오차 보정용 epsilon
        return Mathf.FloorToInt(ratio + 0.0001f);
    }

    /// <summary>
    /// 현재 보너스 단계에서 "마지막으로 지불한" 1단계 비용.
    /// 보너스가 n일 때 마지막 증가 비용은 n (0→1 때 1, 1→2 때 2, …).
    /// </summary>
    private int GetRefundForRemovingLastStep()
    {
        if (targetCharacter == null) return 0;
        int steps = GetCurrentStepCount();
        if (steps <= 0) return 0;
        return steps; // 마지막 단계 비용만 환급
    }

    /// <summary>
    /// 이 스탯에 대해 누적 지불한 영혼먼지 합 (1+2+...+n = n(n+1)/2, n = 현재 보너스 단계).
    /// </summary>
    private int GetAccumulatedStatInvestmentCost()
    {
        if (targetCharacter == null) return 0;
        int n = GetCurrentStepCount();
        return n * (n + 1) / 2; // 1+2+...+n
    }

    /// <summary>
    /// 현재 스탯 값을 반환합니다. (업그레이드 보너스를 포함한 최종값)
    /// </summary>
    /// <returns>최종 스탯 값 (기본값 + 보너스)</returns>
    private float GetCurrentStatValue()
    {
        if (targetCharacter == null) return 0;
        return targetCharacter.GetFinalStatValue(statType);
    }

    /// <summary>
    /// UI 표시를 업데이트합니다.
    /// </summary>
    private void UpdateDisplay()
    {
        float currentValue = GetCurrentStatValue();

        // 값 표시 업데이트
        if (value != null)
        {
            // 정수 스탯과 실수 스탯 구분
            if (statType == TargetStat.Evasion || statType == TargetStat.Accuracy)
            {
                value.text = currentValue.ToString("F2");
            }
            else
            {
                value.text = ((int)currentValue).ToString();
            }
        }

        if (usage != null)
        {
            int spentOnThisStat = GetAccumulatedStatInvestmentCost();
            usage.text = spentOnThisStat > 0 ? $"Spent: {spentOnThisStat}" : "";
            usage.gameObject.SetActive(spentOnThisStat > 0);
        }
    }
}

