using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using PowersPersist.PowersPersistCode.State;

namespace PowersPersist.PowersPersistCode.Patches;

/// <summary>
/// Tag every power applied to a player with whether the application happened
/// during an active combat (Battle) or outside one (Event), so the
/// SkipNonCombatOriginPowers filter has something to filter on.
///
/// Skips tagging while PersistTracker.IsReapplying is true, so the
/// start-of-combat reapply loop doesn't accidentally tag everything as Event
/// (it bypasses PowerCmd.Apply anyway, but this is belt-and-braces).
/// </summary>
internal static class PowerOriginPatch
{
    [HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Apply), new[]
    {
        typeof(PowerModel),
        typeof(Creature),
        typeof(decimal),
        typeof(Creature),
        typeof(CardModel),
        typeof(bool),
    })]
    internal static class TagOriginOnApply
    {
        public static void Postfix(PowerModel power, Creature target)
        {
            try
            {
                if (PersistTracker.IsReapplying)
                {
                    return;
                }

                if (!target.IsPlayer || target.Player == null)
                {
                    return;
                }

                PowerOrigin origin = CombatManager.Instance.IsInProgress
                    ? PowerOrigin.Battle
                    : PowerOrigin.Event;

                PersistTracker.TagOrigin(target.Player.NetId, power.Id, origin);
            }
            catch (Exception ex)
            {
                MainFile.Logger.Error($"PowersPersist: failed to tag power origin: {ex}");
            }
        }
    }
}
