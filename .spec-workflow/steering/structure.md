# Project Structure

## Directory Organization

This repository is a Unity project. The primary structure follows standard Unity conventions plus project-specific organization under `Assets/Discover` and related application folders.

High-level layout:

- **Assets/**: Unity content (scenes, prefabs, scripts, art, settings).
  - `Assets/Discover/`: Main sample application content.
  - `Assets/MRBike/`: Standalone application integrated into Discover.
  - Additional third-party or package-provided content (e.g., Photon, Oculus/Meta assets) as present.
- **Packages/**: Unity Package Manager packages (including Meta utilities packages used by the project).
- **ProjectSettings/**: Unity project settings.
- **Documentation/**: Human-readable project docs (configuration, overview, structure).
- **Library/**, **Temp/**, **Logs/**: Unity-generated or local state (not source-of-truth).

Within `Assets/Discover`, the project is primarily organized by asset type for navigation clarity.

### Key Discover locations
- **Config** (`Assets/Discover/.../Config`): ScriptableObjects for App Manifests and App List.
- **Scenes** (`Assets/Discover/Scenes/`):
  - Main entry scene: `Assets/Discover/Scenes/Discover.unity`
  - Example scenes: `Assets/Discover/Scenes/Examples/` (colocation, room mapping, MR setup, startup)
- **Scripts** (`Assets/Discover/Scripts/`): Primary gameplay/application logic, grouped by feature area:
  - `Colocation/`
  - `Networking/`
  - `NUX/`
  - `SpatialAnchors/`
  - `FakeRoom/` (editor-friendly substitute for headset-based room loading)

### Applications
- **DroneRage** (`Assets/Discover/DroneRage/`): Organized by game elements; scripts in `Assets/Discover/DroneRage/Scripts/`.
- **MRBike** (`Assets/MRBike/`): Standalone application; entry prefab `Assets/MRBike/Prefabs/BikeInteraction.prefab`.

## Naming Conventions

### Files
- **C# scripts**: `PascalCase.cs` matching the primary type name.
- **Unity assets**: Use descriptive `PascalCase` or Unity-typical naming; avoid ambiguous abbreviations.

### Code (C#)
- **Classes/Structs/Enums**: `PascalCase`
- **Methods/Properties**: `PascalCase`
- **Private fields**: `camelCase` with underscore prefix (e.g., `_playerRig`) to align with common Unity conventions.

## Import Patterns
- Prefer explicit references via serialized fields, dependency injection patterns, or clear composition.
- Avoid global lookups (e.g., avoid `GameObject.Find`) and message-based APIs (avoid `SendMessage`).

## Code Structure Patterns
- Favor small, single-responsibility MonoBehaviours that compose into higher-level systems.
- Keep feature logic grouped with its owning subsystem (e.g., colocation logic under `Scripts/Colocation`).
- Prefer event-driven interactions between subsystems to reduce coupling.

## Code Organization Principles
1. **Single Responsibility**: Each script owns one clear behavior.
2. **Modularity**: Feature subsystems are reusable (colocation, anchors, NUX, networking).
3. **Consistency**: Follow existing patterns in `Assets/Discover/Scripts`.
4. **Performance**: Avoid heavy per-frame work; use coroutines/async patterns for IO and long operations.

## Module Boundaries
- **Core shell vs applications**:
  - Discover provides the session/menu/placement shell.
  - Individual “applications” (e.g., DroneRage, MRBike) contain their own logic and content.
- **Networking vs local systems**:
  - Networking concerns live in `Scripts/Networking` (and app-specific networking where required).
  - Persistence/anchors live in `Scripts/SpatialAnchors`.

## Code Size Guidelines
- Keep scripts focused and readable.
- Prefer refactoring if a single MonoBehaviour grows large or crosses multiple responsibilities.

## Documentation Standards
- Project-level docs live in `Documentation/`.
- Major subsystems should have brief READMEs if they grow in scope.
- Keep setup steps accurate and aligned with `Documentation/Configuration.md`.

## Formatting / Standards Enforcement
- Formatting is applied via `dotnet format Unity-Discover.sln`.
- Primary rule sources: `.editorconfig`, solution DotSettings, and csproj DotSettings.
