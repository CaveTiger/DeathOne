using System.Collections.Generic;
using UnityEngine;

public class StageData
{
    public string ID;
    public string ParentID;
    public List<string> BlockIDs = new();
}

public class StageBlockData
{
    public string ID;
    public string ParentID;
    public string BlockType;
    public string FrontCutID;
    public string BackCutID;
    public Vector2Int Position;
    public List<string> EnemyIDs = new();
    public List<string> NextBlockIDs = new(); // li∑Œ π≠¿Œ ∫Œ∫–
}