using System.Collections;
using MotionFight.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MotionFight.Combat
{
    /// <summary>
    /// Presentation-only Combat HUD V2 coordinator. Combat state, HP, damage,
    /// pause, restart, and outcomes remain authority of the existing combat core.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-6000)]
    public sealed class MatchHUD : MonoBehaviour
    {
        [Header("Authority")]
        [SerializeField] private MatchController matchController;
        [SerializeField] private CombatResolver resolver;

        [Header("Player")]
        [SerializeField] private CombatantHealthBarView playerHealthBar;
        [SerializeField] private TMP_Text playerName;
        [SerializeField] private TMP_Text playerRole;

        [Header("Monster")]
        [SerializeField] private CombatantHealthBarView monsterHealthBar;
        [SerializeField] private TMP_Text monsterName;
        [SerializeField] private TMP_Text monsterRole;

        [Header("Views")]
        [SerializeField] private MatchStatusView matchStatus;
        [SerializeField] private MatchResultView resultView;
        [SerializeField] private PauseMenuView pauseView;
        [SerializeField] private ControlHintView controlHints;

        [Header("Feedback")]
        [SerializeField] private CanvasGroup guardFeedbackGroup;
        [SerializeField] private TMP_Text guardFeedbackText;
        [SerializeField] private CanvasGroup hitFeedbackGroup;
        [SerializeField] private TMP_Text hitFeedbackText;
        [SerializeField] private CanvasGroup lowHealthGroup;
        [SerializeField] private CanvasGroup rootCanvasGroup;

        [SerializeField, Min(0.01f)] private float feedbackDuration = 0.42f;
        [SerializeField, Min(0.01f)] private float restartFadeDuration = 0.22f;

        private Coroutine guardFeedbackRoutine;
        private Coroutine hitFeedbackRoutine;
        private bool healthSubscribed;
        private bool resolverSubscribed;
        private bool cursorStateCached;
        private bool cachedCursorVisible;
        private CursorLockMode cachedCursorLock;
        private string mainMenuSceneName;

        public void Configure(
            MatchController controller,
            CombatantHealthBarView playerBar,
            TMP_Text playerNameText,
            TMP_Text playerRoleText,
            CombatantHealthBarView monsterBar,
            TMP_Text monsterNameText,
            TMP_Text monsterRoleText,
            MatchStatusView statusView,
            MatchResultView matchResult,
            PauseMenuView pauseMenu,
            ControlHintView controls,
            CanvasGroup guardGroup,
            TMP_Text guardText,
            CanvasGroup hitGroup,
            TMP_Text hitText,
            CanvasGroup lowHpGroup,
            CanvasGroup rootGroup)
        {
            matchController = controller;
            playerHealthBar = playerBar;
            playerName = playerNameText;
            playerRole = playerRoleText;
            monsterHealthBar = monsterBar;
            monsterName = monsterNameText;
            monsterRole = monsterRoleText;
            matchStatus = statusView;
            resultView = matchResult;
            pauseView = pauseMenu;
            controlHints = controls;
            guardFeedbackGroup = guardGroup;
            guardFeedbackText = guardText;
            hitFeedbackGroup = hitGroup;
            hitFeedbackText = hitText;
            lowHealthGroup = lowHpGroup;
            rootCanvasGroup = rootGroup;
        }

        private void Awake()
        {
            if (matchController == null)
                matchController = MatchController.Instance;
            if (resolver == null)
                resolver = CombatResolver.Instance;
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Start()
        {
            Subscribe();
            BindViews();
            ConfigureButtons();
            ResolveMainMenuScene();

            if (guardFeedbackGroup != null)
                guardFeedbackGroup.alpha = 0f;
            if (hitFeedbackGroup != null)
                hitFeedbackGroup.alpha = 0f;
            if (lowHealthGroup != null)
                lowHealthGroup.alpha = 0f;

            if (resultView != null)
                resultView.Hide();
            if (pauseView != null)
                pauseView.Hide();

            RefreshAll();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            UpdateLowHealthWarning();
        }

        private void Subscribe()
        {
            if (!healthSubscribed && matchController != null)
            {
                matchController.StateChanged += HandleStateChanged;
                if (matchController.PlayerCombatant != null)
                    matchController.PlayerCombatant.HealthChanged += HandleHealthChanged;
                if (matchController.MonsterCombatant != null)
                    matchController.MonsterCombatant.HealthChanged += HandleHealthChanged;
                healthSubscribed = true;
            }

            if (!resolverSubscribed && resolver != null)
            {
                resolver.HitResolved += HandleHitResolved;
                resolverSubscribed = true;
            }
        }

        private void Unsubscribe()
        {
            if (healthSubscribed && matchController != null)
            {
                matchController.StateChanged -= HandleStateChanged;
                if (matchController.PlayerCombatant != null)
                    matchController.PlayerCombatant.HealthChanged -= HandleHealthChanged;
                if (matchController.MonsterCombatant != null)
                    matchController.MonsterCombatant.HealthChanged -= HandleHealthChanged;
            }
            healthSubscribed = false;

            if (resolverSubscribed && resolver != null)
                resolver.HitResolved -= HandleHitResolved;
            resolverSubscribed = false;
        }

        private void BindViews()
        {
            if (matchController == null)
                return;

            if (playerName != null)
                playerName.text = matchController.PlayerCombatant != null
                    ? matchController.PlayerCombatant.DisplayName.ToUpperInvariant()
                    : "ELLEN";
            if (playerRole != null)
                playerRole.text = "PLAYER";
            if (monsterName != null)
                monsterName.text = matchController.MonsterCombatant != null
                    ? matchController.MonsterCombatant.DisplayName.ToUpperInvariant()
                    : "CHOMPER";
            if (monsterRole != null)
                monsterRole.text = "MONSTER";

            if (playerHealthBar != null)
                playerHealthBar.Bind(matchController.PlayerCombatant);
            if (monsterHealthBar != null)
                monsterHealthBar.Bind(matchController.MonsterCombatant);
        }

        private void ConfigureButtons()
        {
            if (resultView != null)
            {
                BindButton(resultView.RestartButton, HandleRestartClicked);
                if (resultView.MainMenuButton != null)
                {
                    resultView.MainMenuButton.onClick.RemoveAllListeners();
                    resultView.MainMenuButton.onClick.AddListener(HandleMainMenuClicked);
                }
                resultView.SetMainMenuAvailable(!string.IsNullOrWhiteSpace(mainMenuSceneName));
            }

            if (pauseView != null)
            {
                BindButton(pauseView.ResumeButton, HandleResumeClicked);
                BindButton(pauseView.RestartButton, HandleRestartClicked);
                BindButton(pauseView.ExitButton, HandleExitClicked);
#if UNITY_EDITOR
                pauseView.SetExitAvailable(false);
#else
                pauseView.SetExitAvailable(true);
#endif
            }
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void ResolveMainMenuScene()
        {
            mainMenuSceneName = null;
            string currentScene = SceneManager.GetActiveScene().path;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                if (string.IsNullOrWhiteSpace(path) || path == currentScene)
                    continue;

                string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
                string lowered = sceneName.ToLowerInvariant();
                if (lowered.Contains("menu") || lowered.Contains("main"))
                {
                    mainMenuSceneName = sceneName;
                    break;
                }
            }
        }

        private void HandleStateChanged(MatchState previous, MatchState current)
        {
            RefreshState(current);
        }

        private void HandleHealthChanged(CombatantHealth health, float previous, float current)
        {
            UpdateLowHealthWarning();
        }

        private void RefreshAll()
        {
            if (matchController == null)
                return;

            BindViews();
            RefreshState(matchController.State);
        }

        private void RefreshState(MatchState state)
        {
            if (matchStatus != null)
                matchStatus.Render(state);

            bool paused = state == MatchState.Paused;
            bool ended = state == MatchState.PlayerWin ||
                         state == MatchState.MonsterWin ||
                         state == MatchState.Draw;

            if (pauseView != null)
            {
                if (paused)
                    pauseView.Show();
                else
                    pauseView.Hide();
            }

            if (resultView != null)
            {
                if (ended)
                    resultView.Show(state);
                else
                    resultView.Hide();
            }

            SetMenuCursor(paused || ended);
        }

        private void UpdateLowHealthWarning()
        {
            if (lowHealthGroup == null || matchController == null || matchController.PlayerCombatant == null)
                return;

            bool visible = matchController.IsFighting &&
                           !matchController.PlayerCombatant.IsDead &&
                           matchController.PlayerCombatant.NormalizedHealth <= 0.25f;

            if (!visible)
            {
                lowHealthGroup.alpha = Mathf.MoveTowards(lowHealthGroup.alpha, 0f, Time.unscaledDeltaTime * 4f);
                return;
            }

            float pulse = 0.58f + Mathf.PingPong(Time.unscaledTime * 1.8f, 0.35f);
            lowHealthGroup.alpha = Mathf.MoveTowards(lowHealthGroup.alpha, pulse, Time.unscaledDeltaTime * 4f);
        }

        private void HandleHitResolved(CombatHitResult result)
        {
            if (result.Blocked)
            {
                ShowFeedback(
                    ref guardFeedbackRoutine,
                    guardFeedbackGroup,
                    guardFeedbackText,
                    "GUARD",
                    new Color32(0x81, 0xDD, 0xF5, 0xFF));
            }
            else if (result.AppliedDamage > 0f)
            {
                string label = result.Target == matchController.PlayerCombatant ? "DAMAGE" : "HIT";
                Color color = result.Target == matchController.PlayerCombatant
                    ? new Color32(0xED, 0x49, 0x63, 0xFF)
                    : new Color32(0xFF, 0xC4, 0x6C, 0xFF);
                ShowFeedback(ref hitFeedbackRoutine, hitFeedbackGroup, hitFeedbackText, label, color);
            }
        }

        private void ShowFeedback(
            ref Coroutine routine,
            CanvasGroup group,
            TMP_Text label,
            string text,
            Color color)
        {
            if (group == null)
                return;

            if (routine != null)
                StopCoroutine(routine);

            if (label != null)
            {
                label.text = text;
                label.color = color;
            }

            routine = StartCoroutine(AnimateFeedback(group));
        }

        private IEnumerator AnimateFeedback(CanvasGroup group)
        {
            float elapsed = 0f;
            RectTransform rect = group.transform as RectTransform;
            Vector3 baseScale = rect != null ? Vector3.one : Vector3.one;

            while (elapsed < feedbackDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / feedbackDuration);
                group.alpha = Mathf.Sin(t * Mathf.PI);
                if (rect != null)
                    rect.localScale = baseScale * Mathf.Lerp(0.96f, 1.04f, t);
                yield return null;
            }

            group.alpha = 0f;
            if (rect != null)
                rect.localScale = baseScale;
        }

        private void SetMenuCursor(bool menuVisible)
        {
            if (menuVisible)
            {
                if (!cursorStateCached)
                {
                    cachedCursorVisible = Cursor.visible;
                    cachedCursorLock = Cursor.lockState;
                    cursorStateCached = true;
                }

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else if (cursorStateCached)
            {
                Cursor.visible = cachedCursorVisible;
                Cursor.lockState = cachedCursorLock;
                cursorStateCached = false;
                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private void HandleResumeClicked()
        {
            matchController?.ResumeMatch();
        }

        private void HandleRestartClicked()
        {
            if (matchController == null || !matchController.isActiveAndEnabled)
                return;

            StartCoroutine(RestartAfterFade());
        }

        private IEnumerator RestartAfterFade()
        {
            if (rootCanvasGroup != null)
            {
                float elapsed = 0f;
                float start = rootCanvasGroup.alpha;
                while (elapsed < restartFadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    rootCanvasGroup.alpha = Mathf.Lerp(start, 0f, elapsed / restartFadeDuration);
                    yield return null;
                }
            }

            matchController.RestartMatch();
        }

        private void HandleMainMenuClicked()
        {
            if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
                SceneManager.LoadScene(mainMenuSceneName);
        }

        private void HandleExitClicked()
        {
#if UNITY_EDITOR
            Debug.LogWarning("Exit is disabled while running inside the Unity Editor.", this);
#else
            Application.Quit();
#endif
        }
    }
}
