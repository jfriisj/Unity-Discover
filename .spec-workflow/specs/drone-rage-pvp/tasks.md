# Tasks Document — DroneRage PvP (with Drones)

> Notes
> - Keep tasks small (1–3 files each) and follow existing DroneRage/Fusion patterns.
> - Any Unity asset/prefab edits must be described as explicit Editor operations.
> - After completing each task: run the relevant validation step(s), log via `log-implementation`, then mark the task `[x]`.

- [x] 1. Add a launcher entry for DroneRage PvP (AppManifest + AppList)
  - Unity Operations:
    - In Unity Project window, duplicate `Assets/Discover/Configs/AppManifests/DroneRage.asset`.
    - Rename duplicate to `DroneRagePvp.asset` and set:
      - `UniqueName = dronerage-pvp`
      - `DisplayName = DroneRage PvP`
      - Keep `AppPrefab` pointing to `Assets/Discover/Prefabs/DroneRage/DroneRageNetworkApplication.prefab`.
    - Open `Assets/Discover/Configs/AppList.asset` and add `DroneRagePvp.asset` to `AppManifests`.
  - Purpose: Make PvP selectable from the existing Discover launcher without new menu UX.
  - _Leverage: Assets/Discover/Configs/AppManifests/DroneRage.asset, Assets/Discover/Configs/AppList.asset_
  - _Requirements: 1_
  - _Prompt: Role: Unity Developer (Discover launcher integration) | Task: Add a new AppManifest entry for `dronerage-pvp` and register it in the existing `AppList` so it appears as a selectable tile, reusing the same DroneRage app prefab | Restrictions: Do not add new menu pages or settings UI; do not create a new app prefab; only add a new manifest entry and update the existing list | _Leverage: Assets/Discover/Configs/AppManifests/DroneRage.asset, Assets/Discover/Configs/AppList.asset, Assets/Discover/Prefabs/DroneRage/DroneRageNetworkApplication.prefab | _Requirements: 1 | Success: `DroneRage PvP` appears in the launcher; launching it sets `NetworkApplicationContainer.AppName` to `dronerage-pvp` and starts the same DroneRage app prefab

- [x] 2. Create PvP config ScriptableObject + default asset
  - Files:
    - `Assets/Discover/DroneRage/Scripts/Pvp/DroneRagePvpConfig.cs` (new)
  - Unity Operations:
    - Create folder `Assets/Discover/DroneRage/Configs/` if missing.
    - Create an asset `Assets/Discover/DroneRage/Configs/DroneRagePvpConfig_Default.asset` from the new CreateAssetMenu entry.
    - Set defaults (as per design): `pvpDamageEnabled=true`, `friendlyFireMultiplier=1`, `respawnDelaySeconds=5`, `respawnHealth=100`, `scoreLimit=20`, `timeLimitSeconds=300`.
  - Purpose: Allow balancing PvP without code changes.
  - _Leverage: (patterns) Assets/Discover/Scripts/Configs/AppManifest.cs_
  - _Requirements: 7_
  - _Prompt: Role: Unity Gameplay Engineer (data/config) | Task: Implement `DroneRagePvpConfig` ScriptableObject with the fields defined in the approved design and create a default asset with documented defaults | Restrictions: Keep the config minimal; do not add runtime UI; do not hardcode values outside the documented fallback defaults | _Leverage: Assets/Discover/Scripts/Configs/AppManifest.cs (CreateAssetMenu pattern) | _Requirements: 7 | Success: A `DroneRagePvpConfig_Default.asset` exists and can be referenced by gameplay scripts; if the config reference is missing at runtime, code falls back to the documented defaults

