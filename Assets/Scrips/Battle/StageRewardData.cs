using System.Collections.Generic;

[System.Serializable]
public class StageRewardData
{
    public string stageID;
    public List<string> enemyIDs = new List<string>(); // 이 스테이지에서 생성되는 적들
    public List<string> unlockableSkills = new List<string>(); // 해금 가능한 스킬들
    public List<string> unlockableCharacters = new List<string>(); // 해금 가능한 캐릭터들
    public int totalSoulDust; // 총 영혼먼지 보상
    public int totalEssence; // 총 강자의 정수 보상
    public bool isAlreadyCleared = false; // 이미 클리어된 스테이지인지
} 