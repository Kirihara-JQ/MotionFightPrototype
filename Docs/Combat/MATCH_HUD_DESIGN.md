# Match HUD Design

Date: 2026-10-30

The existing `HealthCanvas`, CanvasScaler, GraphicRaycaster, and TextMesh Pro
support are reused.

Legacy `HealthUI` is disabled on the scene instance. `MatchHUD` presents:

- Ellen and Chomper current/max HP;
- both health bars;
- Ready/Fighting/Paused/result state;
- original Chomper action state derived from its Animator;
- pause, resume, and restart controls.

The HUD never modifies HP, decides a winner, or starts attacks.

Ellen invulnerability flicker is presentation-only in
`PlayerCombatReaction`. It toggles the character renderers every `0.3 s` while
`CombatantHealth.IsInvulnerable` is true, then restores their original enabled
states. The invulnerability duration is `1.5 s`.

