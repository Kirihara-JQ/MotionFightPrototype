# Original Damageable Bridge

Date: 2026-10-30

The original `Damageable` components remain enabled because they carry the
original animation-event and weapon hit flow.

They no longer own gameplay HP:

- `LegacyDamageBridge` registers itself as a Damageable receiver;
- the bridge forwards accepted DamageMessage data to `CombatResolver`;
- `CombatResolver` applies only `CombatantHealth.ApplyDamage`;
- the legacy Damageable pool is set to a very large runtime value;
- UI, death, and win/loss read only `CombatantHealth`.

Ellen keeps the original `PlayerController` Damageable receiver so the original
Hurt animation, direction, audio, and camera feedback continue to run. The new
reaction component only adds invulnerability renderer flicker.

Chomper keeps its original `ChomperBehavior` receiver so original hit push,
Animator hit reactions, sounds, and death/ragdoll behavior continue.

`BodyDamager` is disabled in this arena scene to avoid unintended continuous
contact damage; `AttackRoot/MeleeWeapon` remains the intentional attack path.
