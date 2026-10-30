using TMPro;
using UnityEngine;

namespace MotionFight.UI
{
    public enum HUDInputDisplayMode
    {
        KeyboardMouse,
        MotionControl
    }

    [DisallowMultipleComponent]
    public sealed class ControlHintView : MonoBehaviour
    {
        [SerializeField] private TMP_Text moveKeys;
        [SerializeField] private TMP_Text moveLabel;
        [SerializeField] private TMP_Text cameraKeys;
        [SerializeField] private TMP_Text cameraLabel;
        [SerializeField] private TMP_Text leftKeys;
        [SerializeField] private TMP_Text leftLabel;
        [SerializeField] private TMP_Text rightKeys;
        [SerializeField] private TMP_Text rightLabel;
        [SerializeField] private TMP_Text guardKeys;
        [SerializeField] private TMP_Text guardLabel;
        [SerializeField] private TMP_Text pauseKeys;
        [SerializeField] private TMP_Text pauseLabel;
        [SerializeField] private HUDInputDisplayMode displayMode = HUDInputDisplayMode.KeyboardMouse;

        public void Configure(
            TMP_Text moveKeyText,
            TMP_Text moveActionText,
            TMP_Text cameraKeyText,
            TMP_Text cameraActionText,
            TMP_Text leftKeyText,
            TMP_Text leftActionText,
            TMP_Text rightKeyText,
            TMP_Text rightActionText,
            TMP_Text guardKeyText,
            TMP_Text guardActionText,
            TMP_Text pauseKeyText,
            TMP_Text pauseActionText)
        {
            moveKeys = moveKeyText;
            moveLabel = moveActionText;
            cameraKeys = cameraKeyText;
            cameraLabel = cameraActionText;
            leftKeys = leftKeyText;
            leftLabel = leftActionText;
            rightKeys = rightKeyText;
            rightLabel = rightActionText;
            guardKeys = guardKeyText;
            guardLabel = guardActionText;
            pauseKeys = pauseKeyText;
            pauseLabel = pauseActionText;
        }

        private void Start()
        {
            SetDisplayMode(displayMode);
        }

        public void SetDisplayMode(HUDInputDisplayMode mode)
        {
            displayMode = mode;
            if (mode == HUDInputDisplayMode.KeyboardMouse)
            {
                SetGroup(moveKeys, moveLabel, "W A S D", "MOVE");
                SetGroup(cameraKeys, cameraLabel, "MOUSE", "LOOK");
                SetGroup(leftKeys, leftLabel, "LMB", "LEFT PUNCH");
                SetGroup(rightKeys, rightLabel, "RMB", "RIGHT PUNCH");
                SetGroup(guardKeys, guardLabel, "L-SHIFT", "GUARD");
                SetGroup(pauseKeys, pauseLabel, "ESC", "PAUSE");
            }
            else
            {
                SetGroup(moveKeys, moveLabel, "BODY", "MOVE");
                SetGroup(cameraKeys, cameraLabel, "HEAD", "LOOK");
                SetGroup(leftKeys, leftLabel, "LEFT", "STRAIGHT");
                SetGroup(rightKeys, rightLabel, "RIGHT", "STRAIGHT");
                SetGroup(guardKeys, guardLabel, "POSTURE", "GUARD");
                SetGroup(pauseKeys, pauseLabel, "VOICE", "PAUSE");
            }
        }

        private static void SetGroup(TMP_Text keys, TMP_Text label, string keyText, string labelText)
        {
            if (keys != null)
                keys.text = keyText;
            if (label != null)
                label.text = labelText;
        }
    }
}
