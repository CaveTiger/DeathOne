using UnityEngine;

[CreateAssetMenu(fileName = "Blessing", menuName = "Scriptable Objects/Blessing")]
public class BlessingData : ScriptableObject
{
    [Header("기본 정보 (필수)")]
    [Tooltip("축복 ID (예: 090001)")]
    public string blessingID;
    
    [Tooltip("축복 이름 (표시용)")]
    public string blessingName;
    
    [TextArea]
    [Tooltip("축복 설명")]
    public string description;

    [Header("UI 설정")]
    [Tooltip("이 축복이 속한 라인 인덱스 (0~4)")]
    public int lineIndex = 0;

    [Header("적용 범위 설정")]
    [Tooltip("true면 파티 전체 아군에게 적용, false면 1번 슬롯 주인공에게만 적용")]
    public bool applyToAllAllies = false;

    [Header("단순 스탯 효과 (scriptClass가 비어있을 때 사용)")]
    [Tooltip("적용 대상 스탯")]
    public TargetStat targetStat = TargetStat.None;
    
    [Tooltip("효과 수치 (예: 속도 10 증가 시 value = 10, 회피율 0.1 증가 시 value = 0.1)")]
    public float value = 0f;
    
    [Header("특수 효과 (scriptClass가 있으면 이쪽 사용)")]
    [Tooltip("특수 효과 클래스명 (예: BlessingEffectCriticalChance). 비어있으면 단순 스탯 효과 사용")]
    public string scriptClass = "";
    
    /// <summary>
    /// ScriptableObject 활성화 시 호출 - blessingID가 비어있으면 경고
    /// </summary>
    private void OnEnable()
    {
        // blessingID가 비어있으면 경고 출력 (Inspector에서 입력 필요)
        if (string.IsNullOrEmpty(blessingID))
        {
            Debug.LogWarning($"[BlessingData] '{name}' ScriptableObject의 blessingID가 비어있습니다. " +
                           $"Inspector에서 blessingID를 입력해주세요 (예: 090001). " +
                           $"현재 blessingName: '{blessingName}'");
        }
    }
}

