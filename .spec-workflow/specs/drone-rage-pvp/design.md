# Design Document — DroneRage PvP (with Drones)

## Overview
This design adds an authoritative PvP layer to the existing DroneRage app so players can damage each other, respawn, and compete on a scoreboard **while the existing drone wave system continues unchanged**.

The design intentionally reuses DroneRage’s existing networking pattern:
- **State authority / master** owns authoritative decisions (damage, kill credit, respawn timers, match end).
- Clients render synchronized state and drive local input only.

PvP is implemented as a *mode* of DroneRage, not a new gameplay app flow.

## Steering Document Alignment
There are no steering docs in `.spec-workflow/steering/` for this project.

### Technical Standards
- Follow existing Discover/DroneRage patterns (Fusion `NetworkBehaviour`, `[Networked]` state, `RpcSources.StateAuthority` for authoritative RPCs).
- Avoid unnecessary RPC spam; prefer `Networked` properties with `OnChanged` for UI updates.

### Project Structure
- New DroneRage PvP scripts live under `Assets/Discover/DroneRage/Scripts/Pvp/`.
- Minimal, targeted changes to existing DroneRage code paths that already own responsibility (damage, death/respawn, UI).

## Code Reuse Analysis
### Existing Components to Leverage
- `Discover.DroneRage.Bootstrapper.DroneRageBootstrapper`
  - Already spawns the `Spawner` and the `DroneRageGameController` on the master client.
- `Discover.DroneRage.Game.DroneRageGameController`
  - Existing networked singleton and player + weapon spawn hook.
- `Discover.DroneRage.Player.Player`
  - Existing networked health + `TakeDamage` RPC flow.
- `Discover.DroneRage.Weapons.NetworkedWeaponController` and `WeaponHitHandler`
  - Existing authoritative hit resolution on master.
- `Discover.Menus.MainMenuController` + `Discover.Configs.AppManifest` + `Discover.Configs.AppList`
  - Existing “selectable app” launcher flow.

### Integration Points
- **Mode selection**: extend DroneRage to detect PvP mode based on `NetworkApplicationContainer.AppName` (set from `AppManifest.UniqueName`).
  - This allows a second “DroneRage PvP” tile in the launcher without changing the launcher flow.
- **Damage multiplier + PvP enable/disable**: applied in the weapon hit resolution path before calling `IDamageable.TakeDamage`.
- **Death/respawn and match end**: handled by an authoritative match controller that listens for player deaths and schedules respawns.
- **Scoreboard**: UI binds to networked stats (kills/deaths/score) and updates via `OnChanged`.

## Architecture

### Mode Selection (Launcher)
PvP is selected by launching a dedicated app manifest entry:
- Add `AppManifest` asset with `UniqueName = "dronerage-pvp"`, `DisplayName = "DroneRage PvP"`.
- Point `AppPrefab` to the existing DroneRage `NetworkApplicationContainer` prefab (same as `dronerage`).
- Add the new manifest to the `AppList` asset used by the menu.

At runtime, DroneRage checks `AppContainer.AppName`:
- `"dronerage"` => current behavior (co-op vs drones).
- `"dronerage-pvp"` => PvP rules enabled.

This satisfies “PvP mode is selectable” without adding new menu pages or redesigning app launching.

### Authoritative Match Controller
Add a new `NetworkBehaviour` (e.g., `DroneRagePvpMatchController`) attached to the already-networked `DroneRageGameController` prefab.

Responsibilities:
- Own networked match state: `NotStarted` → `Running` → `Ended`.
- Own match timers and stop conditions.
- Record and synchronize per-player PvP scoreboard stats (kills/deaths/score).
- Drive respawn scheduling and apply respawns on state authority.

Match flow:
1. On `DroneRageGameController.Spawned()` and when PvP mode is active:
   - State authority sets match state to `Running` and initializes match start tick/time.
2. When a player dies:
   - State authority credits kill (if attacker known), increments deaths, checks stop conditions.
   - If match still running, schedules respawn after `respawnDelaySeconds`.
3. On match end:
   - Set match state to `Ended` on state authority.
   - Block further scoring and respawns.
   - Show results scoreboard UI.

### Damage and Friendly Fire
Currently, `WeaponHitHandler` reduces damage to any `Player` target by a hard-coded `0.2f`.

In PvP mode, that logic becomes configurable:
- `pvpDamageEnabled == false` => player-to-player damage multiplier = `0`.
- `pvpDamageEnabled == true` => player-to-player damage multiplier = `friendlyFireMultiplier` (default decision below).

Non-PvP mode retains the current behavior (multiplier `0.2f`) to preserve existing balance.

**Authority rule**: hit resolution stays on master/state authority via `NetworkedWeaponController.NetworkedWeaponHitHandler` (already master-only).

### Respawn
Current behavior:
- `Player.Die()` sets `Health = 0`, sets layer to `Ignore Raycast`, and if `PlayersLeft <= 0` triggers game over.

PvP changes:
- In PvP mode, player death should NOT trigger DroneRage “loss” game over.
- The match controller manages respawn:
  - When dead, player is treated as inactive (layer + optionally disabling interactions).
  - After delay, state authority restores HP (to configured value) and moves player to a spawn location.

