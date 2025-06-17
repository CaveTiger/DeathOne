using System.Collections.Generic;
using UnityEngine;

public class StageData
{
    public string ID;
    public string ParentID;
    public List<string> BlockIDs = new();
    public List<string> NextBlockIDs = new List<string>();
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
    public List<string> NextBlockIDs = new(); // 다음 블록 ID 리스트
    public bool Last = false; // 스테이지 클리어 가능 여부
    public bool Cleared = false; // 블록 클리어 여부
    public bool Locked = false; // 블록 잠금 여부 아직 갈 수 없는 블록에 적용
}