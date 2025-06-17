using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageProgressData
{
    public string stageId;
    public bool isCleared;
    public int clearCount;
    public int bestScore;
    public DateTime lastClearTime;

    public bool isLocked = true; // 잠금 여부(기본값 true) 보통 갈림길서 쓰임

    // 중간 저장용(이어하기)
    public bool isInProgress;      // 진행 중 여부
    public int currentWave;
    public int playerHp;
    public List<string> partyStatus; // 파티원 상태 등

    // 블록 단위 진행 상황
    public List<BlockProgressData> blockProgress = new(); // 블록별 진행 상황
}

[System.Serializable]
public class BlockProgressData
{
    public string blockId;          // 블록 ID
    public bool isCleared;          // 클리어 여부
    public bool isLocked = true;    // 잠금 여부
    public DateTime lastClearTime;   // 마지막 클리어 시간
}

[System.Serializable]
public class StageProgressWrapper
{
    public List<StageProgressData> progressData = new();
    public string lastPlayedDate;
    public string gameVersion;
    public int totalPlayTime; // 총 플레이 시간 (초)
    public int totalClearCount; // 총 클리어 횟수
}
