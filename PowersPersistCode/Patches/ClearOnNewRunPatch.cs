using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using PowersPersist.PowersPersistCode.State;

namespace PowersPersist.PowersPersistCode.Patches;

/// <summary>
/// Persistence is in-memory only, but a new run can start without the
/// process exiting (victory → next run, abandon → new run). Clear the
/// tracker so powers from the previous run cannot leak across.
/// Reported on Nexus: buffs survived into a brand-new run after a win.
/// </summary>
internal static class ClearOnNewRunPatch
{
    [HarmonyPatch]
    internal static class ClearOnInitializeNewRun
    {
        private static bool Prepare()
        {
            return ResolveTarget() != null;
        }

        private static MethodBase? ResolveTarget()
        {
            return AccessTools.DeclaredMethod(typeof(RunManager), "InitializeNewRun")
                ?? AccessTools.DeclaredMethod(typeof(RunManager), nameof(RunManager.SetUpNewSinglePlayer));
        }

        private static MethodBase TargetMethod()
        {
            return ResolveTarget()
                ?? throw new MissingMethodException(typeof(RunManager).FullName, "InitializeNewRun");
        }

        public static void Prefix()
        {
            try
            {
                PersistTracker.ClearAll();
            }
            catch (Exception ex)
            {
                MainFile.Logger.Error($"PowersPersist: failed to clear persist state for new run: {ex}");
            }
        }
    }

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.SetUpNewMultiPlayer))]
    internal static class ClearOnNewMultiPlayer
    {
        private static bool Prepare()
        {
            // Only needed when InitializeNewRun is missing so the single-player
            // fallback above cannot cover co-op new runs.
            return AccessTools.DeclaredMethod(typeof(RunManager), "InitializeNewRun") == null
                && AccessTools.DeclaredMethod(typeof(RunManager), nameof(RunManager.SetUpNewMultiPlayer)) != null;
        }

        public static void Prefix()
        {
            PersistTracker.ClearAll();
        }
    }
}
