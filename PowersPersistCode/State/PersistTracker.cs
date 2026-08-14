using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;

namespace PowersPersist.PowersPersistCode.State;

public enum PowerOrigin
{
    Battle,
    Event,
}

public readonly record struct PersistedPower(ModelId Id, int Amount);

/// <summary>
/// In-memory only by design: snapshots and origin tags die with the
/// process, which gives us the original mod's "doesn't survive save-and-quit"
/// behaviour for free.
/// </summary>
public static class PersistTracker
{
    private static readonly Dictionary<ulong, List<PersistedPower>> Snapshots = new();
    private static readonly Dictionary<(ulong NetId, ModelId PowerId), PowerOrigin> Origins = new();

    /// <summary>
    /// Set true while we are re-applying persisted powers at start of combat,
    /// so PowerCmd.Apply Postfix knows to skip origin tagging (otherwise the
    /// reapply would re-tag everything as Event, since IsInProgress is still
    /// false at that point).
    /// </summary>
    [System.ThreadStatic]
    private static bool _isReapplying;

    public static bool IsReapplying
    {
        get => _isReapplying;
        set => _isReapplying = value;
    }

    public static void SetSnapshot(ulong netId, List<PersistedPower> powers)
    {
        Snapshots[netId] = powers;
    }

    public static List<PersistedPower>? TakeSnapshot(ulong netId)
    {
        if (!Snapshots.TryGetValue(netId, out List<PersistedPower>? snap))
        {
            return null;
        }

        Snapshots.Remove(netId);
        return snap;
    }

    public static void TagOrigin(ulong netId, ModelId powerId, PowerOrigin origin)
    {
        Origins[(netId, powerId)] = origin;
    }

    public static bool IsEventOrigin(ulong netId, ModelId powerId)
    {
        return Origins.TryGetValue((netId, powerId), out PowerOrigin origin)
            && origin == PowerOrigin.Event;
    }

    public static void ClearOriginsFor(ulong netId)
    {
        List<(ulong NetId, ModelId PowerId)> toRemove = new();
        foreach (KeyValuePair<(ulong NetId, ModelId PowerId), PowerOrigin> kvp in Origins)
        {
            if (kvp.Key.NetId == netId)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach ((ulong NetId, ModelId PowerId) key in toRemove)
        {
            Origins.Remove(key);
        }
    }

    /// <summary>
    /// Wipe every snapshot and origin tag. Used when a new run starts so
    /// powers from a just-finished run cannot leak into the next one
    /// without quitting the process.
    /// </summary>
    public static void ClearAll()
    {
        Snapshots.Clear();
        Origins.Clear();
        IsReapplying = false;
    }
}
