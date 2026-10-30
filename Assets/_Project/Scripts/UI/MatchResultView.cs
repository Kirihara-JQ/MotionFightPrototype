using MotionFight.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MotionFight.UI
{
    [DisallowMultipleComponent]
    public sealed class MatchResultView : MonoBehaviour
    {
        [SerializeField] private HUDPanelAnimator animator;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private TMP_Text mainMenuLabel;

        public Button RestartButton => restartButton;
        public Button MainMenuButton => mainMenuButton;

        public void Configure(
            HUDPanelAnimator panelAnimator,
            TMP_Text titleText,
            TMP_Text subtitleText,
            Button restart,
            Button mainMenu,
            TMP_Text menuLabel)
        {
            animator = panelAnimator;
            title = titleText;
            subtitle = subtitleText;
            restartButton = restart;
            mainMenuButton = mainMenu;
            mainMenuLabel = menuLabel;
        }

        public void Show(MatchState state)
        {
            switch (state)
            {
                case MatchState.PlayerWin:
                    SetText("PLAYER WINS", "YOU DEFEATED THE CHOMPER");
                    break;
                case MatchState.MonsterWin:
                    SetText("MONSTER WINS", "YOU HAVE BEEN DEFEATED");
                    break;
                case MatchState.Draw:
                    SetText("DRAW", "BOTH COMBATANTS HAVE FALLEN");
                    break;
                default:
                    Hide();
                    return;
            }

            animator.Show();
        }

        public void Hide()
        {
            if (animator != null)
                animator.Hide();
        }

        public void SetMainMenuAvailable(bool available)
        {
            if (mainMenuButton != null)
            {
                mainMenuButton.interactable = available;
                HUDButtonMotion motion = mainMenuButton.GetComponent<HUDButtonMotion>();
                if (motion != null)
                    motion.enabled = available;
                if (mainMenuButton.targetGraphic != null)
                    mainMenuButton.targetGraphic.color = available
                        ? new Color32(0x81, 0xDD, 0xF5, 0xFF)
                        : new Color32(0x8F, 0xA6, 0xBA, 0x66);
            }
            if (mainMenuLabel != null)
            {
                mainMenuLabel.text = available ? "RETURN TO MAIN MENU" : "MAIN MENU";
                mainMenuLabel.color = available
                    ? new Color32(0x81, 0xDD, 0xF5, 0xFF)
                    : new Color32(0x8F, 0xA6, 0xBA, 0x99);
            }
        }

        private void SetText(string titleText, string subtitleText)
        {
            if (title != null)
                title.text = titleText;
            if (subtitle != null)
                subtitle.text = subtitleText;
        }
    }
}
