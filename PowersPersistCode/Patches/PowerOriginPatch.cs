using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
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
///
/// STS2 0.105+ inserted <see cref="PlayerChoiceContext"/> as the first
/// argument of <c>PowerCmd.Apply</c>. Resolve the target at patch time the
/// same way BaseLib's SelfApplyDebuffPatch does, so we load on both the
/// current main/beta signatures and the pre-0.105 overload.
/// </summary>
internal static class PowerOriginPatch
{
    [HarmonyPatch]
    internal static class TagOriginOnApply
    {
        private static MethodBase TargetMethod()
        {
            MethodInfo? withContext = AccessTools.DeclaredMethod(
                typeof(PowerCmd),
                nameof(PowerCmd.Apply),
                new[]
                {
                    typeof(PlayerChoiceContext),
                    typeof(PowerModel),
                    typeof(Creature),
                    typeof(decimal),
                    typeof(Creature),
                    typeof(CardModel),
                    typeof(bool),
                });
            if (withContext != null)
            {
                return withContext;
            }

            MethodInfo? legacy = AccessTools.DeclaredMethod(
                typeof(PowerCmd),
                nameof(PowerCmd.Apply),
                new[]
                {
                    typeof(PowerModel),
                    typeof(Creature),
                    typeof(decimal),
                    typeof(Creature),
                    typeof(CardModel),
                    typeof(bool),
                });
            if (legacy != null)
            {
                return legacy;
            }

            throw new MissingMethodException(
                typeof(PowerCmd).FullName,
                "Apply(PlayerChoiceContext?, PowerModel, Creature, decimal, Creature, CardModel, bool)");
        }

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
