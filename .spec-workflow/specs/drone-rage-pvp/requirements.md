# Requirements Document — DroneRage PvP (with Drones)

## Introduction
This feature extends the DroneRage application inside Discover so multiple players can fight each other (PvP) **while** drone waves continue as they do today.

The goal is a PvP mode that:
- reuses the existing Photon Fusion networking and DroneRage gameplay components
- stays clearly scoped (no redesign of the Discover launcher flow)
- is testable and implementation-ready without guessing

## Alignment with Product Vision
There are no steering docs in `.spec-workflow/steering/` for this project. Therefore, the implementation must follow the existing project structure and coding standards described in `Documentation/ProjectStructure.md`.

## Requirements

### Requirement 1 — PvP mode is selectable and network-correct
**User Story:** As a player, I want to start DroneRage in a PvP mode so that I can fight other players in the same session.

#### Acceptance Criteria
1. WHEN a DroneRage session starts in PvP mode THEN the system SHALL initialize match state synchronized via Photon Fusion.
2. WHEN a player joins an ongoing PvP session THEN the system SHALL spawn the player's Player + weapons consistent with the existing DroneRage spawn flow.
3. IF a client is not master/state authority THEN the system SHALL NOT execute authoritative game-state decisions (e.g., match start/end, respawn timer), and SHALL only display synchronized state.

### Requirement 2 — Players can damage each other (PvP damage)
**User Story:** As a player, I want to damage other players so that PvP is possible.

#### Acceptance Criteria
1. WHEN a player hits another player with a valid weapon hit THEN the system SHALL apply damage to the hit Player object over the network.
2. IF PvP damage is disabled in the PvP config THEN the system SHALL NOT reduce HP of other players.
3. WHEN PvP damage is applied THEN the system SHALL update relevant stats for attacker and target (at minimum DamageDealtToPlayers / DamageTakenFromPlayers, or equivalent).

### Requirement 3 — Drones continue alongside PvP
**User Story:** As a player, I want drone waves to continue so that the game remains chaotic and challenging even with PvP.

#### Acceptance Criteria
1. WHEN PvP mode is active THEN the system SHALL continue drone spawning based on the existing Spawner logic.
2. IF there are 2+ players in the session THEN the system SHALL scale drone spawning/pace at least as in the existing multiplayer behavior.

### Requirement 4 — Respawn in PvP (no permanent elimination)
**User Story:** As a player, I want to respawn after dying so that PvP can continue.

#### Acceptance Criteria
1. WHEN a player dies in PvP mode THEN the system SHALL mark the player as "dead" and start a respawn timer.
2. WHEN the respawn timer elapses THEN the system SHALL respawn the player at a valid spawn point and restore the player's HP to a defined value.
3. IF the match has ended THEN the system SHALL NOT respawn players.

### Requirement 5 — Match rules (win condition)
**User Story:** As a player, I want clear rules for when the match ends so that there is a goal.

#### Acceptance Criteria
1. WHEN any match stop condition is met (e.g., score limit or time limit) THEN the system SHALL set match state to "ended" on state authority.
2. WHEN match state becomes "ended" THEN the system SHALL stop scoring/respawn and show results.

### Requirement 6 — Scoreboard and basic results
**User Story:** As a player, I want to see score, kills, and deaths so that I can understand who is winning.

#### Acceptance Criteria
1. WHEN PvP mode is active THEN the system SHALL display a scoreboard UI with at least: PlayerId/name (if available), Kills, Deaths, Score.
2. WHEN a player gets a kill THEN the system SHALL update the scoreboard for all players in the session.

### Requirement 7 — Configuration (balancing without code changes)
**User Story:** As a developer/designer, I want to tune PvP values so that we can balance without changing scripts.

#### Acceptance Criteria
1. WHEN the PvP config is changed (in editor) THEN the system SHALL use those values on the next match start (at minimum: respawn delay, score limit/time limit, PvP damage on/off, friendly fire multiplier).
2. IF the config is not set THEN the system SHALL fall back to documented defaults.

## Non-Functional Requirements

### Code Architecture and Modularity
- Extend existing DroneRage scripts rather than introducing parallel systems.
- New scripts must have single responsibility (e.g., `PvpMatchController`, `PvpScoring`, `PvpRespawnService`).

### Performance
- Match state updates must not spam RPCs unnecessarily; prefer Networked properties where appropriate.
- Scoreboard updates must be event-driven (on-changed), not polling.

### Security
- Authoritative decisions (damage/scoring/respawn/match-end) must occur on state authority/master.

### Reliability
- The system must not deadlock on host migration/disconnect; at minimum: a fail-safe that ends the match and logs the reason.

### Usability
- UI must be readable in MR and require minimal interaction (automatic results display when the match ends).

## Open Questions (must be resolved in Design and approved)
1. Max player count: 2? 4? 8?
2. Win condition: time limit, score limit, or both?
3. Friendly fire multiplier: keep existing 0.2, or full damage in PvP?
4. Spawn points: reuse existing spawn logic or add dedicated spawn anchors in the scene?
