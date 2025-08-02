using UnityEngine;

/// <summary>
/// 캐릭터 모션 데이터 구조 (간단 버전)
/// 
/// 사용법:
/// 1. Resources/MotionData 폴더에 {캐릭터ID}_Motions.asset 파일 생성
/// 2. 각 모션 타입별로 MotionData 설정
/// 3. 스킬 데이터의 Motion 필드에 모션 타입 입력
/// </summary>
[System.Serializable]
public class MotionData
{
    [Header("기본 설정")]
    public string motionType;      // 모션 타입 (Attack, Hit, Buff 등)
    public string spritePath;      // 스프라이트 경로 (Resources 폴더 기준)
    public float duration;         // 모션 지속 시간 (0이면 무한)
    public bool returnToPrevious;  // 이전 모션으로 복귀 여부
    
    [Header("위치")]
    public Vector2 offset;         // 모션 시 위치 오프셋
}

/// <summary>
/// 캐릭터별 모션 데이터 오브젝트
/// 
/// 생성 방법:
/// 1. Project 창에서 우클릭
/// 2. Create > Battle > Motion Data
/// 3. 파일명을 "{캐릭터ID}_Motions"로 설정
/// 4. Resources/MotionData 폴더에 저장
/// </summary>
[CreateAssetMenu(fileName = "MotionData", menuName = "Battle/Motion Data")]
public class MotionDataObject : ScriptableObject
{
    [Header("캐릭터 정보")]
    public string characterID;     // 캐릭터 ID
    public string description;     // 모션 데이터 설명
    
    [Header("모션 데이터")]
    public MotionData[] motions;

    public MotionData GetMotionData(string motionType)
    {
        foreach (var motion in motions)
        {
            if (motion.motionType == motionType)
                return motion;
        }
        return null;
    }
    
    /// <summary>
    /// 모든 모션 타입을 반환합니다.
    /// </summary>
    public string[] GetAllMotionTypes()
    {
        string[] types = new string[motions.Length];
        for (int i = 0; i < motions.Length; i++)
        {
            types[i] = motions[i].motionType;
        }
        return types;
    }
    
    /// <summary>
    /// 특정 모션이 존재하는지 확인합니다.
    /// </summary>
    public bool HasMotion(string motionType)
    {
        return GetMotionData(motionType) != null;
    }
} 