- [x] 3. Implement PvP mode detection helper (single source of truth)
  - Files:
    - `Assets/Discover/DroneRage/Scripts/Pvp/DroneRagePvpMode.cs` (new)
  - Purpose: Centralize “is this match PvP?” logic based on `NetworkApplicationContainer.AppName`.
  - _Leverage: Assets/Discover/DroneRage/Scripts/Game/DroneRageGameController.cs, Assets/Discover/Scripts/NetworkApplicationContainer.cs_
  - _Requirements: 1_
  - _Prompt: Role: Unity Networking Engineer (Fusion/Discover) | Task: Add a small helper (static class) that determines if PvP mode is active by reading the active `NetworkApplicationContainer.AppName` and comparing against `dronerage-pvp` | Restrictions: Do not change launcher flow; do not add settings UI; keep this helper pure and reusable | _Leverage: Assets/Discover/DroneRage/Scripts/Game/DroneRageGameController.cs, Assets/Discover/Scripts/NetworkApplicationContainer.cs | _Requirements: 1 | Success: Other PvP systems can call one helper to detect PvP mode reliably in host and clients

- [x] 4. Extend PlayerStats with PvP scoreboard fields
  - Files:
    - `Assets/Discover/DroneRage/Scripts/Player/PlayerStats.cs` (modify)
  - Changes:
    - Add `[Networked]` fields: `PvpKills`, `PvpDeaths`, `PvpScore`, `DamageDealtToPlayers`, `DamageTakenFromPlayers`.
  - Purpose: Provide synchronized per-player scoreboard stats.
  - _Leverage: Assets/Discover/DroneRage/Scripts/Player/PlayerStats.cs (existing networked stats pattern)_
  - _Requirements: 2, 6_
  - _Prompt: Role: Unity Networking Engineer (Fusion) | Task: Extend `PlayerStats` with networked PvP stats required for a scoreboard (kills, deaths, score, damage dealt/taken vs players) | Restrictions: Keep existing fields and behavior intact; only add new fields; do not change non-PvP scoring | _Leverage: Assets/Discover/DroneRage/Scripts/Player/PlayerStats.cs | _Requirements: 2, 6 | Success: PvP stats replicate to all clients and are safe to bind in UI via OnChanged updates

- [x] 5. Make player damage invoke DamageCallback (enables attacker attribution)
  - Files:
    - `Assets/Discover/DroneRage/Scripts/Player/Player.cs` (modify)
  - Changes:
    - In the state-authority damage path, invoke `IDamageable.DamageCallback` similarly to how enemies do, so weapon owners can attribute damage/kills.
  - Purpose: Allow PvP scoring to know who dealt damage and who got the kill.
  - _Leverage: Assets/Discover/DroneRage/Scripts/Weapons/IDamageable.cs, Assets/Discover/DroneRage/Scripts/Enemies/Enemy.cs (callback usage pattern), Assets/Discover/DroneRage/Scripts/Weapons/NetworkedWeaponController.cs (OnDamage hook)_
  - _Requirements: 2, 6_
  - _Prompt: Role: Unity Gameplay Engineer (combat) | Task: Update `Player` damage handling to invoke the `DamageCallback` with `(damageableAffected, hpAffected, targetDied)` so `NetworkedWeaponController` can attribute player-vs-player damage and kills | Restrictions: Do not change drone damage behavior; do not alter existing health replication logic beyond adding the callback invocation | _Leverage: Assets/Discover/DroneRage/Scripts/Weapons/IDamageable.cs, Assets/Discover/DroneRage/Scripts/Enemies/Enemy.cs, Assets/Discover/DroneRage/Scripts/Weapons/NetworkedWeaponController.cs | _Requirements: 2, 6 | Success: When a player is hit by a weapon, the weapon owner receives `OnDamage` callback events for player targets including `targetDied=true` on lethal hits

