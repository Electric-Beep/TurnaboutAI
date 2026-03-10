# Turnabout AI

This is a mod for Windows version of [Phoenix Wright: Ace Attorney Trilogy](https://store.steampowered.com/app/787480/Phoenix_Wright_Ace_Attorney_Trilogy/) that integrates the [Neuro-sama game API](https://github.com/VedalAI/neuro-sdk).

## Requirements

- .NET Framework 3.5. On Windows 10/11 this can be enabled in Windows Features.
- An installed copy of Phoenix Wright: Ace Attorney Trilogy.

## Building

Since this mod needs game libraries to build, an environment variable is used to reference the game path.

- Create an environment variable named `PWAAT_PATH` and set it the game's `PWAAT_Data\Managed` folder path.
- Post build commands will copy the mod library and relevant dependencies to `PWAAT_PATH`.

> [!NOTE]
> If you don't want to create an environment variable, you'll need to instead edit `TurnaboutAI.csproj` and replace all references to `$(PWAAT_PATH)` with your game's `PWAAT_Data\Managed` folder path.

## Installation

The game library and revelant dependencies should be placed in `PWAAT_Data\Managed`.

<details>

<summary>Mod file and dependencies</summary>

```
TurnaboutAI.dll
0Harmony.dll
Mono.Cecil.dll
Mono.Cecil.Mdb.dll
Mono.Cecil.Pdb.dll
Mono.Cecil.Rocks.dll
MonoMod.Backports.dll
MonoMod.Core.dll
MonoMod.Iced.dll
MonoMod.ILHelpers.dll
MonoMod.RuntimeDetour.dll
MonoMod.Utils.dll
Newtonsoft.Json.dll
websocket-sharp.dll
```

</details>

This mod uses Doorstop to load. Place the files included in the `ThirdParty` folder into the same folder as the game's executable.

> [!IMPORTANT]
> You will most likely also need to copy `System.Runtime.Serialization.dll` from `%systemroot%\Microsoft.NET\Framework\v3.0\Windows Communication Foundation` into `PWAAT_PATH` in order to get the mod to work correctly.

By default, the mod expects a websocket URL to be present in the environment variable `NEURO_SDK_WS_URL`. This can be overriden by a config file.

## Configuration

Some features of the mod can be configured:
- Console to display log info.
- Whether to not allow actions and instead only send dialogue.
- Safety saves.
- Setting which save slot to use for safety saves.
- The websocket URL.

These settings are located in `TurnaboutAIConfig.json`. Place the config file next to the game's executable to have the mod use it. The file already includes default values.

Refer to `Config.cs` to read descriptions of each config value.

## Notes

I recommend starting the game up at least once before using this mod as the game has a couple of screens that appear on first run.

Spots that can be chosen during Examine sections of the game have vague names because I could not find anywhere that actually related a name to each spot.

I didn't write handling for the minigames in the last episode of game 1, since they were made for DS touch controls, and implementation would essentially be a "Solve Puzzle" action. That isn't much fun, so I'm leaving those minigames for a human companion to solve.