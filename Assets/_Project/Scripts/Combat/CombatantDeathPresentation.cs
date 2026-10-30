using System.Collections;
using System.Collections.Generic;
using Gamekit3D;
using UnityEngine;

namespace MotionFight.Combat
{
    /// <summary>
    /// Plays the verified original death state. For Chomper, the original death
    /// hit state is followed by the original ragdoll prefab without destroying
    /// the persistent combatant instance required by restart.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-5500)]
    [RequireComponent(typeof(CombatantHealth), typeof(Animator))]
    public sealed class CombatantDeathPresentation : MonoBehaviour
    {
        [Header("Death")]
        [SerializeField] private CombatantHealth combatant;
        [SerializeField] private CombatResolver resolver;
        [SerializeField] private Animator animator;
        [SerializeField] private string deathAnimatorState;
        [SerializeField] private string reviveAnimatorState;
        [SerializeField] private bool holdDeathState = true;

        [Header("Ragdoll")]
        [SerializeField] private bool useRagdoll;
        [SerializeField] private ReplaceWithRagdoll ragdollReplacement;
        [SerializeField, Min(0f)] private float ragdollDelay = 0.46f;

        private readonly List<Renderer> renderers = new List<Renderer>();
        private readonly List<bool> rendererStates = new List<bool>();
        private readonly List<Collider> colliders = new List<Collider>();
        private readonly List<bool> colliderStates = new List<bool>();

        private GameObject ragdollInstance;
        private Coroutine ragdollRoutine;
        private Coroutine deathHoldRoutine;
        private Vector3 lastAttackOrigin;
        private bool hasDeathOrigin;
        private bool subscribed;

        private void Awake()
        {
            if (combatant == null)
                combatant = GetComponent<CombatantHealth>();
            if (animator == null)
                animator = GetComponent<Animator>();
            if (resolver == null)
                resolver = CombatResolver.Instance;

            CachePresentationObjects();
        }

        private void OnEnable()
        {
            combatant.Died += HandleDeath;
            combatant.HealthChanged += HandleHealthChanged;
            SubscribeResolver();
        }

        private void OnDisable()
        {
            combatant.Died -= HandleDeath;
            combatant.HealthChanged -= HandleHealthChanged;
            UnsubscribeResolver();
            if (deathHoldRoutine != null)
            {
                StopCoroutine(deathHoldRoutine);
                deathHoldRoutine = null;
            }
            ClearRagdoll();
        }

        private void HandleDeath(CombatantHealth deadCombatant)
        {
            if (deadCombatant != combatant)
                return;

            if (ragdollRoutine != null)
                StopCoroutine(ragdollRoutine);
            if (deathHoldRoutine != null)
                StopCoroutine(deathHoldRoutine);

            if (animator != null)
                animator.speed = 1f;
            PlayDeathState();

            if (holdDeathState && !string.IsNullOrWhiteSpace(deathAnimatorState))
                deathHoldRoutine = StartCoroutine(HoldDeathState());

            if (useRagdoll && ragdollReplacement != null &&
                ragdollReplacement.ragdollPrefab != null)
            {
                ragdollRoutine = StartCoroutine(SpawnRagdollAfterDelay());
            }
        }

        private void HandleHealthChanged(CombatantHealth health, float previous, float current)
        {
            if (health != combatant || previous > 0f || current <= 0f)
                return;

            ResetDeathPresentation();
        }

        private void PlayDeathState()
        {
            if (animator == null)
                return;

            if (!string.IsNullOrWhiteSpace(deathAnimatorState))
            {
                animator.ResetTrigger("Death");
                animator.Play(deathAnimatorState, 0, 0f);
                animator.Update(0f);
            }
            else
            {
                animator.SetTrigger("Death");
            }
        }

        private IEnumerator SpawnRagdollAfterDelay()
        {
            yield return new WaitForSeconds(ragdollDelay);
            SpawnRagdoll();
            ragdollRoutine = null;
        }

