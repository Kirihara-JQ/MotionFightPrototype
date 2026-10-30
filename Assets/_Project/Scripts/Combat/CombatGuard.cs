using System;
using UnityEngine;

namespace MotionFight.Combat
{
    /// <summary>
    /// Tracks the current guard stance and resolves only frontal blocks.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatantHealth))]
    public sealed class CombatGuard : MonoBehaviour
    {
        [SerializeField, Range(0f, 360f)] private float guardAngle = 120f;
        [SerializeField, Range(0f, 1f)] private float damageMultiplierWhenGuarding = 0.1f;

        private CombatantHealth health;

        public event Action<CombatGuard, bool> GuardStateChanged;

        public bool IsGuarding { get; private set; }
        public float GuardAngle => guardAngle;
        public float DamageMultiplierWhenGuarding => damageMultiplierWhenGuarding;

        private void Awake()
        {
            health = GetComponent<CombatantHealth>();
            health.Died += HandleDeath;
        }

        private void OnDestroy()
        {
            if (health != null)
                health.Died -= HandleDeath;
        }

        public void Configure(float angle, float damageMultiplier)
        {
            guardAngle = Mathf.Clamp(angle, 0f, 360f);
            damageMultiplierWhenGuarding = Mathf.Clamp01(damageMultiplier);
        }

        public void ConfigureOwner(CombatantHealth combatant)
        {
            if (health != null)
                health.Died -= HandleDeath;

            health = combatant;
            if (health != null)
                health.Died += HandleDeath;
        }

        public void SetGuarding(bool guarding)
        {
            bool next = guarding && health != null && !health.IsDead;
            if (IsGuarding == next)
                return;

            IsGuarding = next;
            GuardStateChanged?.Invoke(this, IsGuarding);
        }

        public bool CanBlockFrom(Vector3 attackOrigin)
        {
            if (!IsGuarding || health == null || health.IsDead)
                return false;

            Vector3 toAttacker = attackOrigin - transform.position;
            toAttacker.y = 0f;
            if (toAttacker.sqrMagnitude < 0.0001f)
                return true;

            float halfAngle = guardAngle * 0.5f;
            return Vector3.Angle(transform.forward, toAttacker) <= halfAngle;
        }

        private void HandleDeath(CombatantHealth deadHealth)
        {
            SetGuarding(false);
        }

        private void OnDisable()
        {
            IsGuarding = false;
        }

        private void OnValidate()
        {
            guardAngle = Mathf.Clamp(guardAngle, 0f, 360f);
            damageMultiplierWhenGuarding = Mathf.Clamp01(damageMultiplierWhenGuarding);
        }
    }
}
