# Technology Stack

## Project Type
Unity 6 Mixed Reality application targeting Meta Quest (Android) with Editor testing via Quest Link.

## Core Technologies

### Primary Language(s)
- **Language**: C# (Unity)
- **Runtime/Compiler**: Unity 6000.x (Unity 6)
- **Language-specific tools**: `dotnet format` for solution-wide formatting (see `.editorconfig` and `.DotSettings`).

### Key Dependencies/Libraries
- **Meta XR SDK**: Core, Platform, Audio, Simulator (package versions pinned in `Packages/manifest.json`).
- **Meta XR MR Utility Kit**: MRUK for scene understanding utilities.
- **Meta XR Interaction SDK**: Interaction and input patterns.
- **Meta Avatars SDK**: Avatar rendering and identity integration.
- **Photon Fusion**: Multiplayer networking.
- **Photon Voice 2**: Voice chat.
- **UniTask**: Async/await patterns optimized for Unity.
- **URP**: Universal Render Pipeline.

### Application Architecture
- Unity component-oriented architecture with a single entry scene (`Assets/Discover/Scenes/Discover.unity`).
- ScriptableObject-driven configuration for app manifests and app lists.
- Networked object lifecycles via Photon Fusion (spawn/despawn, network behaviors).
- Event-driven state transitions (preferred) vs polling.

### Data Storage (if applicable)
- **Primary storage**: Unity assets (ScriptableObjects) + local file persistence for certain runtime data (e.g., anchor/icon placement state).
- **Data formats**: Unity serialization, JSON where appropriate.

### External Integrations (if applicable)
- **Meta Platform**: Entitlement checks and user identity retrieval.
- **Photon**: Session hosting/joining and voice.

## Development Environment

### Build & Development Tools
- Unity Editor (Unity 6000.0.50f1+; project currently on 6000.0.62f1).
- JetBrains Rider / Visual Studio integration packages.
- Android Logcat package for device logs.

### Code Quality Tools
- Formatting: `dotnet format Unity-Discover.sln`.
- EditorConfig + Rider/Resharper settings via `.DotSettings`.

### Version Control & Collaboration
- **VCS**: Git.
- **Code Review Process**: PR-based review (recommended).

## Technical Requirements & Constraints

### Performance Requirements
- XR framerate stability is a primary requirement.
- Avoid heavy per-frame allocations and CPU work; favor:
  - Event-driven updates
  - Job System/Burst for heavy computation
  - Coroutines/UniTask for structured asynchronous flows

### Compatibility Requirements
- **Platform Support**: Meta Quest (Android) + Editor simulation/testing.

### Security & Compliance
- Treat platform authentication/entitlement as required for store-ready builds.

## Technical Decisions & Rationale
### Decision Log
1. **URP**: Chosen for cross-platform rendering and XR compatibility.
2. **Photon Fusion**: Chosen to provide robust session management and state replication.
3. **ScriptableObject configs**: Chosen for designer-friendly configuration without code changes.

## Known Limitations
- Multiple “sub-apps” exist with their own internal structure; long-term maintainability depends on clear module boundaries.
- XR performance constraints require continuous profiling and regression checks as features are added.
