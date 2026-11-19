# Steam Guide: Unfair Flips Coin Mod

Copy each block into the matching field of the Steam guide editor
(Community Hub → Guides → Create Guide). For each section, click **Add Section**,
paste the title into **Section title** and the BBCode into the content field.

The download links point to `https://github.com/Luka3006/unfair-flips-modmenu/releases/latest`,
so the guide never has to be updated for new versions.

---

## Guide title

```
Coin Mod: Heads Chance, Flip Speed & All Endings (Install Guide)
```

## Guide description

```
A small mod to set the heads chance and flip speed and to play any ending instantly. Includes a quick BepInEx setup.
```

---

## Section 1: Overview

**Section title**

```
Overview
```

**Content**

```bbcode
[list]
[*]Set the [b]heads chance[/b] (0–100 %)
[*]Set the [b]flip duration[/b] (0.1–10 s)
[*]Play any of the 5 endings instantly (e.g. for achievements)
[*]In-game menu: [b]F1[/b]
[*]Game files and your save are not modified
[/list]

[b]Warning:[/b] 10 heads in a row ends the game and [b]wipes your save[/b]. Turn on [i]Prevent ending[/i] in the mod menu if you want to keep playing.

[url=https://github.com/Luka3006/unfair-flips-modmenu/releases/latest][b]» Download[/b][/url]
```

---

## Section 2: Installation

**Section title**

```
Installation
```

**Content**

```bbcode
Download [b]UnfairFlipsCoinMod-x.x.x-with-BepInEx.zip[/b] from the [url=https://github.com/Luka3006/unfair-flips-modmenu/releases/latest]download page[/url].

[olist]
[*]In Steam, right-click [b]Unfair Flips[/b] → [b]Manage[/b] → [b]Browse local files[/b].
[*]Extract the zip into that folder.
[*]Start the game and press [b]F1[/b].
[/olist]

The folder should look like this:
[code]
Unfair Flips/
├─ BepInEx/
├─ Unfair Flips_Data/
├─ Unfair Flips.exe
├─ winhttp.dll
└─ doorstop_config.ini
[/code]

[b]Steam Deck / Linux:[/b] set the launch options to
[code]WINEDLLOVERRIDES="winhttp=n,b" %command%[/code]
```

---

## Section 3: Manual BepInEx install

**Section title**

```
Manual BepInEx install (optional)
```

**Content**

```bbcode
Only needed if you want to install BepInEx yourself instead of using the all-in-one zip.

[olist]
[*]Download [b]BepInEx_win_x64_5.4.23.4.zip[/b] from the [url=https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.4]BepInEx releases[/url].
[*]Extract it into the game folder (next to [i]Unfair Flips.exe[/i]).
[*]Start the game once, then close it. BepInEx creates its folders.
[*]Download [b]UnfairFlipsCoinMod-x.x.x.zip[/b] (without BepInEx) and extract it into the game folder.
[/olist]
```

---

## Section 4: Settings & troubleshooting

**Section title**

```
Settings & troubleshooting
```

**Content**

```bbcode
[table]
[tr][th]Setting[/th][th]Range[/th][th]Game default[/th][/tr]
[tr][td]Heads chance[/td][td]0–100 %[/td][td]20 %[/td][/tr]
[tr][td]Flip duration[/td][td]0.1–10 s[/td][td]5 s[/td][/tr]
[tr][td]Prevent ending[/td][td]on/off[/td][td]off[/td][/tr]
[/table]

[b]Play ending:[/b] open [i][+] Play ending[/i] in the menu and pick an ending. It plays like a normal 10th heads, so the achievement unlocks too. Like every ending, it [b]wipes your save[/b].

Config file: [i]BepInEx/config/luka3006.unfairflips.coinmod.cfg[/i]

[b]No menu?[/b] Make sure [i]winhttp.dll[/i] is next to [i]Unfair Flips.exe[/i]. After starting the game, [i]BepInEx/LogOutput.log[/i] should exist.

[b]Uninstall:[/b] delete [i]winhttp.dll[/i] (disables all mods) or the folder [i]BepInEx/plugins/UnfairFlipsCoinMod[/i].
```
