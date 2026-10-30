using MotionFight.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MotionFight.UI
{
    [DisallowMultipleComponent]
    public sealed class CombatantHealthBarView : MonoBehaviour
    {
        [SerializeField] private CombatantHealth health;
        [SerializeField] private Image fill;
        [SerializeField] private Image delayedFill;
        [SerializeField] private Image glow;
        [SerializeField] private Image flashOverlay;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private TMP_Text percentText;
        [SerializeField] private bool fillFromRight;
        [SerializeField, Min(0.01f)] private float fillDuration = 0.25f;
        [SerializeField, Min(0f)] private float delayedWait = 0.35f;
        [SerializeField, Min(0.01f)] private float delayedDuration = 0.45f;
        [SerializeField, Range(0f, 1f)] private float lowHealthThreshold = 0.25f;

        private float targetValue = 1f;
        private float displayedValue = 1f;
        private float delayedValue = 1f;
        private float delayRemaining;
        private float flashRemaining;
        private Color fillBaseColor;
        private bool subscribed;

        public CombatantHealth Health => health;
        public bool IsLowHealth => targetValue > 0f && targetValue <= lowHealthThreshold;

        public void Configure(
            Image primaryFill,
            Image delayed,
            Image glowImage,
            Image flash,
            TMP_Text amount,
            TMP_Text percent,
            bool fromRight)
        {
            fill = primaryFill;
            delayedFill = delayed;
            glow = glowImage;
            flashOverlay = flash;
            valueText = amount;
            percentText = percent;
            fillFromRight = fromRight;
        }

        private void Awake()
        {
            if (fill != null)
            {
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                fill.fillOrigin = fillFromRight ? 1 : 0;
                fillBaseColor = fill.color;
            }

            if (delayedFill != null)
            {
                delayedFill.type = Image.Type.Filled;
                delayedFill.fillMethod = Image.FillMethod.Horizontal;
                delayedFill.fillOrigin = fillFromRight ? 1 : 0;
            }

            if (flashOverlay != null)
                flashOverlay.color = new Color(1f, 1f, 1f, 0f);
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            float alphaStep = Time.unscaledDeltaTime;
            displayedValue = Mathf.MoveTowards(
                displayedValue,
                targetValue,
                alphaStep / Mathf.Max(0.01f, fillDuration));

            if (delayRemaining > 0f)
                delayRemaining -= alphaStep;
            else
            {
                delayedValue = Mathf.MoveTowards(
                    delayedValue,
                    targetValue,
                    alphaStep / Mathf.Max(0.01f, delayedDuration));
            }

            if (fill != null)
                fill.fillAmount = displayedValue;
            if (delayedFill != null)
                delayedFill.fillAmount = Mathf.Max(displayedValue, delayedValue);

            UpdateFlash(alphaStep);
            UpdateLowHealthGlow();
        }

        public void Bind(CombatantHealth target)
        {
            if (health == target)
            {
                RefreshText();
                return;
            }

            Unsubscribe();
            health = target;
            Subscribe();
            SnapToCurrent();
        }

        public void SnapToCurrent()
        {
            targetValue = health != null ? Mathf.Clamp01(health.NormalizedHealth) : 0f;
            displayedValue = targetValue;
            delayedValue = targetValue;
            delayRemaining = 0f;
            if (fill != null)
                fill.fillAmount = targetValue;
            if (delayedFill != null)
                delayedFill.fillAmount = targetValue;
            RefreshText();
        }

        private void Subscribe()
        {
            if (subscribed || health == null)
                return;

            health.HealthChanged += HandleHealthChanged;
            subscribed = true;
            RefreshText();
        }

        private void Unsubscribe()
        {
            if (!subscribed || health == null)
                return;

            health.HealthChanged -= HandleHealthChanged;
            subscribed = false;
        }

        private void HandleHealthChanged(CombatantHealth changedHealth, float previous, float current)
        {
            float next = Mathf.Clamp01(changedHealth.NormalizedHealth);
            if (next < targetValue)
            {
                delayRemaining = delayedWait;
                flashRemaining = 0.18f;
            }

            targetValue = next;
            RefreshText();
        }

        private void RefreshText()
        {
            if (health == null)
                return;

            float current = Mathf.Clamp(health.CurrentHealth, 0f, health.MaxHealth);
            float maximum = Mathf.Max(0f, health.MaxHealth);
            if (valueText != null)
                valueText.text = Mathf.CeilToInt(current).ToString("000") + " / " +
                                 Mathf.CeilToInt(maximum).ToString("000");
            if (percentText != null)
                percentText.text = Mathf.RoundToInt(Mathf.Clamp01(current / Mathf.Max(0.01f, maximum)) * 100f) + "%";
        }

        private void UpdateFlash(float deltaTime)
        {
            if (flashOverlay == null)
                return;

            if (flashRemaining <= 0f)
            {
                flashOverlay.color = new Color(1f, 1f, 1f, 0f);
                if (fill != null)
                    fill.color = fillBaseColor;
                return;
            }

            flashRemaining = Mathf.Max(0f, flashRemaining - deltaTime);
            float pulse = Mathf.Sin((flashRemaining / 0.18f) * Mathf.PI);
            flashOverlay.color = new Color(1f, 1f, 1f, pulse * 0.55f);
            if (fill != null)
                fill.color = Color.Lerp(fillBaseColor, Color.white, pulse * 0.65f);
        }

        private void UpdateLowHealthGlow()
        {
            if (glow == null)
                return;

            if (!IsLowHealth)
            {
                glow.color = new Color(glow.color.r, glow.color.g, glow.color.b, 0.15f);
                return;
            }

            float alpha = 0.35f + Mathf.PingPong(Time.unscaledTime * 2.7f, 0.45f);
            glow.color = new Color(glow.color.r, glow.color.g, glow.color.b, alpha);
        }
    }
}
