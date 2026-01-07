# Product Overview

## Product Purpose
Discover is a Mixed Reality (MR) Unity project that demonstrates how to use key Meta Quest MR capabilities and integrate them into a production-ready project structure.

It serves as both:
- A reference implementation for common MR patterns (passthrough, Scene API, spatial anchors, colocation)
- A template starting point for teams building their own MR experiences

## Target Users
- **Unity MR developers (Meta Quest)**: Need concrete examples of MR features integrated end-to-end.
- **Prototype teams / technical designers**: Need a working baseline to iterate on interactions and UX.
- **Engineering teams integrating networking + MR**: Need proven flows for colocation, shared anchors, and synced content.

## Key Features
1. **MR Foundations**: Passthrough setup and MR scene bootstrapping.
2. **Scene API Integration**: Loads room layout and spawns environment elements.
3. **Colocation via Shared Spatial Anchors**: Host creates a shared anchor; colocated participants align into a common frame.
4. **Networking**: Session-based multiplayer flows (host / join / join remote) using Photon Fusion.
5. **In-app Menu + App Containers**: Users place, move, and launch “applications” (networked containers) using icons.
6. **Persistence of Placed Content**: Placed 3D icons can be saved to disk and restored (anchored via spatial anchors).

## Business Objectives
- Provide a high-quality sample that accelerates MR development on Meta Quest.
- Demonstrate best-practice integration of Meta XR features with real multiplayer flows.
- Reduce time-to-first-prototype for teams by offering a working, modular baseline.

## Success Metrics
- **Time to first run**: New developer can open the project and run the main scene successfully with documented setup.
- **Feature comprehension**: Developers can locate and understand feature areas via `Documentation/` and project organization.
- **Stability**: Main flow (host/join, load room, place/launch apps) operates reliably on supported Quest devices.
- **Adoption/usage**: Internal/external usage measured via clones, sample downloads, or developer feedback.

## Product Principles
1. **Reference-quality clarity**: Prefer explicit, readable implementations over clever abstractions.
2. **MR-first UX**: Optimize flows for headset use (hand tracking/controllers, constrained input, comfort).
3. **Modular by feature**: Keep systems (colocation, anchors, networking, NUX, apps) separable and reusable.
4. **Performance-aware**: Avoid unnecessary per-frame work; prioritize stability and predictable performance on device.

## Monitoring & Visibility (if applicable)
- **Primary visibility**: Unity Console logs + on-device debugging.
- **Operational focus**: Networking connection status, colocation success/failure, anchor load/save outcomes.

## Future Vision
- Expand or refine example applications (e.g., additional “apps” beyond current set) while keeping the core shell stable.
- Continue improving documentation and troubleshooting guidance for common setup issues.
- Add more focused example scenes demonstrating isolated systems where helpful.

### Potential Enhancements
- **Analytics/telemetry hooks** (opt-in): Basic counters for session join success and anchor alignment outcomes.
- **Collaboration workflows**: More ergonomic in-editor tooling for testing multiplayer and MR flows.
