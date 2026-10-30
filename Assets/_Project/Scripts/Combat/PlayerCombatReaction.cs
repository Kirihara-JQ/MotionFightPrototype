using System.Collections.Generic;
using UnityEngine;

namespace MotionFight.Combat
{
    /// <summary>
    /// Connects authoritative CombatResolver hits to Ellen's verified Hurt Animator
    /// parameters. Post-hit invulnerability remains owned by CombatantHealth.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(1000)]
    [RequireComponent(typeof(CombatantHealth), typeof(Animator))]
    public sealed class PlayerCombatReaction : MonoBehaviour
    {
        [SerializeField] private CombatResolver resolver;
        [SerializeField] private CombatantHealth combatant;
        [SerializeField] private Animator animator;
        [SerializeField] private string hurtAnimatorState = "Hurt";
        [SerializeField] private bool playHurtAnimation;
        [SerializeField] private bool enableInvulnerabilityFlicker = true;
        [SerializeField, Min(0.01f)] private float invulnerabilityFlickerInterval = 0.3f;

        private bool subscribed;
        private readonly List<Renderer> flickerRenderers = new List<Renderer>();
        private readonly List<bool> defaultRendererStates = new List<bool>();
        private float flickerTimer;
        private bool flickerVisible = true;
        private bool flickerInitialized;

        private void Awake()
        {
            if (resolver == null)
                resolver = CombatResolver.Instance;
            if (combatant == null)
                combatant = GetComponent<CombatantHealth>();
            if (animator == null)
                animator = GetComponent<Animator>();

            CacheFlickerRenderers();
        }

        private void Update()
        {
            if (!enableInvulnerabilityFlicker || !flickerInitialized)
                return;

            if (combatant != null && combatant.IsInvulnerable)
            {
                flickerTimer += Time.deltaTime;
                if (flickerTimer >= invulnerabilityFlickerInterval)
                {
                    flickerTimer = 0f;
                    flickerVisible = !flickerVisible;
                }

                // Keep the requested visibility authoritative through the frame;
                // animation and character setup components must not re-enable it.
                ApplyFlickerState();
            }
            else if (!flickerVisible)
            {
                flickerVisible = true;
                flickerTimer = 0f;
                RestoreRendererStates();
            }
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            RestoreRendererStates();
        }

        private void Subscribe()
        {
            if (subscribed || resolver == null)
                return;

            resolver.HitResolved += HandleHitResolved;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || resolver == null)
                return;

            resolver.HitResolved -= HandleHitResolved;
            subscribed = false;
        }

        private void HandleHitResolved(CombatHitResult result)
        {
            if (combatant == null || animator == null ||
                result.Target != combatant || result.AppliedDamage <= 0f)
                return;

            if (playHurtAnimation)
            {
                Vector3 attackDirection = result.Attacker != null
                    ? result.Attacker.transform.position - transform.position
                    : transform.forward;
                attackDirection.y = 0f;
                Vector3 localDirection = transform.InverseTransformDirection(attackDirection);

                animator.SetFloat("HurtFromX", localDirection.x);
                animator.SetFloat("HurtFromY", localDirection.z);
                if (!string.IsNullOrWhiteSpace(hurtAnimatorState))
                {
                    animator.ResetTrigger("Hurt");
                    animator.Play(hurtAnimatorState, 0, 0f);
                    animator.Update(0f);
                }
                else
                {
                    animator.SetTrigger("Hurt");
                }
            }

            flickerTimer = 0f;
            flickerVisible = true;
            ApplyFlickerState();
        }

        private void CacheFlickerRenderers()
        {
            flickerRenderers.Clear();
            defaultRendererStates.Clear();

            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || IsExcludedEffectRenderer(renderer))
                    continue;

                flickerRenderers.Add(renderer);
                defaultRendererStates.Add(renderer.enabled);
            }

            flickerInitialized = flickerRenderers.Count > 0;
        }

        private void ApplyFlickerState()
        {
            for (int i = 0; i < flickerRenderers.Count; i++)
            {
                Renderer renderer = flickerRenderers[i];
                if (renderer == null)
                    continue;

                bool originallyVisible = defaultRendererStates[i];
                renderer.enabled = originallyVisible && flickerVisible;
            }
        }

        private void RestoreRendererStates()
        {
            for (int i = 0; i < flickerRenderers.Count; i++)
            {
                Renderer renderer = flickerRenderers[i];
                if (renderer != null)
                    renderer.enabled = defaultRendererStates[i];
            }
        }

        private static bool IsExcludedEffectRenderer(Renderer renderer)
        {
            string rendererName = renderer.transform.name.ToLowerInvariant();
            return rendererName.Contains("trail") ||
                   rendererName.Contains("staff") ||
                   rendererName.Contains("pistol") ||
                   rendererName.Contains("shield");
        }
    }
}
