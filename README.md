# King's Last Castle

A 3D tower-defense game built with **C#**, **.NET 9** and **MonoGame DesktopGL**. Players defend a castle across three maps, multiple waves and two difficulty levels using three tower types, three enemy types and a Royal Power ability.

## Gameplay systems

- Three maps with three waves per map
- Arrow, cannon and crystal towers
- Multiple enemy archetypes and route switching
- Gold, castle health, Gate Energy and Royal Power resources
- Menus, pause state, HUD, victory/defeat and local progress
- Rotating 3D camera, effects and layered audio
- Automated checks plus manual gameplay testing

## Aziz's documented contribution

Aziz acted as **team lead** and focused primarily on gameplay flow, game states, menus and HUD integration. His documented work includes wave flow, gold and castle health, Royal Power activation, route information in the HUD, state integration for enemies, audio/effect events, final integration and acceptance.

## Team

University team project by **Aziz Zarrouki, Jihed Khattat and Rayan Brigui**. The final report attributes enemy/movement/path work mainly to Jihed and tower/projectile/camera/effect work mainly to Rayan.

## Build

```bash
dotnet restore
dotnet build
dotnet run
```

Verified locally with .NET 9: **build succeeded with 0 warnings and 0 errors**.

## Controls

- `1`, `2`, `3` — select a tower type
- Mouse — interact with menus and place towers
- `A` / `D` — rotate the camera
- `Space` — start a wave where available
- `Esc` — pause/resume or return from submenus

## Credits and repository scope

The repository contains the final source snapshot and the assets required to build the game. The 3D models and tower UI icons were created by the university project team. Verified creator, source, license and conversion details for every audio file are recorded in [`Content/Audio/ATTRIBUTION.txt`](Content/Audio/ATTRIBUTION.txt), while all visual-asset ownership is summarized in [`Content/ASSET_CREDITS.md`](Content/ASSET_CREDITS.md).

The team selected, converted and integrated the credited Pixabay audio; the original recordings remain the work of their listed creators. Audio files are included only as integrated game resources and are not offered for standalone reuse. Team-authored work and third-party assets retain their respective rights; no blanket repository license is asserted.
