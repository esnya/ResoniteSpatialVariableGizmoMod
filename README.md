# Spatial Variable Gizmo

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/) that adds a **Visualize** button to box- and sphere-based spatial variable inspectors. Press it to draw the same transient debug overlays used by built-in colliders so you can see the evaluated volume without leaving clutter.

Launch the button from any `BoxSpatialVariable*` or `SphereSpatialVariable*` inspector; the overlay disappears once the inspector UI closes.

## Installation

1. Install the [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
2. Build this project or download the latest `SpatialVariableGizmo.dll` release (when available) and place it in your `rml_mods` folder (typically `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods`).
3. Launch Resonite and confirm the mod by opening the inspector for any box/sphere spatial variable and using the **Visualize** button.

## Build & Hot Reload

1. Install the .NET 9 SDK.
2. `dotnet build ResoniteSpatialVariableGizmoMod.sln` auto-detects a Resonite install next to this repo, then the default Steam Windows path, then the default Steam Linux path. If the game lives elsewhere, pass `-p:ResonitePath="/absolute/path/to/Resonite"` so the build can find the Resonite assemblies.
3. Set `CopyToMods=true` when invoking `dotnet build` to copy the compiled DLL into `$(ResonitePath)/rml_mods` after each build.
4. Drop [ResoniteHotReloadLib](https://github.com/Nytra/ResoniteHotReloadLib) into `$(ResonitePath)/rml_libs` and build with `-p:EnableResoniteHotReloadLib=true` if you want **Hot Reload Mods** to reload this mod without restarting Resonite. Leave the property unset on machines without the DLL.

## Versioning

[GitVersion](https://gitversion.net/) supplies semantic versions for builds and packages. Push a `v*` tag (for example `v0.2.0`) and the CI workflow will build, test, and publish release artifacts automatically.
