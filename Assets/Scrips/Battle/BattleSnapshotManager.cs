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
    public string currentTargetId;
    public List<string> turnOrderIds = new List<string>();
    public List<UnitStateSnapshot> units = new List<UnitStateSnapshot>();
}

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

    public void ClearAllSnapshots()
    {
        ClearTurnSnapshots();
        ClearBattleEndSnapshot();
    }

    public void CaptureTurnSnapshot(
        IEnumerable<CharacterStats> units,
        int turnIndex,
        IEnumerable<CharacterStats> turnOrder = null,
        CharacterStats currentTarget = null)
    {
        TurnSnapshot snapshot = BuildSnapshot(units, turnIndex, turnOrder, currentTarget);
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

    public void CaptureBattleEndSnapshot(IEnumerable<CharacterStats> units)
    {
        battleEndSnapshot = BuildSnapshot(units, -1, null, null);
    }

    public bool TryRestoreBattleEndSnapshot(IList<CharacterStats> liveUnits)
    {
        if (battleEndSnapshot == null) return false;
        ApplySnapshotToUnits(battleEndSnapshot, liveUnits);
        return true;
    }

    private TurnSnapshot BuildSnapshot(
        IEnumerable<CharacterStats> units,
        int turnIndex,
        IEnumerable<CharacterStats> turnOrder,
        CharacterStats currentTarget)
    {
        TurnSnapshot snapshot = new TurnSnapshot
        {
            turnIndex = turnIndex,
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
                    kdp = unit.data != null ? unit.data.KDP : 0,
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

            if (live.data != null)
            {
                live.data.KDP = Mathf.Max(0, state.kdp);
            }

            if (live.HpUI != null)
            {
                live.HpUI.UpdateHpBar(live.Hp, live.MaxHp);
            }
        }
    }
}
