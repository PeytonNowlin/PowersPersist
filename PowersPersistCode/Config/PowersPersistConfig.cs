using BaseLib.Config;

namespace PowersPersist.PowersPersistCode.Config;

/// <summary>
/// Properties must be static; BaseLib only sees static properties on a ModConfig
/// (see BaseLib's ModConfig.CheckConfigProperties which warns and ignores
/// instance properties).
///
/// Localization keys (optional, omitted in v0.1.0): the property name itself
/// is used as the displayed label when no LocString is registered under
/// "settings_ui" -> "POWERSPERSIST-&lt;slugified_property_name&gt;.title".
/// </summary>
public sealed class PowersPersistConfig : SimpleModConfig
{
    /// <summary>
    /// When true, power cards are removed from the run deck after being played
    /// (in addition to their normal exhaust-on-play behaviour). Matches the
    /// optional setting in the original Slay the Spire 1 "Powers Persist" mod.
    /// </summary>
    public static bool RemovePowerCardsOnPlay { get; set; }

    /// <summary>
    /// When true, debuff-type powers (and buffs whose current amount has gone
    /// negative, like Strength=-1 from Shrink) are NOT carried over to the
    /// next combat. Default off, so behaviour matches the original mod.
    /// </summary>
    public static bool SkipNegativePowers { get; set; }

    /// <summary>
    /// When true, powers gained outside an active combat (e.g. from
    /// non-combat events) are NOT carried over to the next combat. Default
    /// off, so behaviour matches the original mod.
    /// </summary>
    public static bool SkipNonCombatOriginPowers { get; set; }
}