- [x] 6. Implement PvP match controller: match state + stop conditions
  - Files:
    - `Assets/Discover/DroneRage/Scripts/Pvp/DroneRagePvpMatchController.cs` (new)
  - Responsibilities (minimum):
    - Networked match state (`NotStarted`, `Running`, `Ended`).
    - Networked match start tick/time.
    - Time-limit end condition (default 300s).
    - Score-limit end condition (default 20 kills).
    - Fail-safe: if authority/match ownership is lost mid-match, end match and log reason.
  - Purpose: Own authoritative PvP match lifecycle.
  - _Leverage: Assets/Discover/DroneRage/Scripts/Game/DroneRageGameController.cs (network singleton patterns), Assets/Discover/Scripts/NetworkApplicationContainer.cs_
  - _Requirements: 1, 5, 7_
  - _Prompt: Role: Unity Networking Engineer (Fusion) | Task: Implement `DroneRagePvpMatchController` as a `NetworkBehaviour` that runs only in PvP mode and owns the authoritative match lifecycle and stop conditions (time limit / score limit) using `Networked` state with OnChanged for UI | Restrictions: Do not stop drone spawning; do not introduce new scene dependencies; avoid RPC spam; keep logic state-authority-owned | _Leverage: Assets/Discover/DroneRage/Scripts/Game/DroneRageGameController.cs, Assets/Discover/Scripts/NetworkApplicationContainer.cs, Assets/Discover/DroneRage/Scripts/Pvp/DroneRagePvpMode.cs | _Requirements: 1, 5, 7 | Success: In PvP mode the match transitions to Running, ends on score/time limits, and sets match state Ended consistently for all clients

- [x] 7. Wire PvP scoring (kills/deaths/score) on player-vs-player damage
  - Files:
    - `Assets/Discover/DroneRage/Scripts/Weapons/NetworkedWeaponController.cs` (modify)
  - Changes:
    - Extend `OnDamage(...)` handling to:
      - When `damageableAffected` is a `Player`, update attacker’s PvP stats (`DamageDealtToPlayers`, and if `targetDied`, increment `PvpKills` and `PvpScore`).
      - Update victim’s PvP stats (`DamageTakenFromPlayers`, and if `targetDied`, increment `PvpDeaths`).
    - Ensure scoring is ignored when match state is Ended.
  - Purpose: Produce the scoreboard numbers required by UX.
  - _Leverage: Assets/Discover/DroneRage/Scripts/Weapons/NetworkedWeaponController.cs, Assets/Discover/DroneRage/Scripts/Weapons/IDamageable.cs, Assets/Discover/DroneRage/Scripts/Pvp/DroneRagePvpMatchController.cs_
  - _Requirements: 2, 5, 6_
  - _Prompt: Role: Unity Gameplay Engineer (networked scoring) | Task: Update `NetworkedWeaponController` damage callback handling to track player-vs-player damage and kills into the new PvP fields on `PlayerStats`, and to notify the match controller so it can evaluate win conditions | Restrictions: Do not change existing enemy scoring; do not double-count; ensure logic executes only when match is running; keep authority rules consistent | _Leverage: Assets/Discover/DroneRage/Scripts/Weapons/NetworkedWeaponController.cs, Assets/Discover/DroneRage/Scripts/Pvp/DroneRagePvpMatchController.cs | _Requirements: 2, 5, 6 | Success: PvP hits update DamageDealt/Taken vs players; lethal hits increment kills/deaths and contribute to match end (score limit)