Spawn locations (no scene anchors required):
- Use deterministic positions around origin based on `PlayerRef.PlayerId` (e.g., points on a ring) to reduce overlap.
- This keeps changes self-contained and avoids scene authoring requirements.

### Scoreboard UI
Add a minimal scoreboard UI shown during PvP and used as the results screen when the match ends.

Data:
- Player display name if available via the existing player identity system (e.g., `DiscoverPlayer.Get(player.Object.StateAuthority).PlayerName` pattern used by the end screen).
- Kills, Deaths, Score.

Update model:
- Stats are stored in `Networked` fields and/or a `Networked` struct array keyed by `PlayerRef.PlayerId`.
- UI updates on `OnChanged` callbacks instead of polling.

## Components and Interfaces

### 1) `DroneRagePvpConfig` (ScriptableObject)
- **Purpose:** Editor-tunable PvP defaults used at match start.
- **Used by:** match controller and weapon hit logic.

Proposed fields (minimum to meet requirements):
- `bool pvpDamageEnabled` (default: `true`)
- `float friendlyFireMultiplier` (default: `1.0`)
- `float respawnDelaySeconds` (default: `5`)
- `float respawnHealth` (default: `100`)
- `int scoreLimit` (default: `20`) — interpreted as “kills to win”
- `float timeLimitSeconds` (default: `300`) — 5 minutes

Defaulting behavior:
- If config reference is missing, use documented defaults above.

### 2) `DroneRagePvpMatchController` (NetworkBehaviour)
- **Purpose:** Own match state, scoring, respawn timers, and match end.
- **Interfaces (public methods/events):**
  - `bool IsPvpActive { get; }`
  - `MatchState State { get; }` (networked)
  - `void NotifyPlayerKilled(Player killer, Player victim)` (state authority only)

### 3) Player extensions (existing `Discover.DroneRage.Player.Player`)
- **Purpose:** Add a minimal API for respawn and avoid PvE game-over logic during PvP.
- **Changes:**
  - Gate the “PlayersLeft <= 0 → TriggerGameOver(false)” logic behind “not in PvP mode”.
  - Add a state-authority-only respawn routine used by the match controller.

### 4) Weapon hit logic (existing `Discover.DroneRage.Weapons.WeaponHitHandler`)
- **Purpose:** Apply PvP damage rules based on mode + config.
- **Changes:**
  - Replace hard-coded `0.2f` multiplier for `Player` targets with:
    - if PvP active: `0` or `friendlyFireMultiplier` based on config.
    - else: `0.2f`.

### 5) Scoreboard UI controller
- **Purpose:** Render networked PvP stats and show results at match end.
- **Dependencies:** match controller + player identity lookup.

## Data Models

### Match State (networked)
```
MatchState:
- notStarted | running | ended

Networked Match Data:
- MatchState state
- int matchStartTick (or float startTime)
- int matchEndTick (optional)
- int winnerPlayerId (optional)
```

### PvP Score Stats
Two viable approaches; select one during implementation based on Fusion constraints and existing patterns:

A) Extend `PlayerStats` with PvP fields:
```
PlayerStats additions:
- uint PvpKills
- uint PvpDeaths
- uint PvpScore
- float DamageDealtToPlayers
- float DamageTakenFromPlayers
```

B) Keep PvP stats in `DroneRagePvpMatchController` keyed by player id:
```
PvpPlayerEntry:
- int playerId
- uint kills
- uint deaths
- uint score
```

Approach A is simpler for UI binding (“stats live on each Player”). Approach B is simpler for match-wide winner calculation.

## Error Handling

### Error Scenarios
1. **PvP config asset missing**
   - **Handling:** fall back to documented defaults (in code) and log one warning on state authority.
   - **User Impact:** PvP still works with sane defaults.

2. **Host leaves / authority changes during match**
   - **Handling:** on detecting loss of master/state authority while match is running, set match to `Ended` and log reason (minimum requirement).
   - **User Impact:** match ends gracefully; scoreboard remains visible.

3. **Respawn failure (no valid spawn position)**
   - **Handling:** use fallback spawn at `(0, 0.8, 0)` with small offset; log warning.
   - **User Impact:** player respawns, possibly near center.

## Testing Strategy

### Unit Testing
- Not required initially (Unity project likely lacks unit test coverage for gameplay scripts). Keep logic small and testable.

### Integration Testing (Unity Play Mode)
- Host starts `dronerage-pvp` app from launcher.
- Join with a second client:
  - Verify both players spawn.
  - Verify player-to-player hits apply damage according to config.
  - Verify deaths increment kills/deaths and player respawns after delay.
  - Verify drones continue spawning and behaving as before.
  - Verify match ends on score limit or time limit; respawns stop; results show.

### Regression Testing
- Launch standard `dronerage` app:
  - Verify friendly-fire reduction remains at `0.2f` and PvE game-over still triggers normally.

## Resolved Open Questions (Design Decisions)
1. **Max player count:** target **4 players** (host + up to 3 clients) as the initial supported experience.
2. **Win condition:** both enabled by default: **first to `scoreLimit` kills OR `timeLimitSeconds` elapsed**, whichever comes first.
3. **Friendly fire multiplier in PvP:** default **`1.0`** (full damage) and configurable.
4. **Spawn points:** use **deterministic computed spawn positions** (no required scene anchors).
