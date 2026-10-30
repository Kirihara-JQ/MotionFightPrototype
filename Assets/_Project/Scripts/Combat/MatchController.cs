using System;
using System.Collections;
using Gamekit3D;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace MotionFight.Combat
{
    public enum MatchState
    {
        Ready,
        Fighting,
        Paused,
        PlayerWin,
        MonsterWin,
        Draw
    }

    /// <summary>
    /// Sole authority for match phase, pause/resume, restart, and winner selection.
    /// Combatants own HP; CombatResolver owns all damage and attack windows.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-7000)]
    public sealed class MatchController : MonoBehaviour
    {
        [Header("Participants")]
        [SerializeField] private CombatResolver resolver;
        [SerializeField] private CombatantHealth player;
        [SerializeField] private CombatantHealth monster;
        [SerializeField] private PlayerCombatInputAdapter playerCombatInput;
        [SerializeField] private PlayerInput legacyPlayerInput;
        [SerializeField] private ChomperBehavior monsterBehavior;
        [SerializeField] private EnemyController monsterController;
        [SerializeField] private LegacyDamageBridge playerDamageBridge;
        [SerializeField] private LegacyDamageBridge monsterDamageBridge;

        [Header("Round")]
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private Transform monsterSpawn;
        [SerializeField, Min(0.1f)] private float readyDuration = 1.25f;
        [SerializeField] private bool autoStart = true;
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private KeyCode restartKey = KeyCode.R;

        private bool healthEventsBound;
        private Coroutine readyRoutine;
        private Coroutine monsterPrimeRoutine;

        public static MatchController Instance { get; private set; }

        public event Action<MatchState, MatchState> StateChanged;
        public event Action<CombatantHealth, float, float> ParticipantHealthChanged;

        public MatchState State { get; private set; } = MatchState.Ready;
        public bool IsFighting => State == MatchState.Fighting;
        public bool IsPaused => State == MatchState.Paused;
        public bool HasEnded => State == MatchState.PlayerWin ||
                                State == MatchState.MonsterWin ||
                                State == MatchState.Draw;

        public CombatantHealth PlayerCombatant => player;
        public CombatantHealth MonsterCombatant => monster;
        public CombatResolver Resolver => resolver;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("Only one MatchController may be active.", this);
                enabled = false;
                return;
            }

            Instance = this;
            if (resolver == null)
                resolver = CombatResolver.Instance;
            if (playerCombatInput == null && player != null)
                playerCombatInput = player.GetComponent<PlayerCombatInputAdapter>();
            if (legacyPlayerInput == null && player != null)
                legacyPlayerInput = player.GetComponent<PlayerInput>();
            if (monsterBehavior == null && monster != null)
                monsterBehavior = monster.GetComponent<ChomperBehavior>();
            if (monsterController == null && monster != null)
                monsterController = monster.GetComponent<EnemyController>();
            if (player != null)
            {
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                    playerController.SetCanAttack(false);
            }

            BindHealthEvents();
            SetInteractionEnabled(false);
            SetMonsterActionsEnabled(false);
            if (resolver != null)
                resolver.SetPaused(true);
        }

        private void Start()
        {
            ResetRound();
            if (autoStart)
                readyRoutine = StartCoroutine(BeginAfterReady());
        }

        private void Update()
        {
            if (Input.GetKeyDown(pauseKey))
                TogglePause();

            if (Input.GetKeyDown(restartKey))
                RestartMatch();
        }

        private void OnDestroy()
        {
            UnbindHealthEvents();
            if (Instance == this)
                Instance = null;
        }

        public void BeginFighting()
        {
            if (State != MatchState.Ready || player == null || monster == null || resolver == null)
                return;

            if (player.IsDead || monster.IsDead)
                EvaluateOutcome();

            if (State != MatchState.Ready)
                return;

            resolver.SetPaused(false);
            SetInteractionEnabled(true);
            SetMonsterActionsEnabled(true);
            SetState(MatchState.Fighting);
        }

        public void PauseMatch()
        {
            if (State != MatchState.Fighting)
                return;

            SetInteractionEnabled(false);
            SetMonsterActionsEnabled(false);
            if (resolver != null)
                resolver.SetPaused(true);

            SetState(MatchState.Paused);
        }

        public void ResumeMatch()
        {
            if (State != MatchState.Paused || player == null || monster == null || resolver == null)
                return;
            if (player.IsDead || monster.IsDead)
            {
                EvaluateOutcome();
                return;
            }

            resolver.SetPaused(false);
            SetInteractionEnabled(true);
            SetMonsterActionsEnabled(true);
            SetState(MatchState.Fighting);
        }

        public void TogglePause()
        {
            if (State == MatchState.Fighting)
                PauseMatch();
            else if (State == MatchState.Paused)
                ResumeMatch();
        }

        public void RestartMatch()
        {
            if (!isActiveAndEnabled)
                return;

            Scene activeScene = gameObject.scene;
            if (activeScene.IsValid() && !string.IsNullOrWhiteSpace(activeScene.name))
                SceneManager.LoadScene(activeScene.name);
        }

        private void ResetRound()
        {
            if (resolver != null)
            {
                resolver.CancelAllAttacks();
                resolver.SetPaused(true);
            }

            SetInteractionEnabled(false);

            if (player != null)
            {
                PlacePlayerAtSpawn();
                player.ResetHealth();
            }

            if (monster != null)
            {
                monster.ResetHealth();
                PlaceMonsterAtSpawn();
            }

            if (playerDamageBridge != null)
                playerDamageBridge.ResetLegacyState();
            if (monsterDamageBridge != null)
                monsterDamageBridge.ResetLegacyState();

            SetMonsterActionsEnabled(false);

            SetState(MatchState.Ready);
        }

        private IEnumerator BeginAfterReady()
        {
            yield return new WaitForSecondsRealtime(readyDuration);
            readyRoutine = null;
            if (State == MatchState.Ready && isActiveAndEnabled)
                BeginFighting();
        }

        private void PlacePlayerAtSpawn()
        {
            if (player == null || playerSpawn == null)
                return;

            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
                controller.enabled = false;

            player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);

            if (controller != null)
                controller.enabled = true;
        }

        private void PlaceMonsterAtSpawn()
        {
            if (monster == null || monsterSpawn == null)
                return;

            NavMeshAgent agent = monster.GetComponent<NavMeshAgent>();
            if (agent != null)
                agent.enabled = false;

            monster.transform.SetPositionAndRotation(monsterSpawn.position, monsterSpawn.rotation);

            if (agent != null)
                agent.enabled = true;
        }

        private void HandleHealthChanged(CombatantHealth participant, float previous, float current)
        {
            ParticipantHealthChanged?.Invoke(participant, previous, current);
        }

        private void HandleDied(CombatantHealth deadParticipant)
        {
            if (HasEnded || deadParticipant == null)
                return;

            EvaluateOutcome();
        }

        private void EvaluateOutcome()
        {
            if (player == null || monster == null)
                return;

            bool playerDead = player.IsDead;
            bool monsterDead = monster.IsDead;
            if (!playerDead && !monsterDead)
                return;

            SetInteractionEnabled(false);
            SetMonsterActionsEnabled(false);
            if (resolver != null)
            {
                resolver.SetPaused(true);
                resolver.CancelAllAttacks();
            }
            SetState(DetermineOutcome(
                player.CurrentHealth,
                monster.CurrentHealth));

            if (playerDead && !monsterDead)
                ReleaseMonsterAttackState();
        }

        public static MatchState DetermineOutcome(float playerHealth, float monsterHealth)
        {
            bool playerDead = playerHealth <= 0f;
            bool monsterDead = monsterHealth <= 0f;

            if (playerDead && monsterDead)
                return MatchState.Draw;
            if (monsterDead)
                return MatchState.PlayerWin;
            if (playerDead)
                return MatchState.MonsterWin;

            return MatchState.Fighting;
        }

        private void SetInteractionEnabled(bool enabled)
        {
            if (playerCombatInput != null)
                playerCombatInput.SetCombatInputEnabled(enabled);

            if (legacyPlayerInput == null)
                return;

            if (enabled)
                legacyPlayerInput.GainControl();
            else
                legacyPlayerInput.ReleaseControl();
        }

        private void SetMonsterActionsEnabled(bool enabled)
        {
            if (monsterController != null)
                monsterController.enabled = enabled;
            if (monsterBehavior != null)
            {
                monsterBehavior.enabled = enabled;
                if (enabled && isActiveAndEnabled)
                {
                    if (monsterPrimeRoutine != null)
                        StopCoroutine(monsterPrimeRoutine);
                    monsterPrimeRoutine = StartCoroutine(PrimeOriginalMonsterPursuit());
                }
            }

            if (monster == null)
                return;

            NavMeshAgent agent = monster.GetComponent<NavMeshAgent>();
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = !enabled;
                if (!enabled)
                    agent.ResetPath();
            }

            Animator animator = monster.GetComponent<Animator>();
            if (animator != null)
                animator.speed = enabled || monster.IsDead ? 1f : 0f;
        }

        private IEnumerator PrimeOriginalMonsterPursuit()
        {
            yield return null;
            monsterPrimeRoutine = null;

            if (State != MatchState.Fighting || monsterBehavior == null ||
                !monsterBehavior.enabled || monsterBehavior.target != null)
            {
                yield break;
            }

            if (monsterBehavior.controller == null)
            {
                monsterBehavior.enabled = false;
                monsterBehavior.enabled = true;
            }

            monsterBehavior.FindTarget();
            if (monsterBehavior.target != null)
                monsterBehavior.StartPursuit();
        }

        private void ReleaseMonsterAttackState()
        {
            if (monster == null)
                return;

            Animator animator = monster.GetComponent<Animator>();
            if (animator == null)
                return;

            animator.applyRootMotion = true;
            animator.Rebind();
            animator.SetBool("InPursuit", false);
            animator.Play("Base Layer.ChomperIdle", 0, 0f);
            animator.speed = 0f;
        }

        private void SetState(MatchState next)
        {
            if (State == next)
                return;

            MatchState previous = State;
            State = next;
            StateChanged?.Invoke(previous, next);
        }

        private void BindHealthEvents()
        {
            if (healthEventsBound)
                return;

            if (player != null)
            {
                player.HealthChanged += HandleHealthChanged;
                player.Died += HandleDied;
            }

            if (monster != null)
            {
                monster.HealthChanged += HandleHealthChanged;
                monster.Died += HandleDied;
            }

            healthEventsBound = true;
        }

        private void UnbindHealthEvents()
        {
            if (!healthEventsBound)
                return;

            if (player != null)
            {
                player.HealthChanged -= HandleHealthChanged;
                player.Died -= HandleDied;
            }

            if (monster != null)
            {
                monster.HealthChanged -= HandleHealthChanged;
                monster.Died -= HandleDied;
            }

            healthEventsBound = false;
        }
    }
}