- [x] 8. Implement PvP respawn (no permanent elimination)
  - Files:
    - `Assets/Discover/DroneRage/Scripts/Player/Player.cs` (modify)
    - `Assets/Discover/DroneRage/Scripts/Pvp/DroneRagePvpMatchController.cs` (modify)
  - Changes:
    - Prevent PvP deaths from triggering DroneRage PvE “all players dead => game over” loss.
    - Add an authoritative respawn schedule:
      - On death (match running), decide respawn time using config and send a respawn command to the player’s state authority.
      - On respawn, restore HP to configured value and move player to a deterministic spawn position (ring around origin based on `PlayerId`).
    - Ensure no respawns occur after match ends.
  - Purpose: Make PvP continuous and match-driven.
  - _Leverage: Assets/Discover/DroneRage/Scripts/Player/Player.cs (existing death flow), Assets/Discover/DroneRage/Scripts/Bootstrapper/DroneRageBootstrapper.cs (game over UI behavior)_
  - _Requirements: 3, 4, 5_
  - _Prompt: Role: Unity Gameplay Engineer (respawn + authority) | Task: Implement PvP respawn behavior controlled by the match controller so players respawn after a configurable delay and DroneRage does not end the app on PvP elimination | Restrictions: Do not stop drone waves; do not require scene spawn anchors; do not respawn after match state Ended | _Leverage: Assets/Discover/DroneRage/Scripts/Player/Player.cs, Assets/Discover/DroneRage/Scripts/Pvp/DroneRagePvpMatchController.cs | _Requirements: 3, 4, 5 | Success: In PvP mode, dead players respawn after delay at deterministic positions with restored HP; match continues even if all players die simultaneously

- [x] 9. Make player-to-player damage configurable (replace hard-coded multiplier)
  - Files:
    - `Assets/Discover/DroneRage/Scripts/Weapons/Weapon.cs` (modify)
  - Changes:
    - Replace the hard-coded player damage multiplier `0.2f` with:
      - If PvP active: `pvpDamageEnabled ? friendlyFireMultiplier : 0`.
      - Else (non-PvP): keep `0.2f` to preserve existing behavior.
  - Purpose: Enable/disable PvP damage and tune friendly fire.
  - _Leverage: Assets/Discover/DroneRage/Scripts/Weapons/Weapon.cs, Assets/Discover/DroneRage/Scripts/Pvp/DroneRagePvpMode.cs, Assets/Discover/DroneRage/Scripts/Pvp/DroneRagePvpConfig.cs_
  - _Requirements: 2, 7_
  - _Prompt: Role: Unity Gameplay Engineer (combat tuning) | Task: Update weapon hit resolution so player targets use PvP config for damage multiplier (or 0 when disabled) while keeping the existing non-PvP friendly-fire reduction behavior intact | Restrictions: Do not change damage vs drones; keep hit resolution authoritative (master-only) as it is today; avoid adding new RPCs | _Leverage: Assets/Discover/DroneRage/Scripts/Weapons/Weapon.cs, Assets/Discover/DroneRage/Scripts/Weapons/NetworkedWeaponController.cs, Assets/Discover/DroneRage/Scripts/Pvp/DroneRagePvpConfig.cs | _Requirements: 2, 7 | Success: In PvP mode, player damage follows config; in normal DroneRage, player damage still uses the legacy 0.2 multiplier

- [x] 10. Add a minimal PvP scoreboard UI and hook it to match state
  - Files:
    - `Assets/Discover/DroneRage/Scripts/UI/Pvp/DroneRagePvpScoreboardUI.cs` (new)
  - Unity Operations:
    - Create a prefab `Assets/Discover/DroneRage/UI/Pvp/PvpScoreboard.prefab` with a simple Canvas/TMP layout (no new styling system).
    - Add the `DroneRagePvpScoreboardUI` component and wire required TMP references.
    - Add the prefab as a child of `Assets/Discover/Prefabs/DroneRage/DroneRageNetworkApplication.prefab` (or alternatively under the GameController prefab if that’s where existing DroneRage UI is hosted), and default it to inactive.
  - Behavior:
    - When PvP mode is active, show the scoreboard and populate rows for all players: name (if available), kills, deaths, score.
    - When match state becomes Ended, keep the scoreboard visible as the results screen and stop updating respawn/scoring.
  - Purpose: Satisfy the scoreboard + results requirements without adding new screens.
  - _Leverage: Assets/Discover/DroneRage/Scripts/UI/EndScreen/EndScreenController.cs (TMP + player name pattern), Assets/Discover/DroneRage/Scripts/Player/Player.cs, Assets/Discover/DroneRage/Scripts/Player/PlayerStats.cs_
  - _Requirements: 5, 6_
  - _Prompt: Role: Unity UI Engineer (TMP/MR UI) | Task: Implement a minimal PvP scoreboard UI prefab and controller that binds to networked PvP stats and match state, showing it during PvP and using it as the results display when the match ends | Restrictions: Do not add extra pages/modals; keep UI minimal and readable; do not poll every frame unnecessarily—prefer event/on-change updates where practical | _Leverage: Assets/Discover/DroneRage/Scripts/UI/EndScreen/EndScreenController.cs, Assets/Discover/DroneRage/Scripts/Pvp/DroneRagePvpMatchController.cs, Assets/Discover/DroneRage/Scripts/Player/PlayerStats.cs | _Requirements: 5, 6 | Success: During PvP, all clients see a synchronized scoreboard (kills/deaths/score); on match end the scoreboard remains visible as results

