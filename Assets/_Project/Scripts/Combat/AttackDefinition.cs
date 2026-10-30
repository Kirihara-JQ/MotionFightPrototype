using UnityEngine;

namespace MotionFight.Combat
{
    [CreateAssetMenu(fileName = "AttackDefinition", menuName = "Motion Fight/Combat/Attack Definition")]
    public sealed class AttackDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string attackId = "unassigned";
        [SerializeField] private string displayName = "Unnamed Attack";

        [Header("Combat")]
        [SerializeField, Min(0f)] private float damage = 10f;
        [SerializeField, Min(0f)] private float range = 2.4f;

        [Header("Timing")]
        [SerializeField, Min(0f)] private float startup = 0.1f;
        [SerializeField, Min(0.01f)] private float active = 0.08f;
        [SerializeField, Min(0f)] private float recovery = 0.25f;
        [SerializeField, Min(0f)] private float cooldown = 0.15f;

        [Header("Hitbox")]
        [SerializeField] private Vector3 hitboxLocalOffset = new Vector3(0f, 1f, 1.1f);
        [SerializeField] private Vector3 hitboxSize = new Vector3(0.8f, 0.8f, 1.4f);
        [SerializeField] private LayerMask targetLayers = ~0;

        [Header("Animation Hook")]
        [Tooltip("Optional future Animator hook. Leave empty when no verified clip exists.")]
        [SerializeField] private string animatorTrigger;

        public string AttackId => attackId;
        public string DisplayName => displayName;
        public float Damage => damage;
        public float Range => range;
        public float Startup => startup;
        public float Active => active;
        public float Recovery => recovery;
        public float Cooldown => cooldown;
        public float TotalDuration => startup + active + recovery + cooldown;
        public Vector3 HitboxLocalOffset => hitboxLocalOffset;
        public Vector3 HitboxSize => hitboxSize;
        public LayerMask TargetLayers => targetLayers;
        public string AnimatorTrigger => animatorTrigger;

        public void Configure(
            string id,
            string label,
            float attackDamage,
            float attackRange,
            float startupTime,
            float activeTime,
            float recoveryTime,
            float cooldownTime,
            Vector3 localOffset,
            Vector3 size,
            LayerMask layers,
            string triggerName = "")
        {
            attackId = id;
            displayName = label;
            damage = Mathf.Max(0f, attackDamage);
            range = Mathf.Max(0f, attackRange);
            startup = Mathf.Max(0f, startupTime);
            active = Mathf.Max(0.01f, activeTime);
            recovery = Mathf.Max(0f, recoveryTime);
            cooldown = Mathf.Max(0f, cooldownTime);
            hitboxLocalOffset = localOffset;
            hitboxSize = new Vector3(
                Mathf.Max(0.01f, size.x),
                Mathf.Max(0.01f, size.y),
                Mathf.Max(0.01f, size.z));
            targetLayers = layers;
            animatorTrigger = triggerName;
        }

        private void OnValidate()
        {
            damage = Mathf.Max(0f, damage);
            range = Mathf.Max(0f, range);
            startup = Mathf.Max(0f, startup);
            active = Mathf.Max(0.01f, active);
            recovery = Mathf.Max(0f, recovery);
            cooldown = Mathf.Max(0f, cooldown);
            hitboxSize = new Vector3(
                Mathf.Max(0.01f, hitboxSize.x),
                Mathf.Max(0.01f, hitboxSize.y),
                Mathf.Max(0.01f, hitboxSize.z));
        }
    }
}
