using System.Collections;
using UnityEngine;

namespace MotionFight.UI
{
    [DisallowMultipleComponent]
    public sealed class HUDPanelAnimator : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panel;
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.2f;
        [SerializeField, Min(0.8f)] private float hiddenScale = 0.965f;

        private Coroutine animationRoutine;

        public bool IsVisible => canvasGroup != null && canvasGroup.alpha > 0.001f;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            if (panel == null)
                panel = transform as RectTransform;
        }

        public void Configure(CanvasGroup group, RectTransform panelTransform)
        {
            canvasGroup = group;
            panel = panelTransform;
        }

        public void HideImmediate()
        {
            StopAnimation();
            gameObject.SetActive(false);
            SetVisual(0f, hiddenScale);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            AnimateTo(1f, 1f);
        }

        public void Hide()
        {
            if (!gameObject.activeSelf)
                return;

            AnimateTo(0f, hiddenScale, true);
        }

        public IEnumerator FadeOut(float duration)
        {
            yield return Animate(0f, hiddenScale, Mathf.Max(0.01f, duration), false);
            gameObject.SetActive(false);
        }

        private void AnimateTo(float alpha, float scale, bool deactivateAtEnd = false)
        {
            if (!gameObject.activeInHierarchy)
            {
                SetVisual(alpha, scale);
                return;
            }

            StopAnimation();
            animationRoutine = StartCoroutine(Animate(alpha, scale, fadeDuration, deactivateAtEnd));
        }

        private IEnumerator Animate(float targetAlpha, float targetScale, float duration, bool deactivateAtEnd)
        {
            float startAlpha = canvasGroup.alpha;
            float startScale = panel != null ? panel.localScale.x : 1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);
                SetVisual(Mathf.Lerp(startAlpha, targetAlpha, t), Mathf.Lerp(startScale, targetScale, t));
                yield return null;
            }

            SetVisual(targetAlpha, targetScale);
            animationRoutine = null;
            if (deactivateAtEnd)
                gameObject.SetActive(false);
        }

        private void SetVisual(float alpha, float scale)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
                canvasGroup.interactable = alpha > 0.95f;
                canvasGroup.blocksRaycasts = alpha > 0.95f;
            }

            if (panel != null)
                panel.localScale = Vector3.one * scale;
        }

        private void StopAnimation()
        {
            if (animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
                animationRoutine = null;
            }
        }
    }
}
