# Unfair Flips Coin Mod

A [BepInEx 5](https://github.com/BepInEx/BepInEx) mod for **Unfair Flips** that lets you set the **heads chance** and the **flip speed**.

- Heads chance: 0–100 % (game default 20 %)
- Flip duration: 0.1–10 seconds (game default 5 s)
- In-game menu (**F1**) and a config file
- Optional: prevent the ending that wipes your save
- Play any of the 5 endings instantly (e.g. for achievements)
- No game files are modified. Your save is not changed either: the game only stores shop levels, not the chances.

## Download

Get the latest version from the [Releases page](https://github.com/Luka3006/unfair-flips-modmenu/releases/latest).

## Installation

### Option A: All-in-one (recommended)

1. In Steam, right-click **Unfair Flips** → **Manage** → **Browse local files**.
2. Extract `UnfairFlipsCoinMod-<version>-with-BepInEx.zip` into that folder.
   `winhttp.dll` and the `BepInEx` folder must end up next to `Unfair Flips.exe`.
3. Start the game. The mod menu appears in the top-left corner. Press **F1** to show or hide it.

### Option B: BepInEx already installed

Extract `UnfairFlipsCoinMod-<version>.zip` into the game folder.
The DLL ends up in `BepInEx/plugins/UnfairFlipsCoinMod/`.

### Steam Deck / Linux (Proton)

Set the Steam launch options to:

```
WINEDLLOVERRIDES="winhttp=n,b" %command%
```

## Settings

| Setting | Range | Game default |
|---|---|---|
| Custom heads chance | 0–100 % | 20 % (+5 % per shop upgrade) |
| Custom flip duration | 0.1–10 s | 5 s (−0.2 s per shop upgrade) |
| Prevent ending | on/off | off |

- Each override can be turned on or off. When you turn one off, the game's normal value comes back, including shop upgrades you bought in the meantime.
- A new flip duration takes effect after the current flip.
- The config file is created on first launch at `BepInEx/config/luka3006.unfairflips.coinmod.cfg`. There you can also change the menu key and whether the menu shows on start.

### Play ending

Open **[+] Play ending** in the menu and pick one of the endings:

| Button | Achievement ID |
|---|---|
| Roll away | `ENDING_ROLL` |
| Eggbug | `ENDING_EGGBUG` |
| Fly up | `ENDING_UP` |
| Explosion | `ENDING_BOOM` |
| 10 heads celebration | `ENDING_10FLIP` |

The mod plays a normal 10th heads flip with the chosen outcome, so the game handles the cutscene and the achievement exactly as usual. Achievements only unlock when the game runs through Steam.
Like every ending in the game, this **wipes your save**, so the mod asks for confirmation first. The buttons are disabled while the coin is flipping.

> ⚠️ **10 heads in a row ends the game and wipes your save.** With a high heads chance this happens fast.
> Turn on **Prevent ending** to cap the streak at 9.

## Troubleshooting

- **No menu:** Check that `winhttp.dll` is next to `Unfair Flips.exe`. After launching, `BepInEx/LogOutput.log` should exist. If it does not, BepInEx did not load.
- **Menu hidden:** Press F1, or set `Show on start = true` in the config file.
- **Shop message shows a different chance:** When you buy an upgrade while an override is on, the game's message briefly shows its own value. The mod's value still applies.

## Uninstall

Delete `BepInEx/plugins/UnfairFlipsCoinMod/`. To remove BepInEx completely, also delete `winhttp.dll`, `doorstop_config.ini`, `.doorstop_version`, `changelog.txt`, `BepInEx_LICENSE.txt` and the `BepInEx` folder.

## Building from source

Requirements: .NET SDK 6 or newer, and an installed copy of the game (the build references its DLLs).

```bash
dotnet build src -c Release -p:GameDir="C:\Program Files (x86)\Steam\steamapps\common\Unfair Flips"
```

Instead of `-p:GameDir` you can set the `UNFAIR_FLIPS_DIR` environment variable.

The build downloads the pinned BepInEx release once and checks its SHA-256. It then writes both packages to `release/`:

- `UnfairFlipsCoinMod-<version>.zip`: plugin only
- `UnfairFlipsCoinMod-<version>-with-BepInEx.zip`: all-in-one

To make a new release, bump `<Version>` in `src/UnfairFlipsCoinMod.csproj` and add an entry to `CHANGELOG.md`.

## How it works

`CoinFlip.Flip()` rolls `Random.Range(0f, 1f) < flipHeadsChance`, and the flip animation lasts `flipDuration` seconds. Both are public fields.
The mod writes your values to these fields every frame. It also tracks changes the game makes itself (shop upgrades), so turning an override off restores the correct value.
To play an ending, the mod sets the heads streak to 9, forces heads and seeds `UnityEngine.Random` so that the game's own ending roll lands on the chosen ending (see `src/EndingTrigger.cs`). No Harmony patches are used.

## License

MIT, see [LICENSE](LICENSE). The all-in-one package includes BepInEx (MIT, see `BepInEx_LICENSE.txt`).
Unfair Flips belongs to its developer. This is an unofficial fan mod.
