using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MotionFight.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class HUDButtonMotion : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private Graphic targetGraphic;
        [SerializeField] private Color normalColor = new Color(0.035f, 0.065f, 0.09f, 0.94f);
        [SerializeField] private Color hoverColor = new Color(0.08f, 0.16f, 0.21f, 0.98f);
        [SerializeField] private Color pressedColor = new Color(0.11f, 0.24f, 0.28f, 1f);
        [SerializeField, Min(1f)] private float hoverScale = 1.025f;
        [SerializeField, Min(0f)] private float transitionSpeed = 16f;

        private bool hovered;
        private bool pressed;
        private Vector3 targetScale = Vector3.one;

        public void Configure(RectTransform motionTarget, Graphic graphic)
        {
            Configure(motionTarget, graphic, normalColor, hoverColor, pressedColor);
        }

        public void Configure(
            RectTransform motionTarget,
            Graphic graphic,
            Color normal,
            Color hover,
            Color pressed)
        {
            target = motionTarget;
            targetGraphic = graphic;
            normalColor = normal;
            hoverColor = hover;
            pressedColor = pressed;
        }

        private void Awake()
        {
            if (target == null)
                target = transform as RectTransform;
            if (targetGraphic == null)
                targetGraphic = GetComponent<Graphic>();
        }

        private void Update()
        {
            float step = 1f - Mathf.Exp(-transitionSpeed * Time.unscaledDeltaTime);
            if (target != null)
                target.localScale = Vector3.Lerp(target.localScale, targetScale, step);

            if (targetGraphic == null)
                return;

            Color desired = pressed ? pressedColor : hovered ? hoverColor : normalColor;
            targetGraphic.color = Color.Lerp(targetGraphic.color, desired, step);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            targetScale = Vector3.one * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            pressed = false;
            targetScale = Vector3.one;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pressed = true;
            targetScale = Vector3.one * 0.975f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pressed = false;
            targetScale = Vector3.one * (hovered ? hoverScale : 1f);
        }

        private void OnDisable()
        {
            hovered = false;
            pressed = false;
            targetScale = Vector3.one;
            if (target != null)
                target.localScale = Vector3.one;
        }
    }
}
