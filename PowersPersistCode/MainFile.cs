using BaseLib.Config;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using PowersPersist.PowersPersistCode.Config;

namespace PowersPersist.PowersPersistCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "PowersPersist";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        ModConfigRegistry.Register(ModId, new PowersPersistConfig());

        Harmony harmony = new(ModId);
        harmony.PatchAll();

        Logger.Info($"{ModId} loaded.");
    }
}
