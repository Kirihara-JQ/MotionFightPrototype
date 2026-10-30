using System.IO;
using MotionFight.Combat;
using MotionFight.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MotionFight.EditorTools
{
    public static class CombatHUDV2Builder
    {
        private const string ArtFolder = "Assets/_Project/Art/UI/CombatHUDV2";
        private const string PrefabFolder = "Assets/_Project/Prefabs/UI";
        private const string PrefabPath = PrefabFolder + "/PF_CombatHUD_V2.prefab";
        private const string FontPath =
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        private const string EllenPortraitPath =
            "Assets/3DGamekit/Art/Sprites/AnimatedSprites/Ellen/CutsceneAnimation/Ellen_CutscenePortrait_000.png";
        private const string ChomperPortraitPath =
            "Assets/3DGamekit/Art/Sprites/UI/Menu/LoadingChomper.png";

        private static readonly Color PlayerCyan = Hex("#24C8F4");
        private static readonly Color PlayerDark = Hex("#071B2B");
        private static readonly Color PlayerLight = Hex("#81DDF5");
        private static readonly Color MonsterRed = Hex("#ED4963");
        private static readonly Color MonsterDark = Hex("#240E19");
        private static readonly Color MonsterLight = Hex("#FF9AA8");
        private static readonly Color VictoryGold = Hex("#FFC46C");
        private static readonly Color PrimaryText = Hex("#F4F7FB");
        private static readonly Color SecondaryText = Hex("#8FA6BA");
        private static readonly Color PanelBlueBlack = Hex("#08131E");

        private struct ButtonParts
        {
            public Button Button;
            public TMP_Text Label;
        }

        private struct HealthBarParts
        {
            public CombatantHealthBarView View;
            public Image Fill;
            public Image DelayedFill;
            public Image Glow;
            public Image Flash;
            public TMP_Text Value;
            public TMP_Text Percent;
        }

        [MenuItem("Tools/Motion Fight/UI/Rebuild Combat HUD V2")]
        public static void Rebuild()
        {
            EnsureFolder("Assets/_Project/Art");
            EnsureFolder("Assets/_Project/Art/UI");
            EnsureFolder(ArtFolder);
            EnsureFolder("Assets/_Project/Prefabs");
            EnsureFolder(PrefabFolder);
            EnsureFolder("Assets/_Project/Editor");
            EnsureFolder("Assets/_Project/Editor/UI");

            Sprite panelSprite = CreateRoundedSprite(ArtFolder + "/HUD_Panel_Rounded.png", 128, 24);
            Sprite keycapSprite = CreateRoundedSprite(ArtFolder + "/HUD_Keycap.png", 64, 14);
            Sprite barSprite = CreateBarSprite(ArtFolder + "/HUD_BarFill.png");
            Sprite glowSprite = CreateGlowSprite(ArtFolder + "/HUD_Glow.png");
            Sprite diamondSprite = CreateDiamondSprite(ArtFolder + "/HUD_Diamond.png");

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            Sprite ellenPortrait = AssetDatabase.LoadAssetAtPath<Sprite>(EllenPortraitPath);
            Sprite chomperPortrait = AssetDatabase.LoadAssetAtPath<Sprite>(ChomperPortraitPath);
            if (font == null || ellenPortrait == null || chomperPortrait == null)
            {
                Debug.LogError(
                    "Combat HUD V2 requires the verified project TMP font and character portraits.",
                    AssetDatabase.LoadAssetAtPath<Object>(ArtFolder));
                return;
            }

            GameObject root = BuildHierarchy(
                font,
                panelSprite,
                keycapSprite,
                barSprite,
                glowSprite,
                diamondSprite,
                ellenPortrait,
                chomperPortrait);

            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
                AssetDatabase.DeleteAsset(PrefabPath);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out bool success);
            Object.DestroyImmediate(root);
            if (!success || prefab == null)
            {
                Debug.LogError("Failed to create Combat HUD V2 prefab.", prefab);
                return;
            }

            IntegratePrefabIntoScene(prefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = prefab;
            Debug.Log("Combat HUD V2 prefab rebuilt and integrated into the formal combat scene.");
        }

        private static GameObject BuildHierarchy(
            TMP_FontAsset font,
            Sprite panelSprite,
            Sprite keycapSprite,
            Sprite barSprite,
            Sprite glowSprite,
            Sprite diamondSprite,
            Sprite ellenPortrait,
            Sprite chomperPortrait)
        {
            var root = new GameObject(
                "HUD_Root",
                typeof(RectTransform),
                typeof(CanvasGroup),
                typeof(MatchHUD));
            var rootRect = (RectTransform)root.transform;
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            var rootGroup = root.GetComponent<CanvasGroup>();
            rootGroup.alpha = 1f;
            rootGroup.interactable = true;
            rootGroup.blocksRaycasts = true;

            RectTransform topHud = CreateRect("TopHUD", rootRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            BuildPlayerPanel(topHud, font, panelSprite, barSprite, glowSprite, diamondSprite, ellenPortrait, out var playerBar, out TMP_Text playerName, out TMP_Text playerRole);
            BuildMonsterPanel(topHud, font, panelSprite, barSprite, glowSprite, diamondSprite, chomperPortrait, out var monsterBar, out TMP_Text monsterName, out TMP_Text monsterRole);
            MatchStatusView status = BuildMatchStatus(topHud, font, panelSprite, diamondSprite);

            MatchResultView result = BuildResultPanel(rootRect, font, panelSprite, diamondSprite);
            PauseMenuView pause = BuildPausePanel(rootRect, font, panelSprite, diamondSprite);
            BuildCombatFeedback(rootRect, font, panelSprite, out CanvasGroup guardGroup, out TMP_Text guardText, out CanvasGroup hitGroup, out TMP_Text hitText, out CanvasGroup lowHpGroup);
            ControlHintView controls = BuildControlBar(rootRect, font, panelSprite, keycapSprite);

            MatchHUD hud = root.GetComponent<MatchHUD>();
            hud.Configure(
                null,
                playerBar,
                playerName,
                playerRole,
                monsterBar,
                monsterName,
                monsterRole,
                status,
                result,
                pause,
                controls,
                guardGroup,
                guardText,
                hitGroup,
                hitText,
                lowHpGroup,
                rootGroup);

            return root;
        }

        private static void BuildPlayerPanel(
            Transform parent,
            TMP_FontAsset font,
            Sprite panelSprite,
            Sprite barSprite,
            Sprite glowSprite,
            Sprite diamondSprite,
            Sprite portrait,
            out CombatantHealthBarView barView,
            out TMP_Text nameText,
            out TMP_Text roleText)
        {
            RectTransform panel = CreateAnchored(
                "PlayerPanel",
                parent,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(36f, -28f),
                new Vector2(590f, 124f),
                new Vector2(0f, 1f));
            AddPanelImage(panel, panelSprite, WithAlpha(PlayerDark, 0.92f), WithAlpha(PlayerCyan, 0.72f));

            RectTransform accent = CreateRect(
                "PlayerAccent",
                panel,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -3f),
                new Vector2(-82f, 0f));
            AddImage(accent, panelSprite, PlayerCyan, Image.Type.Sliced, false);
            accent.localRotation = Quaternion.Euler(0f, 0f, -0.8f);

            BuildPortrait("PlayerPortrait", panel, panelSprite, portrait, false);
            nameText = CreateText(
                "PlayerName",
                panel,
                "ELLEN",
                font,
                30f,
                TextAlignmentOptions.MidlineLeft,
                PrimaryText,
                new Vector2(0f, 1f),
                new Vector2(0.75f, 1f),
                new Vector2(126f, -13f),
                new Vector2(-10f, -49f));
            nameText.characterSpacing = 3f;
            nameText.fontStyle = FontStyles.Bold;

            roleText = CreateText(
                "PlayerRole",
                panel,
                "PLAYER // VANGUARD",
                font,
                13f,
                TextAlignmentOptions.MidlineLeft,
                PlayerLight,
                new Vector2(0f, 1f),
                new Vector2(0.75f, 1f),
                new Vector2(129f, -50f),
                new Vector2(-10f, -71f));
            roleText.characterSpacing = 2f;

            HealthBarParts parts = BuildHealthBar(
                "PlayerHPBar",
                panel,
                font,
                barSprite,
                glowSprite,
                PlayerCyan,
                PlayerLight,
                false,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(128f, -80f),
                new Vector2(434f, 18f));
            barView = parts.View;

            RectTransform diamond = CreateAnchored(
                "PlayerDiamond",
                panel,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(565f, -95f),
                new Vector2(13f, 13f),
                new Vector2(0.5f, 0.5f));
            AddImage(diamond, diamondSprite, PlayerLight, Image.Type.Simple, false);
        }

        private static void BuildMonsterPanel(
            Transform parent,
            TMP_FontAsset font,
            Sprite panelSprite,
            Sprite barSprite,
            Sprite glowSprite,
            Sprite diamondSprite,
            Sprite portrait,
            out CombatantHealthBarView barView,
            out TMP_Text nameText,
            out TMP_Text roleText)
        {
            RectTransform panel = CreateAnchored(
                "MonsterPanel",
                parent,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-36f, -28f),
                new Vector2(590f, 124f),
                new Vector2(1f, 1f));
            AddPanelImage(panel, panelSprite, WithAlpha(MonsterDark, 0.92f), WithAlpha(MonsterRed, 0.75f));

            RectTransform accent = CreateRect(
                "MonsterAccent",
                panel,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(82f, -3f),
                new Vector2(0f, 0f));
            AddImage(accent, panelSprite, MonsterRed, Image.Type.Sliced, false);
            accent.localRotation = Quaternion.Euler(0f, 0f, 0.8f);

            BuildPortrait("MonsterPortrait", panel, panelSprite, portrait, true);
            nameText = CreateText(
                "MonsterName",
                panel,
                "CHOMPER",
                font,
                30f,
                TextAlignmentOptions.MidlineRight,
                PrimaryText,
                new Vector2(0.25f, 1f),
                new Vector2(1f, 1f),
                new Vector2(10f, -13f),
                new Vector2(-126f, -49f));
            nameText.characterSpacing = 3f;
            nameText.fontStyle = FontStyles.Bold;

            roleText = CreateText(
                "MonsterRole",
                panel,
                "MONSTER // APEX PREDATOR",
                font,
                13f,
                TextAlignmentOptions.MidlineRight,
                MonsterLight,
                new Vector2(0.25f, 1f),
                new Vector2(1f, 1f),
                new Vector2(10f, -50f),
                new Vector2(-129f, -71f));
            roleText.characterSpacing = 2f;

            HealthBarParts parts = BuildHealthBar(
                "MonsterHPBar",
                panel,
                font,
                barSprite,
                glowSprite,
                MonsterRed,
                MonsterLight,
                true,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(28f, -80f),
                new Vector2(434f, 18f));
            barView = parts.View;

            RectTransform diamond = CreateAnchored(
                "MonsterDiamond",
                panel,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-565f, -95f),
                new Vector2(13f, 13f),
                new Vector2(0.5f, 0.5f));
            AddImage(diamond, diamondSprite, MonsterLight, Image.Type.Simple, false);
        }

        private static void BuildPortrait(
            string name,
            Transform parent,
            Sprite panelSprite,
            Sprite portrait,
            bool right)
        {
            RectTransform frame = CreateAnchored(
                name,
                parent,
                right ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f),
                right ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f),
                right ? new Vector2(-13f, 0f) : new Vector2(13f, 0f),
                new Vector2(98f, 98f),
                right ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f));

            Image frameImage = AddImage(frame, panelSprite, new Color(0.02f, 0.035f, 0.05f, 0.98f), Image.Type.Sliced, false);
            Mask mask = frame.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            RectTransform imageRect = CreateRect(
                "PortraitImage",
                frame,
                new Vector2(0.06f, 0.06f),
                new Vector2(0.94f, 0.94f),
                Vector2.zero,
                Vector2.zero);
            Image image = AddImage(imageRect, portrait, Color.white, Image.Type.Simple, false);
            image.preserveAspect = true;

            Outline outline = frame.gameObject.AddComponent<Outline>();
            outline.effectColor = right ? WithAlpha(MonsterLight, 0.62f) : WithAlpha(PlayerLight, 0.62f);
            outline.effectDistance = new Vector2(1.2f, -1.2f);
        }

        private static HealthBarParts BuildHealthBar(
            string name,
            Transform parent,
            TMP_FontAsset font,
            Sprite barSprite,
            Sprite glowSprite,
            Color mainColor,
            Color lightColor,
            bool fromRight,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 position,
            Vector2 size)
        {
            RectTransform bar = CreateAnchored(name, parent, anchorMin, anchorMax, position, size, new Vector2(0f, 1f));
            Image background = AddImage(bar, barSprite, new Color(0.008f, 0.014f, 0.02f, 0.9f), Image.Type.Sliced, false);

            RectTransform glowRect = CreateRect(
                "HP_Glow",
                bar,
                Vector2.zero,
                Vector2.one,
                new Vector2(-10f, -9f),
                new Vector2(10f, 9f));
            Image glow = AddImage(glowRect, glowSprite, WithAlpha(mainColor, 0.3f), Image.Type.Simple, false);
            glowRect.SetSiblingIndex(0);

            RectTransform delayedRect = CreateRect("HP_DelayedFill", bar, Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-2f, -2f));
            Image delayed = AddImage(delayedRect, barSprite, WithAlpha(lightColor, 0.82f), Image.Type.Filled, false);
            delayed.fillMethod = Image.FillMethod.Horizontal;
            delayed.fillOrigin = fromRight ? 1 : 0;

            RectTransform fillRect = CreateRect("HP_Fill", bar, Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-2f, -2f));
            Image fill = AddImage(fillRect, barSprite, mainColor, Image.Type.Filled, false);
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = fromRight ? 1 : 0;

            RectTransform highlight = CreateRect(
                "HP_Highlight",
                bar,
                new Vector2(0f, 0.58f),
                new Vector2(1f, 0.84f),
                new Vector2(5f, 0f),
                new Vector2(-5f, 0f));
            AddImage(highlight, barSprite, WithAlpha(Color.white, 0.16f), Image.Type.Sliced, false);

            RectTransform flashRect = CreateRect("HP_Flash", bar, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image flash = AddImage(flashRect, barSprite, new Color(1f, 1f, 1f, 0f), Image.Type.Sliced, false);

            TMP_Text value = CreateText(
                "HP_Value",
                bar,
                "100 / 100",
                font,
                15f,
                TextAlignmentOptions.MidlineLeft,
                PrimaryText,
                new Vector2(0f, 0f),
                new Vector2(0.55f, 1f),
                new Vector2(8f, 2f),
                new Vector2(0f, -2f));
            value.characterSpacing = 0.8f;

            TMP_Text percent = CreateText(
                "HP_Percent",
                bar,
                "100%",
                font,
                14f,
                TextAlignmentOptions.MidlineRight,
                lightColor,
                new Vector2(0.55f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 2f),
                new Vector2(-8f, -2f));

            var view = bar.gameObject.AddComponent<CombatantHealthBarView>();
            view.Configure(fill, delayed, glow, flash, value, percent, fromRight);
            return new HealthBarParts
            {
                View = view,
                Fill = fill,
                DelayedFill = delayed,
                Glow = glow,
                Flash = flash,
                Value = value,
                Percent = percent
            };
        }

        private static MatchStatusView BuildMatchStatus(Transform parent, TMP_FontAsset font, Sprite panelSprite, Sprite diamond)
        {
            RectTransform status = CreateAnchored(
                "MatchStatus",
                parent,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -28f),
                new Vector2(370f, 92f),
                new Vector2(0.5f, 1f));
            AddPanelImage(status, panelSprite, new Color(0.018f, 0.03f, 0.043f, 0.82f), WithAlpha(VictoryGold, 0.48f));

            TMP_Text round = CreateText(
                "RoundLabel",
                status,
                "ROUND 01",
                font,
                12f,
                TextAlignmentOptions.Center,
                SecondaryText,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -7f),
                new Vector2(0f, -24f));
            round.characterSpacing = 3.5f;

            TMP_Text title = CreateText(
                "MatchStatusTitle",
                status,
                "STANDBY",
                font,
                25f,
                TextAlignmentOptions.Center,
                PrimaryText,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -25f),
                new Vector2(0f, -56f));
            title.characterSpacing = 2.2f;
            title.fontStyle = FontStyles.Bold;

            TMP_Text subtitle = CreateText(
                "MatchStatusSubtitle",
                status,
                "PREPARE FOR COMBAT",
                font,
                11f,
                TextAlignmentOptions.Center,
                SecondaryText,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -57f),
                new Vector2(0f, -76f));
            subtitle.characterSpacing = 1.6f;

            RectTransform mark = CreateAnchored(
                "StatusDiamond",
                status,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 8f),
                new Vector2(10f, 10f),
                new Vector2(0.5f, 0.5f));
            AddImage(mark, diamond, VictoryGold, Image.Type.Simple, false);

            var view = status.gameObject.AddComponent<MatchStatusView>();
            view.Configure(title, subtitle, round);
            return view;
        }

        private static MatchResultView BuildResultPanel(
            Transform parent,
            TMP_FontAsset font,
            Sprite panelSprite,
            Sprite diamond)
        {
            RectTransform panel = CreateRect("ResultPanel", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CanvasGroup group = panel.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            HUDPanelAnimator animator = panel.gameObject.AddComponent<HUDPanelAnimator>();
            animator.Configure(group, panel);

            RectTransform dim = CreateRect("DimBackground", panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddImage(dim, panelSprite, new Color(0.005f, 0.008f, 0.012f, 0.62f), Image.Type.Sliced, true);

            RectTransform card = CreateAnchored(
                "ResultCard",
                panel,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(760f, 470f),
                new Vector2(0.5f, 0.5f));
            AddPanelImage(card, panelSprite, new Color(0.018f, 0.026f, 0.036f, 0.98f), WithAlpha(VictoryGold, 0.84f));

            BuildResultDecorations(card, panelSprite, diamond);
            TMP_Text title = CreateText(
                "ResultTitle",
                card,
                "PLAYER WINS",
                font,
                54f,
                TextAlignmentOptions.Center,
                VictoryGold,
                new Vector2(0.08f, 0.58f),
                new Vector2(0.92f, 0.84f),
                Vector2.zero,
                Vector2.zero);
            title.characterSpacing = 5f;
            title.fontStyle = FontStyles.Bold;
            AddTextShadow(title, WithAlpha(VictoryGold, 0.35f), new Vector2(0f, -2f));

            TMP_Text subtitle = CreateText(
                "ResultSubtitle",
                card,
                "YOU DEFEATED THE CHOMPER",
                font,
                17f,
                TextAlignmentOptions.Center,
                SecondaryText,
                new Vector2(0.08f, 0.45f),
                new Vector2(0.92f, 0.58f),
                Vector2.zero,
                Vector2.zero);
            subtitle.characterSpacing = 2.5f;

            ButtonParts restart = CreateButton(
                "RestartButton",
                card,
                "RESTART",
                font,
                panelSprite,
                new Vector2(0.5f - 0.255f, 0.15f),
                new Vector2(0.5f - 0.015f, 0.30f),
                VictoryGold,
                new Color(0.09f, 0.07f, 0.03f, 0.96f));
            ButtonParts menu = CreateButton(
                "MainMenuButton",
                card,
                "MAIN MENU",
                font,
                panelSprite,
                new Vector2(0.5f + 0.015f, 0.15f),
                new Vector2(0.5f + 0.255f, 0.30f),
                WithAlpha(PlayerCyan, 0.58f),
                new Color(0.025f, 0.055f, 0.075f, 0.96f));
            menu.Button.interactable = false;

            MatchResultView view = panel.gameObject.AddComponent<MatchResultView>();
            view.Configure(animator, title, subtitle, restart.Button, menu.Button, menu.Label);
            panel.gameObject.SetActive(false);
            return view;
        }

        private static PauseMenuView BuildPausePanel(
            Transform parent,
            TMP_FontAsset font,
            Sprite panelSprite,
            Sprite diamond)
        {
            RectTransform panel = CreateRect("PausePanel", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CanvasGroup group = panel.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            HUDPanelAnimator animator = panel.gameObject.AddComponent<HUDPanelAnimator>();
            animator.Configure(group, panel);

            RectTransform dim = CreateRect("DimBackground", panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddImage(dim, panelSprite, new Color(0.005f, 0.008f, 0.012f, 0.54f), Image.Type.Sliced, true);

            RectTransform card = CreateAnchored(
                "PauseCard",
                panel,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(480f, 390f),
                new Vector2(0.5f, 0.5f));
            AddPanelImage(card, panelSprite, new Color(0.018f, 0.03f, 0.042f, 0.98f), WithAlpha(PlayerCyan, 0.72f));

            RectTransform topLine = CreateRect("TopLine", card, new Vector2(0.1f, 1f), new Vector2(0.9f, 1f), new Vector2(0f, -16f), new Vector2(0f, -14f));
            AddImage(topLine, panelSprite, WithAlpha(PlayerCyan, 0.75f), Image.Type.Sliced, false);

            TMP_Text title = CreateText(
                "PauseTitle",
                card,
                "PAUSED",
                font,
                42f,
                TextAlignmentOptions.Center,
                PrimaryText,
                new Vector2(0.08f, 0.69f),
                new Vector2(0.92f, 0.87f),
                Vector2.zero,
                Vector2.zero);
            title.characterSpacing = 4f;
            title.fontStyle = FontStyles.Bold;

            RectTransform mark = CreateAnchored(
                "PauseDiamond",
                card,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -13f),
                new Vector2(10f, 10f),
                new Vector2(0.5f, 0.5f));
            AddImage(mark, diamond, VictoryGold, Image.Type.Simple, false);

            ButtonParts resume = CreateButton(
                "ResumeButton",
                card,
                "RESUME",
                font,
                panelSprite,
                new Vector2(0.18f, 0.48f),
                new Vector2(0.82f, 0.60f),
                VictoryGold,
                new Color(0.07f, 0.065f, 0.04f, 0.96f));
            ButtonParts restart = CreateButton(
                "RestartButton",
                card,
                "RESTART",
                font,
                panelSprite,
                new Vector2(0.18f, 0.33f),
                new Vector2(0.82f, 0.45f),
                PlayerCyan,
                new Color(0.025f, 0.055f, 0.075f, 0.96f));
            ButtonParts exit = CreateButton(
                "ExitButton",
                card,
                "EXIT // EDITOR",
                font,
                panelSprite,
                new Vector2(0.18f, 0.18f),
                new Vector2(0.82f, 0.30f),
                WithAlpha(SecondaryText, 0.65f),
                new Color(0.025f, 0.038f, 0.05f, 0.96f));

            PauseMenuView view = panel.gameObject.AddComponent<PauseMenuView>();
            view.Configure(animator, resume.Button, restart.Button, exit.Button, exit.Label);
            panel.gameObject.SetActive(false);
            return view;
        }

        private static ControlHintView BuildControlBar(
            Transform parent,
            TMP_FontAsset font,
            Sprite panelSprite,
            Sprite keycapSprite)
        {
            RectTransform bar = CreateAnchored(
                "BottomControlBar",
                parent,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 18f),
                new Vector2(1240f, 72f),
                new Vector2(0.5f, 0f));
            AddPanelImage(bar, panelSprite, new Color(0.018f, 0.032f, 0.045f, 0.86f), WithAlpha(PlayerCyan, 0.32f));

            BuildControlGroup("MoveHint", bar, font, panelSprite, keycapSprite, 0, "W A S D", "MOVE",
                out TMP_Text moveKeys, out TMP_Text moveLabel);
            BuildControlGroup("CameraHint", bar, font, panelSprite, keycapSprite, 1, "MOUSE", "LOOK",
                out TMP_Text cameraKeys, out TMP_Text cameraLabel);
            BuildControlGroup("LeftPunchHint", bar, font, panelSprite, keycapSprite, 2, "LMB", "LEFT PUNCH",
                out TMP_Text leftKeys, out TMP_Text leftLabel);
            BuildControlGroup("RightPunchHint", bar, font, panelSprite, keycapSprite, 3, "RMB", "RIGHT PUNCH",
                out TMP_Text rightKeys, out TMP_Text rightLabel);
            BuildControlGroup("GuardHint", bar, font, panelSprite, keycapSprite, 4, "L-SHIFT", "GUARD",
                out TMP_Text guardKeys, out TMP_Text guardLabel);
            BuildControlGroup("PauseHint", bar, font, panelSprite, keycapSprite, 5, "ESC", "PAUSE",
                out TMP_Text pauseKeys, out TMP_Text pauseLabel);

            ControlHintView view = bar.gameObject.AddComponent<ControlHintView>();
            view.Configure(
                moveKeys,
                moveLabel,
                cameraKeys,
                cameraLabel,
                leftKeys,
                leftLabel,
                rightKeys,
                rightLabel,
                guardKeys,
                guardLabel,
                pauseKeys,
                pauseLabel);
            return view;
        }

        private static void BuildControlGroup(
            string name,
            Transform parent,
            TMP_FontAsset font,
            Sprite panelSprite,
            Sprite keycapSprite,
            int index,
            string keyText,
            string labelText,
            out TMP_Text keys,
            out TMP_Text label)
        {
            float width = 1f / 6f;
            RectTransform group = CreateRect(
                name,
                parent,
                new Vector2(width * index, 0f),
                new Vector2(width * (index + 1), 1f),
                new Vector2(6f, 5f),
                new Vector2(-6f, -5f));

            RectTransform keyFrame = CreateAnchored(
                "Keycap",
                group,
                new Vector2(0.5f, 0.65f),
                new Vector2(0.5f, 0.65f),
                Vector2.zero,
                new Vector2(88f, 28f),
                new Vector2(0.5f, 0.5f));
            AddImage(keyFrame, keycapSprite, new Color(0.055f, 0.10f, 0.13f, 0.98f), Image.Type.Sliced, false);
            Outline outline = keyFrame.gameObject.AddComponent<Outline>();
            outline.effectColor = WithAlpha(PlayerLight, 0.42f);
            outline.effectDistance = new Vector2(1f, -1f);

            keys = CreateText(
                "Keys",
                keyFrame,
                keyText,
                font,
                12f,
                TextAlignmentOptions.Center,
                PrimaryText,
                Vector2.zero,
                Vector2.one,
                new Vector2(3f, 1f),
                new Vector2(-3f, -1f));
            keys.characterSpacing = 1.8f;
            keys.fontStyle = FontStyles.Bold;

            label = CreateText(
                "Action",
                group,
                labelText,
                font,
                10f,
                TextAlignmentOptions.Center,
                SecondaryText,
                new Vector2(0f, 0f),
                new Vector2(1f, 0.4f),
                Vector2.zero,
                Vector2.zero);
            label.characterSpacing = 1.4f;
        }

        private static void BuildCombatFeedback(
            Transform parent,
            TMP_FontAsset font,
            Sprite panelSprite,
            out CanvasGroup guardGroup,
            out TMP_Text guardText,
            out CanvasGroup hitGroup,
            out TMP_Text hitText,
            out CanvasGroup lowHpGroup)
        {
            RectTransform feedback = CreateRect("CombatFeedback", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            guardGroup = BuildFeedbackBadge(
                "GuardFeedback",
                feedback,
                font,
                panelSprite,
                "GUARD",
                new Vector2(0.5f, 0.35f),
                new Vector2(220f, 48f),
                PlayerLight,
                out guardText);
            hitGroup = BuildFeedbackBadge(
                "HitFeedback",
                feedback,
                font,
                panelSprite,
                "HIT",
                new Vector2(0.5f, 0.61f),
                new Vector2(180f, 38f),
                MonsterLight,
                out hitText);

            RectTransform low = CreateAnchored(
                "LowHPWarning",
                feedback,
                new Vector2(0.5f, 0.13f),
                new Vector2(0.5f, 0.13f),
                Vector2.zero,
                new Vector2(330f, 34f),
                new Vector2(0.5f, 0.5f));
            AddPanelImage(low, panelSprite, new Color(0.12f, 0.018f, 0.028f, 0.84f), WithAlpha(MonsterRed, 0.6f));
            lowHpGroup = low.gameObject.AddComponent<CanvasGroup>();
            lowHpGroup.alpha = 0f;
            lowHpGroup.interactable = false;
            lowHpGroup.blocksRaycasts = false;
            TMP_Text warning = CreateText(
                "Label",
                low,
                "WARNING // VITALS CRITICAL",
                font,
                12f,
                TextAlignmentOptions.Center,
                MonsterLight,
                Vector2.zero,
                Vector2.one,
                new Vector2(4f, 2f),
                new Vector2(-4f, -2f));
            warning.characterSpacing = 2.2f;
            warning.fontStyle = FontStyles.Bold;
        }

        private static CanvasGroup BuildFeedbackBadge(
            string name,
            Transform parent,
            TMP_FontAsset font,
            Sprite panelSprite,
            string text,
            Vector2 anchor,
            Vector2 size,
            Color accent,
            out TMP_Text textComponent)
        {
            RectTransform badge = CreateAnchored(name, parent, anchor, anchor, Vector2.zero, size, new Vector2(0.5f, 0.5f));
            AddPanelImage(badge, panelSprite, new Color(0.012f, 0.022f, 0.03f, 0.88f), WithAlpha(accent, 0.66f));
            CanvasGroup group = badge.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            textComponent = CreateText(
                "Label",
                badge,
                text,
                font,
                21f,
                TextAlignmentOptions.Center,
                accent,
                Vector2.zero,
                Vector2.one,
                new Vector2(4f, 1f),
                new Vector2(-4f, -1f));
            textComponent.characterSpacing = 3f;
            textComponent.fontStyle = FontStyles.Bold;
            return group;
        }

        private static ButtonParts CreateButton(
            string name,
            Transform parent,
            string label,
            TMP_FontAsset font,
            Sprite panelSprite,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color borderColor,
            Color fillColor)
        {
            RectTransform border = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Image borderImage = AddImage(border, panelSprite, borderColor, Image.Type.Sliced, true);
            Button button = border.gameObject.AddComponent<Button>();
            button.targetGraphic = borderImage;
            button.transition = Selectable.Transition.None;

            RectTransform inner = CreateRect(
                "Fill",
                border,
                Vector2.zero,
                Vector2.one,
                new Vector2(2f, 2f),
                new Vector2(-2f, -2f));
            AddImage(inner, panelSprite, fillColor, Image.Type.Sliced, false);

            TMP_Text labelText = CreateText(
                "Label",
                border,
                label,
                font,
                18f,
                TextAlignmentOptions.Center,
                borderColor,
                Vector2.zero,
                Vector2.one,
                new Vector2(8f, 2f),
                new Vector2(-8f, -2f));
            labelText.characterSpacing = 2.6f;
            labelText.fontStyle = FontStyles.Bold;

            HUDButtonMotion motion = border.gameObject.AddComponent<HUDButtonMotion>();
            motion.Configure(
                border,
                borderImage,
                borderColor,
                Color.Lerp(borderColor, Color.white, 0.28f),
                Color.white);

            return new ButtonParts { Button = button, Label = labelText };
        }

        private static void BuildResultDecorations(Transform card, Sprite panelSprite, Sprite diamond)
        {
            CreateDecorativeLine("GoldLineTop", card, panelSprite, new Vector2(0.08f, 0.86f), new Vector2(0.92f, 0.86f), VictoryGold);
            CreateDecorativeLine("GoldLineBottom", card, panelSprite, new Vector2(0.16f, 0.40f), new Vector2(0.84f, 0.40f), VictoryGold);
            CreateDecorativeLine("CyanLineLeft", card, panelSprite, new Vector2(0.08f, 0.28f), new Vector2(0.24f, 0.28f), PlayerCyan);
            CreateDecorativeLine("CyanLineRight", card, panelSprite, new Vector2(0.76f, 0.28f), new Vector2(0.92f, 0.28f), PlayerCyan);

            for (int i = 0; i < 5; i++)
            {
                RectTransform mark = CreateAnchored(
                    "Diamond" + i,
                    card,
                    new Vector2(0.5f, 0.87f),
                    new Vector2(0.5f, 0.87f),
                    new Vector2((i - 2) * 22f, 0f),
                    new Vector2(i == 2 ? 14f : 8f, i == 2 ? 14f : 8f),
                    new Vector2(0.5f, 0.5f));
                AddImage(mark, diamond, i == 2 ? VictoryGold : WithAlpha(VictoryGold, 0.42f), Image.Type.Simple, false);
            }
        }

        private static void CreateDecorativeLine(
            string name,
            Transform parent,
            Sprite sprite,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color)
        {
            RectTransform line = CreateRect(name, parent, anchorMin, anchorMax, new Vector2(0f, -0.75f), new Vector2(0f, 0.75f));
            AddImage(line, sprite, color, Image.Type.Sliced, false);
        }

        private static void IntegratePrefabIntoScene(GameObject prefab)
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject canvasObject = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == "Canvas_CombatHUD" || root.name == "HealthCanvas")
                {
                    canvasObject = root;
                    break;
                }
            }

            if (canvasObject == null)
            {
                Debug.LogError("Could not find the existing combat HealthCanvas.");
                return;
            }

            canvasObject.name = "Canvas_CombatHUD";
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            EditorUtility.SetDirty(canvasObject);
            EditorUtility.SetDirty(canvas);
            EditorUtility.SetDirty(scaler);

            foreach (MatchHUD oldHud in canvasObject.GetComponents<MatchHUD>())
                Object.DestroyImmediate(oldHud);

            var staleHudRoots = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < canvasObject.transform.childCount; i++)
            {
                GameObject child = canvasObject.transform.GetChild(i).gameObject;
                GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(child);
                if (child.name == "HUD_Root" ||
                    child.name.StartsWith("PF_CombatHUD_V2") ||
                    child.GetComponent<MatchHUD>() != null ||
                    source != null && AssetDatabase.GetAssetPath(source) == PrefabPath)
                {
                    staleHudRoots.Add(child);
                }
            }
            for (int i = 0; i < staleHudRoots.Count; i++)
                Object.DestroyImmediate(staleHudRoots[i]);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvasObject.transform);
            instance.name = "HUD_Root";
            var instanceRect = (RectTransform)instance.transform;
            instanceRect.anchorMin = Vector2.zero;
            instanceRect.anchorMax = Vector2.one;
            instanceRect.offsetMin = Vector2.zero;
            instanceRect.offsetMax = Vector2.zero;

            MatchHUD hud = instance.GetComponent<MatchHUD>();
            MatchController controller = Object.FindFirstObjectByType<MatchController>(FindObjectsInactive.Include);
            if (hud != null && controller != null)
            {
                SerializedObject serializedHud = new SerializedObject(hud);
                serializedHud.FindProperty("matchController").objectReferenceValue = controller;
                serializedHud.ApplyModifiedPropertiesWithoutUndo();
            }

            foreach (MonoBehaviour behaviour in canvasObject.GetComponents<MonoBehaviour>())
            {
                if (behaviour != null && behaviour.GetType().Name == "HealthUI")
                    behaviour.enabled = false;
            }

            EnsureSingleEventSystem(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureSingleEventSystem(Scene scene)
        {
            EventSystem[] systems = Object.FindObjectsByType<EventSystem>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            EventSystem selected = null;
            for (int i = 0; i < systems.Length; i++)
            {
                if (systems[i] == null || systems[i].gameObject.scene != scene)
                    continue;

                if (systems[i].transform.parent == null && systems[i].gameObject.activeInHierarchy)
                    selected = systems[i];
            }

            if (selected == null)
            {
                for (int i = 0; i < systems.Length; i++)
                {
                    if (systems[i] != null && systems[i].gameObject.scene == scene)
                        Object.DestroyImmediate(systems[i].gameObject);
                }

                var eventSystemObject = new GameObject(
                    "EventSystem",
                    typeof(EventSystem),
                    typeof(StandaloneInputModule));
                selected = eventSystemObject.GetComponent<EventSystem>();
            }

            selected.name = "EventSystem";
            selected.gameObject.SetActive(true);

            for (int i = 0; i < systems.Length; i++)
            {
                if (systems[i] != null && systems[i] != selected && systems[i].gameObject.scene == scene)
                    Object.DestroyImmediate(systems[i].gameObject);
            }
        }

        private static Image AddPanelImage(RectTransform rect, Sprite sprite, Color fill, Color outline)
        {
            Image image = AddImage(rect, sprite, fill, Image.Type.Sliced, false);
            Outline border = rect.gameObject.AddComponent<Outline>();
            border.effectColor = outline;
            border.effectDistance = new Vector2(1.1f, -1.1f);
            return image;
        }

        private static Image AddImage(
            RectTransform rect,
            Sprite sprite,
            Color color,
            Image.Type type,
            bool raycastTarget)
        {
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.type = type;
            image.raycastTarget = raycastTarget;
            if (type == Image.Type.Sliced)
                image.pixelsPerUnitMultiplier = 1f;
            return image;
        }

        private static TMP_Text CreateText(
            string name,
            Transform parent,
            string value,
            TMP_FontAsset font,
            float fontSize,
            TextAlignmentOptions alignment,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = false;
            text.richText = true;
            return text;
        }

        private static void AddTextShadow(TMP_Text text, Color color, Vector2 distance)
        {
            Shadow shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        private static RectTransform CreateRect(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        private static RectTransform CreateAnchored(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Vector2 pivot)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            rect.pivot = pivot;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPosition;
            return rect;
        }

        private static Sprite CreateRoundedSprite(string path, int size, int radius)
        {
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) == null)
            {
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float px = Mathf.Abs(x - size * 0.5f) - (size * 0.5f - radius);
                        float py = Mathf.Abs(y - size * 0.5f) - (size * 0.5f - radius);
                        float outside = Mathf.Sqrt(Mathf.Max(px, 0f) * Mathf.Max(px, 0f) +
                                                   Mathf.Max(py, 0f) * Mathf.Max(py, 0f));
                        float signedDistance = outside + Mathf.Min(Mathf.Max(px, py), 0f) - radius;
                        float alpha = Mathf.Clamp01(0.7f - signedDistance * 0.7f);
                        texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                }
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
            }

            ImportSprite(path, new Vector4(radius, radius, radius, radius), false);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite CreateBarSprite(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) == null)
            {
                const int width = 64;
                const int height = 24;
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                for (int y = 0; y < height; y++)
                {
                    float vertical = Mathf.Lerp(0.72f, 1f, y / (height - 1f));
                    for (int x = 0; x < width; x++)
                    {
                        float edge = Mathf.Clamp01(Mathf.Min(x, width - 1 - x) / 3f);
                        float cap = Mathf.Clamp01(Mathf.Min(y, height - 1 - y) / 4f);
                        texture.SetPixel(x, y, new Color(1f, 1f, 1f, vertical * Mathf.Min(edge, cap)));
                    }
                }
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
            }

            ImportSprite(path, new Vector4(6f, 6f, 6f, 6f), false);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite CreateGlowSprite(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) == null)
            {
                const int size = 64;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        Vector2 center = new Vector2((x + 0.5f) / size - 0.5f, (y + 0.5f) / size - 0.5f);
                        float alpha = Mathf.Exp(-Vector2.Dot(center, center) * 18f);
                        texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                }
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
            }

            ImportSprite(path, Vector4.zero, false);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite CreateDiamondSprite(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) == null)
            {
                const int size = 32;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float distance = Mathf.Abs(x - (size - 1) * 0.5f) +
                                         Mathf.Abs(y - (size - 1) * 0.5f);
                        float alpha = Mathf.Clamp01((size * 0.42f - distance) * 0.65f);
                        texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                }
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
            }

            ImportSprite(path, Vector4.zero, false);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void ImportSprite(string path, Vector4 border, bool tight)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = border;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 256;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spritePixelsPerUnit = 100f;
            settings.spriteExtrude = 1;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out Color color);
            return color;
        }
    }
}
