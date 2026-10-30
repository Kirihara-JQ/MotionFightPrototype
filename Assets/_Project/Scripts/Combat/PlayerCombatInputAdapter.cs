using Gamekit3D;
using UnityEngine;

namespace MotionFight.Combat
{
    /// <summary>
    /// Keyboard/mouse intent adapter. It starts the original Ellen melee animation
    /// and temporarily applies the selected attack damage to the original weapon.
    /// Future motion-capture input can call the same RequestAttack/RequestGuard API.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatantHealth), typeof(CombatGuard), typeof(Animator))]
    public sealed class PlayerCombatInputAdapter : MonoBehaviour
    {
        [SerializeField] private CombatResolver resolver;
        [SerializeField] private CombatantHealth combatant;
        [SerializeField] private CombatGuard guard;
        [SerializeField] private Animator animator;
        [SerializeField] private MeleeWeapon meleeWeapon;
        [SerializeField] private AttackDefinition leftStraight;
        [SerializeField] private AttackDefinition rightStraight;

        private bool acceptsCombatInput = true;
        private bool lastGuardRequest;
        private AttackDefinition pendingAttack;
        private float nextAttackTime;

        public CombatantHealth Combatant => combatant;
        public AttackDefinition LeftStraight => leftStraight;
        public AttackDefinition RightStraight => rightStraight;

        private void Awake()
        {
            if (resolver == null)
                resolver = CombatResolver.Instance;
            if (combatant == null)
                combatant = GetComponent<CombatantHealth>();
            if (guard == null)
                guard = GetComponent<CombatGuard>();
            if (animator == null)
                animator = GetComponent<Animator>();

            if (meleeWeapon == null)
            {
                PlayerController controller = GetComponent<PlayerController>();
                if (controller != null)
                    meleeWeapon = controller.meleeWeapon;
            }
        }

        private void Update()
        {
            if (!acceptsCombatInput || resolver == null || combatant == null || combatant.IsDead)
                return;

            if (Input.GetMouseButtonDown(0))
                RequestAttack(leftStraight);
            if (Input.GetMouseButtonDown(1))
                RequestAttack(rightStraight);

            bool guardRequested = Input.GetKey(KeyCode.LeftShift);
            if (guardRequested != lastGuardRequest)
            {
                lastGuardRequest = guardRequested;
                RequestGuard(guardRequested);
            }
        }

        private void FixedUpdate()
        {
            if (pendingAttack == null || animator == null)
                return;

            if (meleeWeapon != null)
                meleeWeapon.damage = Mathf.Max(1, Mathf.RoundToInt(pendingAttack.Damage));

            animator.SetTrigger("MeleeAttack");
            pendingAttack = null;
        }

        public bool RequestAttack(AttackDefinition attack)
        {
            if (!acceptsCombatInput || attack == null || pendingAttack != null ||
                guard != null && guard.IsGuarding || Time.time < nextAttackTime ||
                resolver == null || resolver.IsPaused ||
                combatant == null || combatant.IsDead)
            {
                return false;
            }

            pendingAttack = attack;
            nextAttackTime = Time.time + Mathf.Max(0.1f, attack.TotalDuration);
            return true;
        }

        public void RequestGuard(bool guarding)
        {
            if (guard != null)
            {
                guard.SetGuarding(
                    guarding && acceptsCombatInput &&
                    resolver != null && !resolver.IsPaused &&
                    combatant != null && !combatant.IsDead);
            }
        }

        public void SetCombatInputEnabled(bool enabled)
        {
            acceptsCombatInput = enabled;
            pendingAttack = null;
            lastGuardRequest = false;
            RequestGuard(false);
        }

        private void OnDisable()
        {
            lastGuardRequest = false;
            pendingAttack = null;
            RequestGuard(false);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                lastGuardRequest = false;
                pendingAttack = null;
                RequestGuard(false);
            }
        }
    }
}
