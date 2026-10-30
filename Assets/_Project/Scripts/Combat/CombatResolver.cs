using System;
using System.Collections.Generic;
using UnityEngine;

namespace MotionFight.Combat
{
    public struct CombatHitResult
    {
        public CombatantHealth Attacker;
        public CombatantHealth Target;
        public string AttackName;
        public float RawDamage;
        public float AppliedDamage;
        public bool Blocked;
        public bool KilledTarget;
    }

    /// <summary>
    /// Authoritative HP gateway for the original 3D Game Kit damage path.
    /// Original Movement/Animator/MeleeWeapon actions stay unchanged; their
    /// accepted Damageable messages are forwarded here and never own new HP.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-9000)]
    public sealed class CombatResolver : MonoBehaviour
    {
        [SerializeField] private List<CombatantHealth> combatants = new List<CombatantHealth>();
        [SerializeField] private bool logResolvedHits;

        public static CombatResolver Instance { get; private set; }
        public bool IsPaused { get; private set; }

        public event Action<CombatHitResult> HitResolved;
        public event Action<CombatantHealth> CombatantDied;

        public IReadOnlyList<CombatantHealth> Combatants => combatants;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("Only one CombatResolver may be active.", this);
                enabled = false;
                return;
            }

            Instance = this;
            for (int i = 0; i < combatants.Count; i++)
                RegisterCombatant(combatants[i]);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            for (int i = 0; i < combatants.Count; i++)
            {
                if (combatants[i] != null)
                    combatants[i].Died -= HandleCombatantDied;
            }
        }

        public void RegisterCombatant(CombatantHealth combatant)
        {
            if (combatant == null || combatants.Contains(combatant))
                return;

            combatants.Add(combatant);
            combatant.Died += HandleCombatantDied;
        }

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
        }

        public void CancelAllAttacks()
        {
            // Original MeleeWeapon windows are animation-event driven. Match gating
            // prevents their damage from resolving after a pause or result.
        }

        public CombatHitResult? ResolveLegacyDamage(
            CombatantHealth attacker,
            CombatantHealth target,
            float rawDamage,
            Vector3 attackOrigin,
            string attackName)
        {
            if (IsPaused || attacker == null || target == null ||
                attacker == target || attacker.IsDead || target.IsDead ||
                rawDamage <= 0f || !combatants.Contains(attacker) ||
                !combatants.Contains(target))
            {
                return null;
            }

            CombatGuard guard = target.GetComponent<CombatGuard>();
            bool blocked = guard != null && guard.CanBlockFrom(attackOrigin);
            float multiplier = blocked ? guard.DamageMultiplierWhenGuarding : 1f;
            float appliedDamage = target.ApplyDamage(rawDamage * multiplier);

            var result = new CombatHitResult
            {
                Attacker = attacker,
                Target = target,
                AttackName = string.IsNullOrWhiteSpace(attackName) ? "legacy-melee" : attackName,
                RawDamage = rawDamage,
                AppliedDamage = appliedDamage,
                Blocked = blocked,
                KilledTarget = target.IsDead
            };

            HitResolved?.Invoke(result);
            if (logResolvedHits)
            {
                Debug.Log(
                    result.AttackName + " hit " + target.DisplayName + " for " +
                    appliedDamage + (blocked ? " (blocked)" : string.Empty),
                    this);
            }

            return result;
        }

        private void HandleCombatantDied(CombatantHealth combatant)
        {
            CombatantDied?.Invoke(combatant);
        }
    }
}
