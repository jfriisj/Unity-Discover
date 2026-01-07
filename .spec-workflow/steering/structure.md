# Project Structure

## Directory Organization
This repository is a Unity project. The primary runtime content lives under `Assets/`.

High-level layout:
- `Assets/Discover/` — Discover shell application content (scenes, scripts, prefabs, config).
- `Assets/MRBike/` — MRBike integrated sub-application.
- `Assets/Photon/`, `Assets/Oculus/`, `Assets/MetaXR/`, etc. — third-party SDK content.
- `Packages/` — UPM dependencies (Meta XR SDK, MRUK, UniTask, etc.).
- `Documentation/` — human-readable docs explaining project behavior and setup.

## Naming Conventions

### Files
- **C# scripts**: `PascalCase.cs` matching the primary type.
- **ScriptableObjects / assets**: `PascalCase` or `Title Case` consistent within the owning module.

### Code
- **Classes/Types**: `PascalCase`
- **Methods**: `PascalCase`
- **Private fields**: `camelCase` with underscore prefix (e.g., `_networkRunner`)
- **Serialized fields**: Prefer private `_fieldName` with `[SerializeField]`.

## Unity-Specific Standards (Mandatory)
- No `GameObject.Find*` or `SendMessage` for gameplay/runtime logic.
- Prefer explicit references (serialized fields), factory/spawner patterns, and event-driven communication.
- Heavy logic should not live in `Update()` unless unavoidable; use Jobs/Burst or optimized coroutines.

## Script Location Policy
Primary rule:
- New scripts SHOULD live under `Assets/Discover/Scripts/` (Discover shell) or the owning module’s `Scripts/` folder.

Legacy note:
- Some integrated modules may have scripts outside the shell folder (e.g., `Assets/MRBike/...`). These should be treated as module-owned. If new code is added to those modules, follow their local `Scripts/` folder pattern.

## Import Patterns
- Prefer namespace-based organization matching folder/module boundaries.
- Avoid cross-module coupling: Discover shell code should depend on module public APIs, not internal implementation details.

## Code Size Guidelines
- Keep MonoBehaviour classes focused; prefer composition over “god objects.”
- Prefer small, testable utility classes for pure logic.

## Documentation Standards
- Maintain module-level READMEs for major subsystems.
- Keep `Documentation/` up to date when changing setup, flows, or architecture.
