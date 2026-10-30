# Monster Action Authority

Date: 2026-10-30

The formal combat scene no longer contains `ArenaMonsterAI` or a replacement
custom NavMesh pursuit state machine.

Chomper actions are owned by the original modules:

- `ChomperBehavior`
- `EnemyController`
- `NavMeshAgent`
- original `Chomper.controller`
- original `ChomperSMBAttack`, pursuit, idle, and hit behaviours

`ChomperSMBAttack` disables NavMesh following for the attack, then lets the
original attack clip's root motion move the monster forward. It re-enables
following when the attack state exits.

The match layer only:

- enables these components during `Fighting`;
- disables them during Ready/Pause/result;
- forwards successful original damage events to `CombatResolver`;
- stops the gameplay result after authoritative HP reaches zero.

Chomper damage enters through the original `AttackRoot/MeleeWeapon`.
`AttackRoot/MeleeWeapon.damage` is configured to `12`.

Continuous body contact damage was disabled for the 1v1 prototype; the original
attack animation and weapon action are kept as the monster's intentional attack.

The active Chomper is the original scene instance at
`Enemies/Chomper`. The other 25 Chompers and Grenadier remain excluded by the
2A arena isolation list.
