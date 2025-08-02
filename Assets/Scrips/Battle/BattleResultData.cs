using System.Collections.Generic;

[System.Serializable]
public class BattleResultData
{
    public bool isVictory;
    public bool isCleared; // 스테이지 클리어 여부 (이미 클리어된 경우 false)
    public List<string> deadEnemyIDs = new List<string>();
    public List<string> deadAllyIDs = new List<string>();
    public List<string> unlockedSkillIDs = new List<string>();
    public List<string> unlockedCharacterIDs = new List<string>();
    public int soulDustGained;
    public int essenceGained;
    public string stageID;
} 