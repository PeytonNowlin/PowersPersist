# Game hooks reference (sts2.dll)

Captured from `ilspycmd` decompile of
`<STS2>/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_x86_64/sts2.dll`.
Confirmed against game build present on disk during initial development.
If hook signatures change in a game update, fix here first.

## Snapshot point: end of combat clearing

`MegaCrit.Sts2.Core.Entities.Players.Player.AfterCombatEnd()` (sync, returns void)

```csharp
public void AfterCombatEnd()
{
    Creature.RemoveAllPowersInternalExcept();
    PlayerCombatState?.AfterCombatEnd();
    Creature.LoseBlockInternal(Creature.Block);
}
```

Player powers live on `Player.Creature` (a `Creature`), exposed as
`Creature.Powers => IReadOnlyList<PowerModel>`. They survive being on the
`Creature` object itself across combats — the only thing that wipes them is
`Creature.RemoveAllPowersInternalExcept(except)` called from this method.

We Harmony-`Prefix` `Player.AfterCombatEnd` and capture the current powers
(filtered by config) into `PersistTracker` before letting the original clear
them. The original then runs as normal.

## Reapply point: start of next combat

`MegaCrit.Sts2.Core.Combat.CombatManager.SetUpCombat(CombatState state)` (sync)

By the time this runs, every `Player.Creature` already has its `CombatState`
attached (via `CombatRoom.EnterInternal -> CombatState.AddPlayer ->
CombatState.AttachCreature`, which sets `creature.CombatState = this`). So
`Creature.CanReceivePowers` is true and `PowerModel.ApplyInternal` will work.

We Harmony-`Postfix` this method, and for each player look up its snapshot
in `PersistTracker`. Re-application uses
`PowerModel.ApplyInternal(creature, amount, silent: true)` directly (after
constructing a fresh mutable via `ModelDb.GetByIdOrNull<PowerModel>(id).ToMutable(0)`
and setting `Owner` via the public setter chain). This bypasses
`PowerCmd.Apply` so we do NOT re-trigger `Hook.BeforePowerAmountChanged`,
`AfterPowerAmountChanged`, on-apply relic effects, or
`CombatManager.History.PowerReceived` — re-application from persistence is
not "gaining" the power, just restoring it.

`silent: true` skips amount-change UI flashes; the `PowerApplied` event still
fires so the icon shows up.

## Origin tagging

`MegaCrit.Sts2.Core.Commands.PowerCmd.Apply(PowerModel power, Creature target,
decimal amount, Creature? applier, CardModel? cardSource, bool silent = false)`
(static, async)

This is the canonical entry point for applying a power. The generic
`Apply<T>` overload also routes through here when the target doesn't already
have the power. We Harmony-`Postfix` it: when `target.IsPlayer`, we tag
`(player.NetId, power.Id)` in `PersistTracker.Origins` based on whether
`CombatManager.Instance.IsInProgress` is true (Battle origin) or false
(Event origin).

The reapply path bypasses `PowerCmd.Apply` entirely (see above), so it
won't accidentally re-tag everything as Event during the start-of-combat
reapply window where `IsInProgress` is still false.

## Power-card removal

`MegaCrit.Sts2.Core.Models.CardModel.OnPlayWrapper(PlayerChoiceContext, Creature?, bool, ResourceInfo, bool)`
(async Task)

Returns once the card play is fully resolved (including the
`CardPileCmd.RemoveFromCombat(this, ...)` branch that power cards take when
`GetResultPileType()` returns `PileType.None`).

We Harmony-`Postfix` it with a `ref Task __result` parameter and replace
`__result` with a wrapper task that awaits the original then, when
`Type == CardType.Power && DeckVersion != null && DeckVersion.Pile?.Type
== PileType.Deck`, calls
`CardPileCmd.RemoveFromDeck(DeckVersion, showPreview: false)` to purge the
card from the run deck. Guarded by `PowersPersistConfig.RemovePowerCardsOnPlay`.

## Useful types

- `MegaCrit.Sts2.Core.Entities.Powers.PowerType` — `Buff | Debuff | None`.
- `PowerModel.TypeForCurrentAmount` — flips `Strength` to `Debuff` when
  `Amount < 0`, so a single check catches both inherent debuffs (Weak,
  Vulnerable, Hex, …) and "negative buff" debuffs (NegativeStrength etc).
- `MegaCrit.Sts2.Core.Models.ModelDb.GetByIdOrNull<PowerModel>(ModelId id)` —
  resolves canonical power instance for re-application.
- `CombatManager.Instance.IsInProgress` — true between
  `StartCombatInternal` (after IsInProgress=true) and `EndCombatInternal`.
  Used to distinguish Battle vs Event origin.
