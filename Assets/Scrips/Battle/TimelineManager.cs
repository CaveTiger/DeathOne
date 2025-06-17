//  using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;

// public class TimelineManager : MonoBehaviour
// {
//     public static TimelineManager Instance { get; private set; }

//     private const string TIMELINE_PREFIX = "Timeline_";
//     private const int MAX_SAVED_BLOCKS = 10;

//     private List<TimelineBlock> currentTimelineBlocks = new();
//     private TimelineBlock currentBlock;

//     private void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else Destroy(gameObject);
//     }

//     // 새로운 타임라인 블록 생성
//     public void CreateNewBlock(bool isPlayerTurn)
//     {
//         currentBlock = new TimelineBlock
//         {
//             BlockID = Guid.NewGuid().ToString(),
//             Timestamp = DateTime.Now,
//             IsMyTurn = isPlayerTurn,
//             Snapshot = CreateBattleSnapshot()
//         };
//         currentTimelineBlocks.Add(currentBlock);
//         SaveTimelineBlock(currentBlock);
//     }

//     // 행동 로그 추가
//     public void AddActionLog(string actionType, string sourceId, string targetId, string actionId, int value)
//     {
//         if (currentBlock == null) return;

//         var action = new TimelineActionLog
//         {
//             ActionType = actionType,
//             SourceID = sourceId,
//             TargetID = targetId,
//             ActionID = actionId,
//             Value = value,
//             Timestamp = DateTime.Now
//         };

//         currentBlock.Actions.Add(action);
//         SaveTimelineBlock(currentBlock);
//     }

//     // 전투 스냅샷 생성
//     private BattleSnapshot CreateBattleSnapshot()
//     {
//         var snapshot = new BattleSnapshot();
        
//         // 캐릭터 상태 저장
//         var characters = FindObjectsOfType<CharacterStats>();
//         foreach (var character in characters)
//         {
//             var charSnapshot = new CharacterSnapshot
//             {
//                 CharacterID = character.name,
//                 CurrentHP = character.Hp,
//                 MaxHP = character.MaxHp,
//                 Position = character.transform.position
//             };

//             // 상태이상 효과 저장
//             var statusEffects = character.GetComponents<StatusEffectInstance>();
//             foreach (var effect in statusEffects)
//             {
//                 charSnapshot.StatusEffects.Add(new StatusEffectSnapshot
//                 {
//                     EffectID = effect.EffectID,
//                     Duration = effect.Duration,
//                     Value = effect.Value
//                 });
//             }

//             snapshot.Characters.Add(charSnapshot);
//         }

//         // 현재 턴 정보 저장
//         var turnManager = TurnManager.Instance;
//         if (turnManager != null)
//         {
//             snapshot.CurrentTurn = turnManager.CurrentTurn;
//             snapshot.CurrentActorID = turnManager.CurrentActor?.name;
//             snapshot.IsPlayerTurn = turnManager.IsPlayerTurn;
//         }

//         return snapshot;
//     }

//     // 타임라인 블록 저장
//     private void SaveTimelineBlock(TimelineBlock block)
//     {
//         string json = JsonUtility.ToJson(block);
//         string key = $"{TIMELINE_PREFIX}{block.BlockID}";
//         PlayerPrefs.SetString(key, json);
//         PlayerPrefs.Save();
//         Debug.Log($"[TimelineManager] 타임라인 블록 저장: {block.BlockID}");
//     }

//     // 타임라인 블록 로드
//     public TimelineBlock LoadTimelineBlock(string blockId)
//     {
//         string key = $"{TIMELINE_PREFIX}{blockId}";
//         string json = PlayerPrefs.GetString(key, "");
//         if (!string.IsNullOrEmpty(json))
//         {
//             return JsonUtility.FromJson<TimelineBlock>(json);
//         }
//         return null;
//     }

//     // 시간 되돌리기 실행
//     public void ReturnToTimeline(string blockId)
//     {
//         var block = LoadTimelineBlock(blockId);
//         if (block != null)
//         {
//             // 1. 현재 상태 저장
//             SaveCurrentState();
            
//             // 2. 타임라인 블록의 상태로 복원
//             RestoreFromSnapshot(block.Snapshot);
            
//             // 3. 행동 로그 재생
//             ReplayActions(block.Actions);
            
//             Debug.Log($"[TimelineManager] 시간 되돌리기 완료: {blockId}");
//         }
//     }

//     // 현재 상태 저장
//     private void SaveCurrentState()
//     {
//         if (currentBlock != null)
//         {
//             currentBlock.Snapshot = CreateBattleSnapshot();
//             SaveTimelineBlock(currentBlock);
//         }
//     }

//     // 스냅샷에서 상태 복원
//     private void RestoreFromSnapshot(BattleSnapshot snapshot)
//     {
//         // 캐릭터 상태 복원
//         var characters = FindObjectsOfType<CharacterStats>();
//         foreach (var charSnapshot in snapshot.Characters)
//         {
//             var character = characters.FirstOrDefault(c => c.name == charSnapshot.CharacterID);
//             if (character != null)
//             {
//                 character.Hp = charSnapshot.CurrentHP;
//                 character.MaxHp = charSnapshot.MaxHP;
//                 character.transform.position = charSnapshot.Position;

//                 // 상태이상 효과 복원
//                 var statusEffects = character.GetComponents<StatusEffectInstance>();
//                 foreach (var effect in statusEffects)
//                 {
//                     Destroy(effect);
//                 }

//                 foreach (var effectSnapshot in charSnapshot.StatusEffects)
//                 {
//                     // TODO: 상태이상 효과 재생성 로직 구현
//                 }
//             }
//         }

//         // 턴 정보 복원
//         var turnManager = TurnManager.Instance;
//         if (turnManager != null)
//         {
//             turnManager.SetTurn(snapshot.CurrentTurn);
//             // TODO: 현재 행동자 설정 로직 구현
//         }
//     }

//     // 행동 로그 재생
//     private void ReplayActions(List<TimelineActionLog> actions)
//     {
//         foreach (var action in actions)
//         {
//             // TODO: 행동 재생 로직 구현
//             Debug.Log($"[TimelineManager] 행동 재생: {action.ActionType} by {action.SourceID}");
//         }
//     }

//     // 오래된 타임라인 블록 정리
//     public void ClearOldTimelineBlocks()
//     {
//         var blocks = GetAllTimelineBlocks();
//         if (blocks.Count > MAX_SAVED_BLOCKS)
//         {
//             blocks.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
//             for (int i = 0; i < blocks.Count - MAX_SAVED_BLOCKS; i++)
//             {
//                 string key = $"{TIMELINE_PREFIX}{blocks[i].BlockID}";
//                 PlayerPrefs.DeleteKey(key);
//             }
//             PlayerPrefs.Save();
//             Debug.Log($"[TimelineManager] 오래된 타임라인 블록 {blocks.Count - MAX_SAVED_BLOCKS}개 정리 완료");
//         }
//     }

//     // 모든 타임라인 블록 가져오기
//     private List<TimelineBlock> GetAllTimelineBlocks()
//     {
//         var blocks = new List<TimelineBlock>();
//         foreach (var key in PlayerPrefs.AllKeys)
//         {
//             if (key.StartsWith(TIMELINE_PREFIX))
//             {
//                 string json = PlayerPrefs.GetString(key);
//                 var block = JsonUtility.FromJson<TimelineBlock>(json);
//                 if (block != null)
//                 {
//                     blocks.Add(block);
//                 }
//             }
//         }
//         return blocks;
//     }
// }