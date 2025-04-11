using System.Collections.Generic;

public class StageData
{
    public string ID;
    public string ParentID;
    public string Label;
    public int TotalStageBlock;
    public List<StageBlockData> Blocks = new();
}
public class StageBlockData
{
    public string StageKey;
    public string BlockType;
    public string ParentBlockName;
    public List<string> EnemyIDs = new();
    public List<string> EventList = new();
    public List<string> SpecialGuests = new();
    public string FrontCutID;
    public string BackCutID;
}