using System;
using UnityEngine;
using UnityEngine.Events;

namespace MotionFight.Combat
{
    /// <summary>
    /// Authoritative health state for a combatant in the motion-fight prototype.
    /// Damage is applied only by CombatResolver.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CombatantHealth : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string displayName;

        [Header("Health")]
        [SerializeField, Min(0.01f)] private float maxHealth = 100f;
        [SerializeField, Min(0f)] private float currentHealth = 100f;
        [SerializeField, Min(0f)] private float postHitInvulnerability;

        [Header("Events")]
        [SerializeField] private UnityEvent onHealthChanged = new UnityEvent();
        [SerializeField] private UnityEvent onDied = new UnityEvent();

        private bool deathRaised;
        private bool deathEventRaised;
        private float invulnerableUntil;

        public event Action<CombatantHealth, float, float> HealthChanged;
        public event Action<CombatantHealth> Died;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float NormalizedHealth => maxHealth <= 0f ? 0f : currentHealth / maxHealth;
        public bool IsDead => deathRaised || currentHealth <= 0f;
        public bool IsInvulnerable => !IsDead && Time.time < invulnerableUntil;
        public float InvulnerabilityRemaining => Mathf.Max(0f, invulnerableUntil - Time.time);
        public float PostHitInvulnerability => postHitInvulnerability;

        private void Awake()
        {
            ResetHealth();
        }

        public void ConfigureMaxHealth(float value, bool refill = true)
        {
            maxHealth = Mathf.Max(0.01f, value);
            currentHealth = refill ? maxHealth : Mathf.Clamp(currentHealth, 0f, maxHealth);
            deathRaised = currentHealth <= 0f;
            deathEventRaised = false;
            invulnerableUntil = 0f;
        }

        public void ConfigurePostHitInvulnerability(float duration)
        {
            postHitInvulnerability = Mathf.Max(0f, duration);
        }

        public void ClearInvulnerability()
        {
            invulnerableUntil = 0f;
        }

        public void ResetHealth()
        {
            float previous = currentHealth;
            currentHealth = maxHealth;
            deathRaised = false;
            deathEventRaised = false;
            invulnerableUntil = 0f;

            if (!Mathf.Approximately(previous, currentHealth))
                RaiseHealthChanged(previous, currentHealth);
        }

        internal float ApplyDamage(float amount)
        {
            return ApplyDamage(amount, false);
        }

        internal float ApplyDamage(float amount, bool deferDeathEvent)
        {
            if (amount <= 0f || IsDead || IsInvulnerable)
                return 0f;

            float previous = currentHealth;
            currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
            float applied = previous - currentHealth;

            if (applied > 0f)
                RaiseHealthChanged(previous, currentHealth);

            if (currentHealth <= 0f && !deathRaised)
            {
                deathRaised = true;
                invulnerableUntil = 0f;
                if (!deferDeathEvent)
                    RaiseDeathEvent();
            }
            else if (applied > 0f && postHitInvulnerability > 0f)
            {
                invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + postHitInvulnerability);
            }

            return applied;
        }

        internal bool RaiseDeferredDeathEvent()
        {
            if (!deathRaised || deathEventRaised)
                return false;

            RaiseDeathEvent();
            return true;
        }

        private void RaiseHealthChanged(float previous, float current)
        {
            onHealthChanged.Invoke();
            HealthChanged?.Invoke(this, previous, current);
        }

        private void RaiseDeathEvent()
        {
            if (deathEventRaised)
                return;

            deathEventRaised = true;
            onDied.Invoke();
            Died?.Invoke(this);
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(displayName))
                displayName = name;
            maxHealth = Mathf.Max(0.01f, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            postHitInvulnerability = Mathf.Max(0f, postHitInvulnerability);
        }
    }
}
