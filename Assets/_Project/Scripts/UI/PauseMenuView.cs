using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MotionFight.UI
{
    [DisallowMultipleComponent]
    public sealed class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private HUDPanelAnimator animator;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private TMP_Text exitLabel;

        public Button ResumeButton => resumeButton;
        public Button RestartButton => restartButton;
        public Button ExitButton => exitButton;

        public void Configure(
            HUDPanelAnimator panelAnimator,
            Button resume,
            Button restart,
            Button exit,
            TMP_Text exitText)
        {
            animator = panelAnimator;
            resumeButton = resume;
            restartButton = restart;
            exitButton = exit;
            exitLabel = exitText;
        }

        public void Show()
        {
            animator.Show();
        }

        public void Hide()
        {
            animator.Hide();
        }

        public void SetExitAvailable(bool available)
        {
            if (exitButton != null)
            {
                exitButton.interactable = available;
                HUDButtonMotion motion = exitButton.GetComponent<HUDButtonMotion>();
                if (motion != null)
                    motion.enabled = available;
            }
            if (exitLabel != null)
            {
                exitLabel.text = available ? "EXIT" : "EXIT // EDITOR";
                exitLabel.color = available
                    ? new Color32(0xF4, 0xF7, 0xFB, 0xFF)
                    : new Color32(0x8F, 0xA6, 0xBA, 0x88);
            }
        }
    }
}
