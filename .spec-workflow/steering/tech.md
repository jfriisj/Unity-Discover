# Technology Stack

## Project Type
- **Unity (Mixed Reality) application** targeting **Meta Quest** devices.
- Built as a sample/reference project that demonstrates end-to-end MR + multiplayer patterns.

## Core Technologies

### Primary Language(s)
- **Language**: C# (Unity scripting)
- **Runtime/Compiler**: Unity runtime + IL2CPP/Mono (depending on build target and Unity settings)
- **Language-specific tools**: Unity Package Manager (UPM), .NET tooling for formatting (see project standards)

### Key Dependencies/Libraries
The project integrates Meta Quest MR and multiplayer tooling. Key dependencies called out in project docs include:
- **Meta XR SDK / Meta XR Utilities / Meta XR Interaction SDK**: MR device capabilities and interaction patterns.
- **Meta Avatars SDK**: Avatar identity and rendering.
- **Photon Fusion**: Networking (session host/join flows).
- **Photon Voice 2**: Voice functionality.
- **Meta Utilities packages (in `Packages/`)**: Reusable utilities and helpers.
- **UniTask**: Async patterns suitable for Unity.

### Application Architecture
- **Scene-driven**: Main entry scene `Assets/Discover/Scenes/Discover.unity` plus example scenes under `Assets/Discover/Scenes/Examples/`.
- **Feature modularity**: Systems grouped by major concerns (e.g., colocation, networking, NUX, spatial anchors, apps).
- **Networked feature shell**: Host/Join/Join Remote session flows and network-spawned app containers.
- **Event-driven where possible**: Prefer explicit dependencies and events over global lookups.

### Data Storage (if applicable)
- **Local persistence**: Placed icon/app data and anchors saved locally (file-based) to support restoring content between runs.
- **Data formats**: Unity/Meta SDK native types + serialized local data (implementation-dependent).

### External Integrations (if applicable)
- **Photon Fusion**: Multiplayer session service.
- **Meta platform services**: Platform initialization, entitlement checks, user identity retrieval.
- **Shared Spatial Anchors**: Anchor sharing for colocation alignment.

### Monitoring & Dashboard Technologies (if applicable)
- **Primary diagnostics**: Unity Console + device logs.
- **Operational metrics of interest**: network join success, colocation/anchor success, anchor load/save outcomes.

## Development Environment

### Build & Development Tools
- **Unity**: 6000.0.50f1 or newer (per project README).
- **Package management**: Unity Package Manager; additional dependencies pulled via Git/UPM or external SDK installs.
- **Dev workflow**: Editor iteration + Quest Link for in-editor testing; device builds for real-world performance validation.

### Code Quality Tools
- **Formatting**: `dotnet format Unity-Discover.sln` (per `Documentation/ProjectStructure.md`).
- **Rules**: `.editorconfig`, `Unity-Discover.sln.DotSettings`, `Assembly-CSharp.csproj.DotSettings`.
- **Testing**: Not a primary focus of this sample; prefer integration validation via scenes and device runs.
- **Documentation**: Markdown under `Documentation/` + README files for packages.

### Version Control & Collaboration
- **VCS**: Git (with Git LFS required for large assets).
- **Reviews**: PR-based review recommended for changes to core systems and sample flows.

## Deployment & Distribution (if applicable)
- **Target platform(s)**: Meta Quest (Android).
- **Distribution**: Sample repository; app build can be deployed via standard Unity Android build pipeline.

## Technical Requirements & Constraints

### Performance Requirements
- Maintain predictable frame time on Meta Quest; avoid heavy work in `Update()` when feasible.
- Prefer batching/async patterns (e.g., UniTask or coroutines) for IO and long-running operations.

### Compatibility Requirements
- Unity 6000.0.50f1+.
- Meta XR SDK / Interaction SDK versions as required by the project’s packages and documentation.

### Security & Compliance
- Platform entitlement checks and user identity retrieval are part of initialization.
- Avoid storing sensitive user data in plaintext local files; keep persistence limited to what the sample needs.

### Scalability & Reliability
- Multiplayer flows should degrade gracefully:
  - “Join Remote” path skips colocation while preserving shared session visibility.
  - Robust error handling around anchor share/load and network connectivity.

## Technical Decisions & Rationale

### Decision Log
1. **Unity + Meta XR stack**: Enables native access to Quest MR features (Scene API, Passthrough, anchors, interactions).
2. **Photon Fusion**: Provides a practical, widely used networking layer for real-time multiplayer.
3. **Feature-area script grouping**: Keeps the sample teachable and discoverable for developers.
4. **Local persistence for placed content**: Simplifies the example and makes iteration fast without requiring backend services.

## Known Limitations
- As a sample/reference project, completeness may prioritize clarity over exhaustive abstraction.
- Device and SDK version constraints can evolve; dependency updates may require periodic maintenance.
