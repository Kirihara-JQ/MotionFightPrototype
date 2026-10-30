using MotionFight.Combat;
using TMPro;
using UnityEngine;

namespace MotionFight.UI
{
    [DisallowMultipleComponent]
    public sealed class MatchStatusView : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField] private TMP_Text roundLabel;

        public void Configure(TMP_Text titleText, TMP_Text subtitleText, TMP_Text roundText)
        {
            title = titleText;
            subtitle = subtitleText;
            roundLabel = roundText;
        }

        public void Render(MatchState state)
        {
            if (roundLabel != null)
                roundLabel.text = "ROUND 01";

            switch (state)
            {
                case MatchState.Ready:
                    Set("STANDBY", "PREPARE FOR COMBAT", new Color32(0x81, 0xDD, 0xF5, 0xFF));
                    break;
                case MatchState.Fighting:
                    Set("FIGHT", "DEFEAT THE MONSTER", new Color32(0xFF, 0xC4, 0x6C, 0xFF));
                    break;
                case MatchState.Paused:
                    Set("PAUSED", "COMBAT SUSPENDED", new Color32(0x8F, 0xA6, 0xBA, 0xFF));
                    break;
                case MatchState.PlayerWin:
                    Set("MATCH COMPLETE", "MONSTER DEFEATED", new Color32(0xFF, 0xC4, 0x6C, 0xFF));
                    break;
                case MatchState.MonsterWin:
                    Set("MATCH COMPLETE", "PLAYER DEFEATED", new Color32(0xED, 0x49, 0x63, 0xFF));
                    break;
                case MatchState.Draw:
                    Set("MATCH COMPLETE", "BOTH COMBATANTS HAVE FALLEN", new Color32(0xFF, 0xC4, 0x6C, 0xFF));
                    break;
            }
        }

        private void Set(string titleText, string subtitleText, Color accent)
        {
            if (title != null)
            {
                title.text = titleText;
                title.color = accent;
            }

            if (subtitle != null)
                subtitle.text = subtitleText;
        }
    }
}