- [x] 11. Attach PvP components to prefabs (GameController + config reference)
  - Unity Operations:
    - Open `Assets/Discover/DroneRage/Prefabs` (or locate the prefab referenced by `DroneRageBootstrapper` as the `m_gameControllerPrefab`).
    - Add `DroneRagePvpMatchController` component to the `DroneRageGameController` prefab.
    - Assign `DroneRagePvpConfig_Default.asset` reference on the match controller.
  - Purpose: Ensure PvP systems exist at runtime and are configured.
  - _Leverage: Assets/Discover/DroneRage/Scripts/Game/DroneRageGameController.cs, Assets/Discover/DroneRage/Scripts/Bootstrapper/DroneRageBootstrapper.cs_
  - _Requirements: 1, 5, 7_
  - _Prompt: Role: Unity Developer (prefabs/wiring) | Task: Wire the match controller and PvP config into the DroneRage prefabs so PvP mode works when launching `dronerage-pvp` | Restrictions: Do not change DroneRage behavior for `dronerage`; ensure components self-disable when PvP is not active | _Leverage: Assets/Discover/DroneRage/Scripts/Bootstrapper/DroneRageBootstrapper.cs, Assets/Discover/Prefabs/DroneRage/DroneRageNetworkApplication.prefab | _Requirements: 1, 5, 7 | Success: Launching `dronerage-pvp` instantiates match controller with config; launching `dronerage` does not activate PvP behavior

- [x] 12. Validation pass (Play Mode checklist)
  - Validation Steps:
    - Launch normal `dronerage`:
      - Verify drones spawn/waves advance as before.
      - Verify player-to-player damage still uses legacy 0.2 multiplier and PvE game-over still works.
    - Launch `dronerage-pvp` as host and join a second client:
      - Verify both players spawn with weapons.
      - Verify player-vs-player hits apply damage per PvP config.
      - Verify kills/deaths/score update on all clients.
      - Verify drones continue spawning.
      - Verify respawn after delay and no respawns after match ended.
      - Verify match ends on score limit or time limit and results remain visible.
  - Purpose: Ensure requirements are met and regressions avoided.
  - _Leverage: Assets/Discover/DroneRage/Scripts/Bootstrapper/DroneRageBootstrapper.cs, Assets/Discover/DroneRage/Scripts/Game/DroneRageGameController.cs_
  - _Requirements: 1, 2, 3, 4, 5, 6, 7_
  - _Prompt: Role: QA Engineer (Unity MR + networking) | Task: Perform a focused Play Mode validation of both standard DroneRage and PvP DroneRage using the checklist, and record any failures with clear repro steps | Restrictions: Do not expand scope; report only issues related to these requirements; do not refactor unrelated systems | _Leverage: Assets/Discover/DroneRage/Scripts/Game/DroneRageGameController.cs, Assets/Discover/DroneRage/Scripts/Player/Player.cs | _Requirements: 1, 2, 3, 4, 5, 6, 7 | Success: All checklist items pass or failures are clearly documented with repro steps and logs
