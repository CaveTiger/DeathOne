using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class UnitStateSnapshot
{
    public string characterId;
    public int hp;
    public bool isDead;
    public int kdp;
    public float collapseChance;
}

[System.Serializable]
public class TurnSnapshot
{
    public int turnIndex;
    public string currentActorId;
    public string currentTargetId;
    public List<string> turnOrderIds = new List<string>();
    public List<UnitStateSnapshot> units = new List<UnitStateSnapshot>();
}

/// <summary>
/// 전투 스냅샷: (1) 턴 되돌리기용 히스토리 (2) 전투 종료 직후 상태.
/// <para><b>전투 종료 스냅샷(battleEnd)</b>은 월드맵으로 돌아가지 않고 <b>연속 전투</b>를 이어갈 때,
/// 직전 전투 종료 시점의 HP·KDP 등을 다음 <see cref="BattleManager.StartBattle"/> 한 번에만 복원하기 위함.</para>
/// <para>월드맵 복귀 시에는 <see cref="ClearAllSnapshots"/>로 턴 히스토리와 battleEnd를 함께 버려 세션을 끊는다.</para>
/// </summary>
public class BattleSnapshotManager : MonoBehaviour
{
    public static BattleSnapshotManager Instance { get; private set; }

    [Header("턴 되돌리기 설정")]
    [SerializeField] private int maxTurnSnapshots = 8;

    private readonly List<TurnSnapshot> turnSnapshots = new List<TurnSnapshot>();
    private TurnSnapshot battleEndSnapshot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public int GetTurnSnapshotCount()
    {
        return turnSnapshots.Count;
    }

    public void ClearTurnSnapshots()
    {
        turnSnapshots.Clear();
    }

    public void ClearBattleEndSnapshot()
    {
        battleEndSnapshot = null;
    }

    /// <summary>턴 되돌리기 히스토리 + 전투 종료 스냅샷까지 전부 제거. 월드맵 복귀 시 호출 전제.</summary>
    public void ClearAllSnapshots()
    {
        ClearTurnSnapshots();
        ClearBattleEndSnapshot();
    }

    public void CaptureTurnSnapshot(
        IEnumerable<CharacterStats> units,
        int turnIndex,
        CharacterStats currentActor = null,
        IEnumerable<CharacterStats> turnOrder = null,
        CharacterStats currentTarget = null)
    {
        TurnSnapshot snapshot = BuildSnapshot(units, turnIndex, currentActor, turnOrder, currentTarget);
        turnSnapshots.Add(snapshot);

        if (maxTurnSnapshots < 1) maxTurnSnapshots = 1;
        if (turnSnapshots.Count > maxTurnSnapshots)
        {
            turnSnapshots.RemoveAt(0);
        }
    }

    public bool TryGetTurnSnapshot(int turnsBack, out TurnSnapshot snapshot)
    {
        snapshot = null;
        if (turnsBack < 1) return false;
        if (turnSnapshots.Count == 0) return false;
        if (turnsBack > turnSnapshots.Count) return false;

        int index = turnSnapshots.Count - turnsBack;
        snapshot = turnSnapshots[index];
        return snapshot != null;
    }

    public bool TryRestoreTurnSnapshot(int turnsBack, IList<CharacterStats> liveUnits)
    {
        if (!TryGetTurnSnapshot(turnsBack, out TurnSnapshot snapshot)) return false;
        ApplySnapshotToUnits(snapshot, liveUnits);
        return true;
    }

    /// <summary>직전 전투가 끝난 직후 유닛 상태를 저장. 다음 전투가 같은 스테이지 체인(맵 미복귀)일 때만 소비됨.</summary>
    public void CaptureBattleEndSnapshot(IEnumerable<CharacterStats> units)
    {
        battleEndSnapshot = BuildSnapshot(units, -1, null, null, null);
    }

    /// <summary>스폰 직후 liveUnits에 battleEnd 스냅샷을 한 번 적용. 성공 시 호출부에서 <see cref="ClearBattleEndSnapshot"/>로 비움.</summary>
    public bool TryRestoreBattleEndSnapshot(IList<CharacterStats> liveUnits)
    {
        if (battleEndSnapshot == null) return false;
        ApplySnapshotToUnits(battleEndSnapshot, liveUnits);
        return true;
    }

    private TurnSnapshot BuildSnapshot(
        IEnumerable<CharacterStats> units,
        int turnIndex,
        CharacterStats currentActor,
        IEnumerable<CharacterStats> turnOrder,
        CharacterStats currentTarget)
    {
        TurnSnapshot snapshot = new TurnSnapshot
        {
            turnIndex = turnIndex,
            currentActorId = currentActor != null ? currentActor.CharacterId : string.Empty,
            currentTargetId = currentTarget != null ? currentTarget.CharacterId : string.Empty
        };

        if (turnOrder != null)
        {
            snapshot.turnOrderIds = turnOrder
                .Where(u => u != null && !string.IsNullOrEmpty(u.CharacterId))
                .Select(u => u.CharacterId)
                .ToList();
        }

        if (units != null)
        {
            foreach (CharacterStats unit in units)
            {
                if (unit == null || string.IsNullOrEmpty(unit.CharacterId)) continue;

                snapshot.units.Add(new UnitStateSnapshot
                {
                    characterId = unit.CharacterId,
                    hp = unit.Hp,
                    isDead = unit.IsDead,
                    kdp = !unit.IsPlayer ? unit.KnockdownBuildup : 0,
                    collapseChance = unit.CollapseChance
                });
            }
        }

        return snapshot;
    }

    private void ApplySnapshotToUnits(TurnSnapshot snapshot, IList<CharacterStats> liveUnits)
    {
        if (snapshot == null || liveUnits == null) return;

        Dictionary<string, UnitStateSnapshot> map = snapshot.units
            .Where(s => s != null && !string.IsNullOrEmpty(s.characterId))
            .ToDictionary(s => s.characterId, s => s);

        foreach (CharacterStats live in liveUnits)
        {
            if (live == null || string.IsNullOrEmpty(live.CharacterId)) continue;
            if (!map.TryGetValue(live.CharacterId, out UnitStateSnapshot state)) continue;

            live.Hp = Mathf.Max(0, state.hp);
            live.IsDead = state.isDead;
            live.CollapseChance = Mathf.Max(0f, state.collapseChance);

            if (!live.IsPlayer)
                live.KnockdownBuildup = Mathf.Max(0, state.kdp);
            else
                live.KnockdownBuildup = 0;

            if (live.HpUI != null)
            {
                live.HpUI.UpdateHpBar(live.Hp, live.MaxHp);
            }
        }
    }
}
