# Product Overview

## Product Purpose
Discover is a Mixed Reality (MR) Unity project for Meta Quest that demonstrates end-to-end patterns for building MR experiences: scene understanding (Scene API), passthrough, interaction, spatial anchors, shared spatial anchors/colocation, and multiplayer session flow.

The project is intentionally structured as both:
- A runnable sample app (for headset testing and demos)
- A reference/template codebase that teams can copy patterns from

## Target Users
- Unity developers building MR applications for Meta Quest
- Technical designers and engineers evaluating Meta XR features and recommended integration patterns
- Teams looking for a starting point for a networked MR app (host/join/remote)

## Key Features
1. **MR foundations**: Scene API room loading, passthrough setup, interaction via Interaction SDK, and MR Utility Kit usage.
2. **Multiplayer session flow**: Host/Join/Join-Remote with Photon Fusion networking and colocation anchoring.
3. **Anchor-backed content placement**: Place/move 3D app icons that persist across sessions using spatial anchors.
4. **Applications framework**: Launchable “apps” within the Discover shell (e.g., sample experiences like DroneRage and MRBike).
5. **New User Experience (NUX)**: Guided onboarding panels for networking and in-app interaction.

## Business Objectives
- Provide a high-quality, maintainable reference implementation for Meta Quest MR features.
- Reduce integration time for teams adopting Scene API / anchors / networking.
- Demonstrate best practices for performance-aware XR development (especially avoiding per-frame heavy logic).

## Success Metrics
- **Stability**: No repeated runtime exceptions in core flows (launch → host/join → place/launch apps).
- **Performance**: Maintains headset target framerate for the intended device configuration under typical usage.
- **Usability**: First-time users can successfully connect and place/launch apps with minimal friction.
- **Adoption**: Internal/external teams reuse modules (e.g., spatial anchor persistence, session UI, app manifest patterns).

## Product Principles
1. **Clarity over cleverness**: Prefer readable, teachable patterns over “magic.”
2. **Performance-first XR**: Avoid heavy work in `Update()`; use events, Jobs/Burst where appropriate, and optimized coroutines.
3. **Composable systems**: Keep features modular so sub-experiences can be added/removed without rewriting core flows.

## Monitoring & Visibility (if applicable)
- **Dashboard Type**: Unity tooling (Profiler, Frame Debugger) and in-app debug UI/logging where present.
- **Real-time Updates**: N/A (runtime UX); networking state is surfaced via UI flows and logs.
- **Key Metrics Displayed**: Session status, user/app state, and relevant diagnostics.
- **Sharing Capabilities**: N/A.

## Future Vision
### Potential Enhancements
- **Performance instrumentation**: Built-in capture helpers (profiling markers, trace capture hooks) for repeatable perf analysis.
- **App extensibility**: Stronger “app module” boundaries with clearer contracts between shell and apps.
- **Collaboration**: More multi-user flows (roles, permissions) and richer spatial synchronization patterns.