        private void SpawnRagdoll()
        {
            if (ragdollInstance != null || ragdollReplacement == null)
                return;

            ragdollInstance = Instantiate(
                ragdollReplacement.ragdollPrefab,
                transform.position,
                transform.rotation);
            ragdollInstance.name = ragdollReplacement.ragdollPrefab.name + "_CombatDeath";
            ragdollInstance.SetActive(false);

            AlignRagdollHierarchy(transform, ragdollInstance.transform);

            var delayedForce = ragdollInstance.AddComponent<RigidbodyDelayedForce>();
            Vector3 attackDirection = hasDeathOrigin
                ? transform.position - lastAttackOrigin
                : -transform.forward;
            attackDirection.y = 0f;
            delayedForce.forceToAdd =
                attackDirection.normalized * 7f - Physics.gravity * 0.6f;

            ragdollInstance.SetActive(true);
            HideOriginalBody();
        }

        private void AlignRagdollHierarchy(Transform source, Transform target)
        {
            target.SetPositionAndRotation(source.position, source.rotation);
            target.localScale = source.localScale;

            for (int i = 0; i < source.childCount; i++)
            {
                Transform sourceChild = source.GetChild(i);
                Transform targetChild = FindDirectChildByName(target, sourceChild.name);
                if (targetChild != null)
                    AlignRagdollHierarchy(sourceChild, targetChild);
            }
        }

        private static Transform FindDirectChildByName(Transform parent, string childName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == childName)
                    return child;
            }

            return null;
        }

        private void HideOriginalBody()
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null)
                    renderers[i].enabled = false;
            }

            for (int i = 0; i < colliders.Count; i++)
            {
                if (colliders[i] != null)
                    colliders[i].enabled = false;
            }

            if (animator != null)
                animator.enabled = false;
        }

        private void ResetDeathPresentation()
        {
            if (ragdollRoutine != null)
            {
                StopCoroutine(ragdollRoutine);
                ragdollRoutine = null;
            }

            if (deathHoldRoutine != null)
            {
                StopCoroutine(deathHoldRoutine);
                deathHoldRoutine = null;
            }

            ClearRagdoll();
            RestoreOriginalBody();
            hasDeathOrigin = false;
        }

        private void RestoreOriginalBody()
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null)
                    renderers[i].enabled = rendererStates[i];
            }

            for (int i = 0; i < colliders.Count; i++)
            {
                if (colliders[i] != null)
                    colliders[i].enabled = colliderStates[i];
            }

            if (animator != null)
            {
                animator.enabled = true;
                animator.speed = 1f;
                animator.Rebind();
                if (!string.IsNullOrWhiteSpace(reviveAnimatorState))
                    animator.Play(reviveAnimatorState, 0, 0f);
                animator.Update(0f);
                animator.SetBool("Grounded", true);
            }
        }

        private IEnumerator HoldDeathState()
        {
            yield return null;

            while (combatant != null && combatant.IsDead && animator != null)
            {
                AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);
                if (current.IsName(deathAnimatorState))
                {
                    if (!animator.IsInTransition(0) && current.normalizedTime >= 0.99f)
                    {
                        animator.Play(deathAnimatorState, 0, 0.99f);
                        animator.Update(0f);
                        animator.speed = 0f;
                        deathHoldRoutine = null;
                        yield break;
                    }
                }
                else if (!animator.IsInTransition(0))
                {
                    animator.Play(deathAnimatorState, 0, 0f);
                    animator.Update(0f);
                }

                yield return null;
            }

            deathHoldRoutine = null;
        }

        private void ClearRagdoll()
        {
            if (ragdollInstance == null)
                return;

            Destroy(ragdollInstance);
            ragdollInstance = null;
        }

        private void CachePresentationObjects()
        {
            renderers.Clear();
            rendererStates.Clear();
            colliders.Clear();
            colliderStates.Clear();

            Renderer[] foundRenderers = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < foundRenderers.Length; i++)
            {
                renderers.Add(foundRenderers[i]);
                rendererStates.Add(foundRenderers[i].enabled);
            }

            Collider[] foundColliders = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < foundColliders.Length; i++)
            {
                colliders.Add(foundColliders[i]);
                colliderStates.Add(foundColliders[i].enabled);
            }
        }

        private void SubscribeResolver()
        {
            if (subscribed || resolver == null)
                return;

            resolver.HitResolved += HandleHitResolved;
            subscribed = true;
        }

        private void UnsubscribeResolver()
        {
            if (!subscribed || resolver == null)
                return;

            resolver.HitResolved -= HandleHitResolved;
            subscribed = false;
        }

        private void HandleHitResolved(CombatHitResult result)
        {
            if (result.Target != combatant || result.Attacker == null)
                return;

            lastAttackOrigin = result.Attacker.transform.position;
            hasDeathOrigin = true;
        }
    }
}
