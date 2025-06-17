// using System;
// using System.Collections.Generic;
// using UnityEngine;

// [System.Serializable]
// public class TimelineActionLog
// {
//     public string ActionType; // "Skill", "Move", "Item" 등
//     public string SourceID;   // 행동을 수행한 캐릭터 ID
//     public string TargetID;   // 행동의 대상 ID
//     public string ActionID;   // 스킬ID, 아이템ID 등
//     public int Value;         // 데미지, 회복량 등
//     public DateTime Timestamp;
// }

// [System.Serializable]
// public class BattleSnapshot
// {
//     public List<CharacterSnapshot> Characters = new();
//     public int CurrentTurn;
//     public string CurrentActorID;
//     public bool IsPlayerTurn;
// }

// [System.Serializable]
// public class CharacterSnapshot
// {
//     public string CharacterID;
//     public int CurrentHP;
//     public int MaxHP;
//     public Vector2 Position;
//     public List<StatusEffectSnapshot> StatusEffects = new();
// }

// [System.Serializable]
// public class StatusEffectSnapshot
// {
//     public string EffectID;
//     public int Duration;
//     public int Value;
// }

// [System.Serializable]
// public class TimelineBlock
// {
//     public string BlockID;
//     public DateTime Timestamp;
//     public List<TimelineActionLog> Actions = new();
//     public BattleSnapshot Snapshot;
//     public bool IsMyBlock;
//     public bool IsMyTurn;
//     public bool ReturnOnline;
//     public int ReturnCount;
// } 