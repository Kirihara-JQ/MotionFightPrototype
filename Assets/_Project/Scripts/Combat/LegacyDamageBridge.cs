using Gamekit3D;
using Gamekit3D.Message;
using UnityEngine;

namespace MotionFight.Combat
{
    /// <summary>
    /// Keeps the original Damageable event flow for hit reactions, while routing
    /// accepted damage into CombatantHealth through CombatResolver.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-6000)]
    [RequireComponent(typeof(CombatantHealth), typeof(Damageable))]
    public sealed class LegacyDamageBridge : MonoBehaviour, IMessageReceiver
    {
        [SerializeField] private CombatantHealth combatant;
        [SerializeField] private CombatResolver resolver;
        [SerializeField] private Damageable legacyDamageable;
        [SerializeField] private bool removePlayerControllerReceiver;
        [SerializeField, Min(0f)] private float legacyInvulnerabilityTime = 0.7f;

        private const int LegacyPoolHealth = 100000000;
        private bool registered;

        public Damageable LegacyDamageable => legacyDamageable;

        private void Awake()
        {
            if (combatant == null)
                combatant = GetComponent<CombatantHealth>();
            if (legacyDamageable == null)
                legacyDamageable = GetComponent<Damageable>();
            if (resolver == null)
                resolver = CombatResolver.Instance;
        }

        private void OnEnable()
        {
            RegisterReceiver();
        }

        private void Start()
        {
            if (resolver == null)
                resolver = CombatResolver.Instance;

            RegisterReceiver();
            ResetLegacyState();

            if (removePlayerControllerReceiver && legacyDamageable != null)
            {
                for (int i = legacyDamageable.onDamageMessageReceivers.Count - 1; i >= 0; i--)
                {
                    if (legacyDamageable.onDamageMessageReceivers[i] is PlayerController)
                        legacyDamageable.onDamageMessageReceivers.RemoveAt(i);
                }
            }
        }

        private void OnDisable()
        {
            if (registered && legacyDamageable != null)
                legacyDamageable.onDamageMessageReceivers.Remove(this);
            registered = false;
        }

        public void ResetLegacyState()
        {
            if (legacyDamageable == null)
                return;

            legacyDamageable.maxHitPoints = LegacyPoolHealth;
            legacyDamageable.invulnerabiltyTime = legacyInvulnerabilityTime;
            legacyDamageable.ResetDamage();
            legacyDamageable.isInvulnerable = false;
        }

        public void OnReceiveMessage(MessageType type, object sender, object data)
        {
            if (type != MessageType.DAMAGED && type != MessageType.DEAD)
                return;
            if (resolver == null || combatant == null)
                return;

            var message = (Damageable.DamageMessage)data;
            CombatantHealth attacker = message.damager != null
                ? message.damager.GetComponentInParent<CombatantHealth>()
                : null;

            resolver.ResolveLegacyDamage(
                attacker,
                combatant,
                message.amount,
                message.damageSource,
                message.damager != null ? message.damager.name : "legacy-damage");
        }

        private void RegisterReceiver()
        {
            if (registered || legacyDamageable == null)
                return;

            if (!legacyDamageable.onDamageMessageReceivers.Contains(this))
                legacyDamageable.onDamageMessageReceivers.Add(this);
            registered = true;
        }
    }
}
