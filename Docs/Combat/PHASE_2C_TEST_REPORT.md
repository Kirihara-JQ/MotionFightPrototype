# Original Action Combat Test Report

Date: 2026-10-30
Unity: `6000.3.27f1`
Scene: `Assets/Scenes/Combat/Level1_CircularArena_Combat.unity`

## Architecture Baseline

- 2A completion commit: `e6830de`
- Current `main`: reset to `e6830de`
- Pre-rollback custom architecture backup:
  `backup/combat-architecture-before-original-actions`
- Backup checkpoint commit: `86d247f`

## Verified

- Original Ellen movement remains on `PlayerController` and
  `CharacterController`.
- Original Chomper pursuit uses `ChomperBehavior`, `EnemyController`,
  `NavMeshAgent`, and Animator Root Motion.
- Original Chomper attack animation and forward dash are preserved.
- Original Ellen Hurt clips were observed through the Animator:
  `Ellen_HitFront`, `Ellen_HitFrontRight`, `Ellen_HitLeft`, `Ellen_HitRight`,
  and `Ellen_HitBack`.
- Ellen invulnerability flicker toggled at approximately `0.3 s` intervals.
- Ellen post-hit invulnerability is configured to `1.5 s`.
- Ellen death plays the original `Ellen_Death` Animator state and holds its
  final frame.
- When Ellen dies, living Chomper exits attack behavior and is held in
  `Chomper_Idle`.
- Left mouse attack applied 10 HP damage through the original melee animation
  and weapon chain.
- Right mouse attack applied 12 HP damage through the same original action path.
- New `CombatantHealth` values changed while the legacy Damageable pool remained
  at its inert large value.
- `HealthUI` is disabled in the scene and `MatchHUD` is active.
- Original Chomper attack Root Motion remains owned by the original attack state.
- Windows Standalone build job `build-8156cb60da` passed with 0 errors,
  1 existing warning, and total size 1,716.71 MB.

## Tests Not Yet Completed

- Full two-sided death/draw round has not been rerun after this architecture
  rewrite.
- Built Player smoke test has not been run.

These items must remain untested rather than being reported as passed.
