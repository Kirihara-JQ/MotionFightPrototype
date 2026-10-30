# Match Controller Design

Date: 2026-10-30

`MatchController` owns Ready, Fighting, Paused, PlayerWin, MonsterWin, and Draw.
It does not implement movement, attack animation, weapon sweeps, or monster
target selection.

During Ready, Pause, and result states it:

- pauses legacy damage resolution;
- releases player input;
- disables the original Chomper behavior and controller;
- updates the HUD state.

During Fighting it:

- enables the original Chomper action components;
- seeds original target detection/pursuit from the 10 m arena spawn;
- gives movement input back to the original `PlayerInput` path;
- enables the player mouse/guard adapter.

HP and death are observed from both `CombatantHealth` components. Win/loss is
decided only after the authoritative HP changes.

Restart reloads `Level1_CircularArena_Combat`, which is the enabled build scene.
This is intentional because the original Chomper ragdoll replacement destroys
its combat scene object after death.

