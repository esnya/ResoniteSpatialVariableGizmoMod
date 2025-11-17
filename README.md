# SpatialVariableGizmo

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/) that adds a **Visualize** button to the inspector for box- and sphere-based spatial variables. Pressing the button renders the same transient debug overlays used by built-in colliders so you can see the evaluated volume without leaving persistent clutter.

## Design Brief (2025-11-16)

- **Goal:** Provide collider-style visualization for `BoxSpatialVariable*` and `SphereSpatialVariable*` inspectors without altering spatial-variable behavior or leaving persistent gizmos.
- **Scope & placement:** Harmony patch `WorkerInspector.BuildUIForComponent` to append a visualize button for the targeted types only. Visualization reuses `Debug.Box` / `Debug.Sphere` with the same transform inputs as spatial-variable sampling. No BlendDistance handling for now (explicitly out of scope).
- **API & lifecycle:** Button lives in the inspector; it disables itself after the first press and drives a per-frame debug draw loop until the UI element is destroyed. No new public API surface.
- **Dependencies & testing:** .NET 9, Resonite install providing `FrooxEngine.dll`. Automated coverage remains the template property checks; manual validation occurs in-game by pressing the new button and verifying the overlay matches spatial-variable bounds.

## Installation

1. Install the [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
2. Build this project or download the latest `SpatialVariableGizmo.dll` release (when available) and place it in your `rml_mods` folder (typically `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods`).
3. Launch Resonite and confirm the mod by opening the inspector for any box/sphere spatial variable and using the **Visualize** button.

## Development

### Requirements

- .NET 9 SDK
- A Resonite installation that exposes `FrooxEngine.dll` (auto-discovered for the default Steam paths; otherwise pass `-p:ResonitePath=/absolute/path/to/Resonite`)
- [ResoniteHotReloadLib](https://github.com/Nytra/ResoniteHotReloadLib) if you plan to use hot reload

### Setup

1. Clone this repository.
2. Ensure the Resonite installation path is reachable (add `-p:ResonitePath="..."` to build/test commands if needed).
3. Build the project: `dotnet build`

### Workflow

- Before committing, run `dotnet format ResoniteSpatialVariableGizmoMod.sln --verify-no-changes --no-restore`.
- Keep local builds/tests aligned with CI by running `dotnet build ResoniteSpatialVariableGizmoMod.sln -c Release -p:ResonitePath="..."` and `dotnet test ResoniteSpatialVariableGizmoMod.sln -c Release -p:ResonitePath="..."`.
- Refer to `AGENTS.md` for the authoritative checklist shared with CI.

### Install to `rml_mods` (and `rml_mods/HotReloadMods`)

Set `CopyToMods=true` when building to mirror the compiled DLL into your Resonite install automatically:

```bash
dotnet build -p:CopyToMods=true -p:ResonitePath="C:\Program Files (x86)\Steam\steamapps\common\Resonite"
```

### Hot Reload Development

Opt in to hot reload by dropping [ResoniteHotReloadLib](https://github.com/Nytra/ResoniteHotReloadLib) into `$(ResonitePath)/rml_libs` and passing `-p:EnableResoniteHotReloadLib=true` (combine with `CopyToMods=true` if you also want the HotReloadMods copy). Without that property the project omits both the reference and compiler symbol so developers without the DLL can still build.

### Versioning & Releases

[GitVersion](https://gitversion.net/) supplies semantic versions for builds and packages. Push a `v*` tag (for example `v0.2.0`) and the CI workflow will build, test, and publish the release artifacts automatically.
