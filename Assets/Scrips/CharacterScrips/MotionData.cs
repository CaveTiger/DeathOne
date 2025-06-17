using UnityEngine;

[System.Serializable]
public class MotionData
{
    public string motionType;      // 모션 타입 (Attack, Hit, Buff 등)
    public string spritePath;      // 스프라이트 경로
    public float duration;         // 모션 지속 시간
    public bool returnToPrevious;  // 이전 모션으로 복귀 여부
    public Vector2 offset;         // 모션 시 위치 오프셋
    public bool flipX;             // X축 반전 여부
    public bool flipY;             // Y축 반전 여부
}

[CreateAssetMenu(fileName = "MotionData", menuName = "Battle/Motion Data")]
public class MotionDataObject : ScriptableObject
{
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
} 