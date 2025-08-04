# NeuroWordPlay

BepInEx 5.x mod for [Word Play][WordPlay] to give a [Neuro Game SDK][NeuroGameSDK]-compatible server control over gameplay.

## Usage

> [!TIP]
> A pre-built `.dll` is available through [GitHub Actions][GitHubActions]. This is pinned to Word Play version 1.07.

1. Install the latest [BepInEx][BepInExReleases] 5.x release to Word Play
2. Clone or download the repository
3. Reference the necessary `.dll` files
    - For ease-of-use, the necessary files are redistributed through `game/`.
      If testing for a newer version of the game, continue. Otherwise, skip to step 4.
    1. Modify `NeuroWordPlay.csproj` and update `<WordPlay>` in the second `<PropertyGroup>` to point to an absolute path of `<Game Install Directory>/Word Play_Data/Managed`
4. Build the project
5. Copy `bin/Debug/netstandard2.1/NeuroWordPlay.dll` to `BepInEx/plugins/` in the game directory
6. Launch the game with the `NEURO_SDK_WS_URL` environment variable set.
    - If running through Steam: right click Word Play in your library, click 'Properties', then enter the following into 'Launch Options': `NEURO_SDK_WS_URL=ws://localhost:8000 %command%`

## Contributing

Please ask and discuss in the Word Play Integration thread in Neuro-sama Headquarters beforehand.

[WordPlay]: https://store.steampowered.com/app/3586660/Word_Play/
[NeuroGameSDK]: https://github.com/VedalAI/neuro-game-sdk
[GitHubActions]: https://github.com/VanillaSixtySix/NeuroWordPlay/actions/workflows/build.yml
[BepInExReleases]: https://github.com/BepInEx/BepInEx/releases/
