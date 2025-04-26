using System.Collections.Generic;
using UnityEngine;
public class PartySlot
{
    public int SlotIndex;         // 예: 0~3
    public Vector2 Position;      // 씬 내 실제 위치
    public string CharacterID;    // 현재 슬롯에 누구 배정됐는지
}

public class InStageCharacterData
{
    public string CharacterID;
    public int CurrentHP;
    // 나중에 확장 가능: 상태이상, 스킬 쿨타임 등
}

public class InStageData
{
    public List<PartySlot> Slots = new();
    public Dictionary<string, InStageCharacterData> Characters = new(); // CharacterID 기준
}

