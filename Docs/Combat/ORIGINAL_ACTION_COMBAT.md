# Original Action Combat Integration

Date: 2026-10-30
Unity: `6000.3.27f1`
Scene: `Assets/Scenes/Combat/Level1_CircularArena_Combat.unity`

## Architecture

The action architecture starts from the completed 2A project (`e6830de`) and
keeps the original 3D Game Kit action modules:

- Ellen: `PlayerController`, `PlayerInput`, `Animator`, `MeleeWeapon`
- Chomper: `ChomperBehavior`, `EnemyController`, `NavMeshAgent`, `Animator`
- Original Animator Controllers and animation clips

No custom monster AI, custom monster steering, or custom player attack Hitbox
drives the actions.

New code is limited to:

- authoritative HP (`CombatantHealth`);
- guard/damage authorization (`CombatResolver`, `CombatGuard`);
- a Damageable event bridge;
- player mouse-button intent driving the original melee animation;
- match state and HUD.

## Damage Path

```text
Original animation event
  -> original MeleeWeapon / ContactDamager
  -> original Damageable hit-angle and invulnerability check
  -> LegacyDamageBridge
  -> CombatResolver
  -> CombatantHealth.ApplyDamage
  -> HP/death events
  -> MatchController / MatchHUD
```

The original `Damageable` objects are retained only as accepted-hit event
carriers. Their legacy HP pools are assigned a very large runtime value and are
never used for display, death, or win/loss. `CombatantHealth` is the only HP
authority.

The original player controller remains registered as a Damageable receiver, so
Ellen's original hurt animation, hurt audio, camera shake, and hit direction
logic remain intact.

## Player Attack Module

`PlayerCombatInputAdapter` maps:

| Input | Attack |
|---|---|
| Left mouse | `LeftStraight.asset`, 10 damage |
| Right mouse | `RightStraight.asset`, 12 damage |
| Left Shift | guard |

The adapter does not perform its own overlap test. It sets the original Staff
`MeleeWeapon.damage` for that action and starts the original `MeleeAttack`
Animator transition. Damage timing, weapon sweep, animation events, sounds, and
combo behavior remain from the original module.

The public `RequestAttack` / `RequestGuard` methods remain the future
motion-capture command boundary.

## Monster Actions

The original Chomper action chain remains:

```text
ChomperBehavior
  -> ChomperSMBIdle / Pursuit / Attack / Hit
  -> EnemyController
  -> NavMeshAgent + Animator Root Motion
```

This retains pursuit, rotation, attack wind-up, Root Motion forward dash,
recovery, hit reaction, death reaction, and ragdoll replacement.

`MatchController` only gates the original components while Ready, Paused, or in
a result state. It does not replace their action decisions.

## Death and Restart

Ellen's scene Animator uses
`Assets/_Project/Animations/EllenCombat.controller`, a copy of the original
controller with only the obsolete `EllenDeath -> BeginResapwn` transition
removed. All normal movement, attack, and hurt states remain original.

Chomper keeps its original Controller, including `ReplaceWithRagdollSMB`, so
martial hit/death animation and ragdoll replacement remain original.

Restart reloads the active formal combat scene. This allows the original Chomper
ragdoll replacement to complete without preserving invalid scene references.

`CombatantDeathPresentation` directly plays the verified original
`EllenDeath` Animator state when Ellen's authoritative HP reaches zero. When
the clip reaches its final frame, Animator speed is set to zero and the state
remains `EllenDeath`.

When Ellen dies while Chomper is alive, `MatchController` stops the original
Chomper behavior and navigation and resets its Animator to the `ChomperIdle`
state. The idle state is then held.

## Invulnerability

Ellen's post-hit invulnerability is `1.5 s` in all three participating layers:

- `CombatantHealth.postHitInvulnerability`;
- the original Damageable invulnerability window;
- `LegacyDamageBridge.legacyInvulnerabilityTime`.

Renderer flicker remains `0.3 s` per visibility toggle.

## Scene Setup

- `Ellen`: `CombatantHealth`, `CombatGuard`, `LegacyDamageBridge`,
  `PlayerCombatInputAdapter`, `PlayerCombatReaction`,
  `CombatantDeathPresentation`.
- `Enemies/Chomper`: `CombatantHealth`, `CombatGuard`, `LegacyDamageBridge`,
  `CombatantDeathPresentation`.
- `__CombatOverlay/CombatSystems`: `CombatResolver`, `MatchController`.
- `HealthCanvas`: `MatchHUD`; legacy `HealthUI` disabled.
