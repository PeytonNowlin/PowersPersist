# Powers Persist

**Buffs and debuffs on the player carry over from one combat to the next.**

A faithful port of the popular Slay the Spire 1 mod
"[Powers Persist](https://steamcommunity.com/sharedfiles/filedetails/?id=3630689909)"
to Slay the Spire 2.

---

## Features

- **Buffs persist across combats.** Strength, Dexterity, Energized, Inflame,
  Demon Form stacks, Metallicize block — anything you build up sticks around
  for the next fight.
- **Debuffs persist too.** That Vulnerable an enemy slapped on you carries
  over (use the optional toggle below if you'd rather skip those).
- **Resets on save and quit.** When you close the game, the persisted powers
  are wiped clean — same as the original. So you can always "rest" by
  closing the game.
- **Three optional toggles** to fine-tune the experience (all default OFF
  so out-of-the-box behavior matches the original mod):
  | Setting | What it does |
  | --- | --- |
  | **Remove power cards from deck on play** | Power cards are permanently removed from your deck after you play them, so you don't draw them again. Stops "drew Inflame turn 1, drew it again later" frustration. |
  | **Skip persisting debuff powers** | Debuffs (Weak, Vulnerable, Frail, Hex, Confused, Poison, …) and reverse-stat powers (e.g. Strength = -1) are dropped at end of combat instead of carried over. |
  | **Skip persisting powers from non-combat events** | Powers granted by events that happen outside an active combat are dropped at end of combat instead of carried over. |

## Requirements

This mod requires **[BaseLib](https://www.nexusmods.com/slaythespire2/mods/103)**
(by Alchyr) to be installed and enabled. Without it, Powers Persist will not
load.

## Installation

### Vortex / Mod Manager

1. Install **BaseLib** if you don't already have it.
2. Click "Mod Manager Download" on this page (when available), or download
   the manual file and let your mod manager handle it.
3. Launch Slay the Spire 2.
4. In the main menu: `Settings` → scroll to the bottom and make sure both
   `BaseLib` and `Powers Persist` are toggled **on**.
5. Restart the game so the mods load.

### Manual install

1. Install BaseLib first.
2. Download the `PowersPersist-vX.Y.Z.zip` from the Files tab.
3. Extract it so you end up with this folder structure:

   **Windows:** `C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\mods\PowersPersist\`

   **macOS:** `~/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/MacOS/mods/PowersPersist/`

   **Linux:** `~/.local/share/Steam/steamapps/common/Slay the Spire 2/mods/PowersPersist/`

   The folder should contain:

   ```
   mods/
   └── PowersPersist/
       ├── PowersPersist.dll
       └── PowersPersist.json
   ```

4. Launch the game, enable both `BaseLib` and `Powers Persist` in
   `Settings`, then restart.

### Configuring the optional toggles

In the main menu: `Settings` → `Mod Configuration` → `Powers Persist`.
The mod is fully usable without touching anything here.

## Compatibility

- **Game version:** built against Slay the Spire 2 v0.103.x; should work on
  any version where `BaseLib` itself loads. If a game patch breaks it I'll
  push an update.
- **Multiplayer:** marked as gameplay-affecting, so every player in a co-op
  lobby needs both this mod and BaseLib at the same versions, otherwise the
  game won't let you connect. **Single-player is the recommended way to
  play this mod** — see "Known issues" below.
- **Other mods:** no known conflicts. This mod only patches four methods
  related to player powers and power-card cleanup, so it should play nicely
  with most other mods. Please report any conflicts on the Bugs tab.

## Known issues

- **Multiplayer power duplication.** The original Slay the Spire 1 mod had
  a known bug where powers could exponentially duplicate in multiplayer.
  That bug is **not** fixed in this port — it's the same architectural
  issue. Stick to single-player to avoid it.
- **Save and quit clears your powers.** This is **by design** to match the
  original mod's behavior. The persisted powers live in memory only and die
  with the game process. If you want them to survive saves, that's a
  different mod.
- **Power-card removal happens at the end of the play, not instantly.**
  When `Remove power cards from deck on play` is on, the deck-purge
  triggers right after the card finishes resolving. You may briefly see the
  card animate to the discard/exhaust pile before it's removed for good.

## Reporting bugs

Please open a report on the **Bugs** tab of this Nexus page with:

- A short description of what happened vs. what you expected.
- Your game version (visible on the main menu) and BaseLib version.
- Whether any other mods were enabled.
- A copy of the most recent log file from
  `<game_user_data>/logs/godot*.log` if the game crashed or threw an
  exception. On macOS that's
  `~/Library/Application Support/SlayTheSpire2/logs/`; on Windows it's
  under `%APPDATA%/SlayTheSpire2/logs/`.

## Credits

- **Original mod author:** the creator of the Slay the Spire 1
  [Powers Persist](https://steamcommunity.com/sharedfiles/filedetails/?id=3630689909)
  Workshop mod — all design credit for the concept goes to them.
- **STS2 port:** Peyton Nowlin.
- Built on **[BaseLib-StS2](https://www.nexusmods.com/slaythespire2/mods/103)**
  by Alchyr, **[ModTemplate-StS2](https://github.com/Alchyr/ModTemplate-StS2)**,
  and **HarmonyX**.

## Source code

Source is MIT-licensed. See `LICENSE` in the repo. Pull requests welcome.

### Building from source (developers only)

```bash
dotnet new install Alchyr.Sts2.Templates    # one-time
dotnet build                                 # auto-deploys to <STS2>/mods/PowersPersist/
```

The csproj auto-discovers the STS2 install on macOS, Linux, and Windows.
Override `Sts2Path` in `Directory.Build.props` if needed. See
`PowersPersistCode/Patches/HOOKS.md` for the exact game class/method names
the mod patches — that's the first place to look if a game update breaks
something.
