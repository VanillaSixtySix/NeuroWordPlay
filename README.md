# NeuroWordPlay

BepInEx 5.x mod for [Word Play][WordPlay] to give a [Neuro Game SDK][NeuroGameSDK]-compatible server control over gameplay.

## Usage

> [!TIP]
> A pre-built `.dll` is available through GitHub Actions. This references a provided `Assembly-CSharp.dll` for Word Play version 1.07.

1. Install the latest [BepInEx][BepInExReleases] 5.x release to Word Play
2. Clone or download the repository
3. Reference the necessary `.dll` files
    - For ease-of-use, the necessary files are redistributed through `game/`.
      If testing for a newer version of the game, continue. Otherwise, skip to step 4.
    1. Modify `NeuroWordPlay.csproj` and update `<WordPlay>` in the second `<PropertyGroup>` to point to an absolute path of `<Game Install Directory>/Word Play_Data/Managed`
4. Build the project
5. Copy `bin/Debug/netstandard2.1/NeuroWordPlay.dll` to `BepInEx/plugins/` in the game directory

## Contributing

Outside contributions aren't accepted at this time.

[WordPlay]: https://store.steampowered.com/app/3586660/Word_Play/
[NeuroGameSDK]: https://github.com/VedalAI/neuro-game-sdk
[BepInExReleases]: https://github.com/BepInEx/BepInEx/releases/
