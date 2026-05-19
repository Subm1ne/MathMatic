using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using TMPro;

public class SceneSetup : Editor
{
    static readonly Color chalkWhite = new Color(0.95f, 0.95f, 0.9f);
    static readonly Color boardGreen = new Color(0.15f, 0.35f, 0.15f);
    static readonly Color buttonColor = new Color(0.22f, 0.22f, 0.18f, 0.85f);
    static readonly Color panelDark = new Color(0.1f, 0.1f, 0.08f, 0.92f);

    [MenuItem("Toplama Oyunu/Oyun Sahnesini Kur")]
    public static void SetupGameScene()
    {
        // --- Canvas ---
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.AddComponent<UIButtonSoundManager>();

        // --- Background ---
        GameObject bgGO = CreateImage(canvasGO, "Background", boardGreen);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        StretchFull(bgRect);

        // --- Chalkboard Image ---
        GameObject boardGO = CreateImage(canvasGO, "ChalkboardImage", Color.white);
        RectTransform boardRect = boardGO.GetComponent<RectTransform>();
        boardRect.anchorMin = new Vector2(0.1f, 0.3f);
        boardRect.anchorMax = new Vector2(0.9f, 0.95f);
        boardRect.offsetMin = Vector2.zero;
        boardRect.offsetMax = Vector2.zero;

        string[] guids = AssetDatabase.FindAssets("chalkboard t:Texture2D", new[] { "Assets/Images" });
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            Sprite boardSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (boardSprite != null)
                boardGO.GetComponent<Image>().sprite = boardSprite;
        }

        // --- Question Text ---
        GameObject questionGO = CreateTMPText(boardGO, "QuestionText", "3 + 5 = ?", 72, chalkWhite);
        RectTransform qRect = questionGO.GetComponent<RectTransform>();
        qRect.anchorMin = new Vector2(0.1f, 0.3f);
        qRect.anchorMax = new Vector2(0.9f, 0.75f);
        qRect.offsetMin = Vector2.zero;
        qRect.offsetMax = Vector2.zero;

        // --- Timer Text ---
        GameObject timerGO = CreateTMPText(boardGO, "TimerText", "30", 48, chalkWhite);
        RectTransform tRect = timerGO.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0.8f, 0.78f);
        tRect.anchorMax = new Vector2(0.95f, 0.98f);
        tRect.offsetMin = Vector2.zero;
        tRect.offsetMax = Vector2.zero;

        // --- Level Info Text ---
        GameObject levelGO = CreateTMPText(boardGO, "LevelInfoText", "Bolum 1 - Soru 1/5", 32, chalkWhite);
        RectTransform lRect = levelGO.GetComponent<RectTransform>();
        lRect.anchorMin = new Vector2(0.05f, 0.78f);
        lRect.anchorMax = new Vector2(0.5f, 0.98f);
        lRect.offsetMin = Vector2.zero;
        lRect.offsetMax = Vector2.zero;
        levelGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;

        // --- Options Panel ---
        GameObject optionsPanel = new GameObject("OptionsPanel");
        optionsPanel.transform.SetParent(canvasGO.transform, false);
        RectTransform opRect = optionsPanel.AddComponent<RectTransform>();
        opRect.anchorMin = new Vector2(0.1f, 0.05f);
        opRect.anchorMax = new Vector2(0.9f, 0.28f);
        opRect.offsetMin = Vector2.zero;
        opRect.offsetMax = Vector2.zero;
        HorizontalLayoutGroup hlg = optionsPanel.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.padding = new RectOffset(10, 10, 10, 10);

        string[] labels = { "A", "B", "C", "D" };
        for (int i = 0; i < 4; i++)
        {
            CreateOptionButton(optionsPanel, "Option" + labels[i], labels[i] + ") 8");
        }

        // --- Feedback Panel ---
        GameObject feedbackPanel = CreatePanel(canvasGO, "FeedbackPanel", new Color(0, 0, 0, 0.6f));
        RectTransform fpRect = feedbackPanel.GetComponent<RectTransform>();
        fpRect.anchorMin = new Vector2(0.25f, 0.4f);
        fpRect.anchorMax = new Vector2(0.75f, 0.6f);
        fpRect.offsetMin = Vector2.zero;
        fpRect.offsetMax = Vector2.zero;

        GameObject feedbackTextGO = CreateTMPText(feedbackPanel, "FeedbackText", "Dogru!", 52, chalkWhite);
        StretchFull(feedbackTextGO.GetComponent<RectTransform>());
        feedbackPanel.SetActive(false);

        // --- Level Complete Panel ---
        GameObject lcPanel = CreatePanel(canvasGO, "LevelCompletePanel", panelDark);
        StretchFull(lcPanel.GetComponent<RectTransform>());

        GameObject lcTitle = CreateTMPText(lcPanel, "LevelCompleteTitle", "Bolum Tamamlandi!", 56, chalkWhite);
        RectTransform lcTitleRect = lcTitle.GetComponent<RectTransform>();
        lcTitleRect.anchorMin = new Vector2(0.1f, 0.7f);
        lcTitleRect.anchorMax = new Vector2(0.9f, 0.9f);
        lcTitleRect.offsetMin = Vector2.zero;
        lcTitleRect.offsetMax = Vector2.zero;

        GameObject lcResult = CreateTMPText(lcPanel, "LevelResultText", "4 / 5 Dogru", 42, chalkWhite);
        RectTransform lcResRect = lcResult.GetComponent<RectTransform>();
        lcResRect.anchorMin = new Vector2(0.2f, 0.55f);
        lcResRect.anchorMax = new Vector2(0.8f, 0.7f);
        lcResRect.offsetMin = Vector2.zero;
        lcResRect.offsetMax = Vector2.zero;

        // Stars
        GameObject starsContainer = new GameObject("StarsContainer");
        starsContainer.transform.SetParent(lcPanel.transform, false);
        RectTransform starsRect = starsContainer.AddComponent<RectTransform>();
        starsRect.anchorMin = new Vector2(0.3f, 0.42f);
        starsRect.anchorMax = new Vector2(0.7f, 0.55f);
        starsRect.offsetMin = Vector2.zero;
        starsRect.offsetMax = Vector2.zero;
        HorizontalLayoutGroup starHLG = starsContainer.AddComponent<HorizontalLayoutGroup>();
        starHLG.spacing = 20;
        starHLG.childAlignment = TextAnchor.MiddleCenter;
        starHLG.childForceExpandWidth = true;
        starHLG.childForceExpandHeight = true;

        for (int i = 0; i < 3; i++)
        {
            GameObject star = CreateTMPText(starsContainer, "Star" + (i + 1), "\u2605", 64,
                new Color(1f, 0.85f, 0.2f));
        }

        // Buttons row
        GameObject btnRow = new GameObject("ButtonsRow");
        btnRow.transform.SetParent(lcPanel.transform, false);
        RectTransform btnRowRect = btnRow.AddComponent<RectTransform>();
        btnRowRect.anchorMin = new Vector2(0.15f, 0.12f);
        btnRowRect.anchorMax = new Vector2(0.85f, 0.35f);
        btnRowRect.offsetMin = Vector2.zero;
        btnRowRect.offsetMax = Vector2.zero;
        HorizontalLayoutGroup btnHLG = btnRow.AddComponent<HorizontalLayoutGroup>();
        btnHLG.spacing = 30;
        btnHLG.childAlignment = TextAnchor.MiddleCenter;
        btnHLG.childForceExpandWidth = true;
        btnHLG.childForceExpandHeight = true;

        CreateMenuButton(btnRow, "NextLevelButton", "Sonraki Bolum", new Color(0.2f, 0.6f, 0.2f));
        CreateMenuButton(btnRow, "RetryButton", "Tekrar Dene", new Color(0.6f, 0.5f, 0.1f));
        CreateMenuButton(btnRow, "MenuButton", "Ana Menu", new Color(0.5f, 0.2f, 0.2f));

        lcPanel.SetActive(false);

        // --- GameManager ---
        GameObject gmGO = new GameObject("GameManager");
        GameManager gm = gmGO.AddComponent<GameManager>();

        // --- UIManager ---
        UIManager ui = canvasGO.AddComponent<UIManager>();
        gm.uiManager = ui;

        // Wire up UI references
        ui.chalkboardImage = boardGO;
        ui.optionsPanel = optionsPanel;
        ui.questionText = questionGO.GetComponent<TextMeshProUGUI>();
        ui.timerText = timerGO.GetComponent<TextMeshProUGUI>();
        ui.levelInfoText = levelGO.GetComponent<TextMeshProUGUI>();

        ui.optionButtons = new Button[4];
        ui.optionTexts = new TextMeshProUGUI[4];
        for (int i = 0; i < 4; i++)
        {
            Transform optChild = optionsPanel.transform.GetChild(i);
            ui.optionButtons[i] = optChild.GetComponent<Button>();
            ui.optionTexts[i] = optChild.GetComponentInChildren<TextMeshProUGUI>();
        }

        ui.feedbackPanel = feedbackPanel;
        ui.feedbackText = feedbackPanel.GetComponentInChildren<TextMeshProUGUI>();

        ui.levelCompletePanel = lcPanel;
        ui.levelCompleteTitle = lcTitle.GetComponent<TextMeshProUGUI>();
        ui.levelResultText = lcResult.GetComponent<TextMeshProUGUI>();

        ui.starImages = new GameObject[3];
        for (int i = 0; i < 3; i++)
            ui.starImages[i] = starsContainer.transform.GetChild(i).gameObject;

        ui.nextLevelButton = btnRow.transform.Find("NextLevelButton").GetComponent<Button>();
        ui.retryButton = btnRow.transform.Find("RetryButton").GetComponent<Button>();
        ui.menuButton = btnRow.transform.Find("MenuButton").GetComponent<Button>();

        // --- EventSystem ---
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Oyun sahnesi basariyla kuruldu!");
    }

    [MenuItem("Toplama Oyunu/Ana Menu Sahnesini Kur")]
    public static void SetupMainMenuScene()
    {
        // --- Canvas ---
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.AddComponent<UIButtonSoundManager>();

        // --- Background ---
        GameObject bgGO = CreateImage(canvasGO, "Background", boardGreen);
        StretchFull(bgGO.GetComponent<RectTransform>());

        // --- Main Panel ---
        GameObject mainPanel = new GameObject("MainPanel");
        mainPanel.transform.SetParent(canvasGO.transform, false);
        RectTransform mpRect = mainPanel.AddComponent<RectTransform>();
        StretchFull(mpRect);

        // Title
        GameObject titleGO = CreateTMPText(mainPanel, "TitleText", "TOPLAMA OYUNU", 80, chalkWhite);
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.55f);
        titleRect.anchorMax = new Vector2(0.9f, 0.85f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        // Subtitle
        GameObject subGO = CreateTMPText(mainPanel, "SubtitleText", "Matematik Macerasi", 36, chalkWhite);
        RectTransform subRect = subGO.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0.2f, 0.45f);
        subRect.anchorMax = new Vector2(0.8f, 0.55f);
        subRect.offsetMin = Vector2.zero;
        subRect.offsetMax = Vector2.zero;

        // Play Button
        GameObject playBtn = CreateMenuButton(mainPanel, "PlayButton", "OYNA", new Color(0.2f, 0.6f, 0.2f));
        RectTransform playRect = playBtn.GetComponent<RectTransform>();
        playRect.anchorMin = new Vector2(0.3f, 0.22f);
        playRect.anchorMax = new Vector2(0.7f, 0.38f);
        playRect.offsetMin = Vector2.zero;
        playRect.offsetMax = Vector2.zero;

        // --- Level Select Panel ---
        GameObject lsPanel = new GameObject("LevelSelectPanel");
        lsPanel.transform.SetParent(canvasGO.transform, false);
        Image lsImage = lsPanel.AddComponent<Image>();
        lsImage.color = panelDark;
        StretchFull(lsPanel.GetComponent<RectTransform>());

        // Level Select Title
        GameObject lsTitle = CreateTMPText(lsPanel, "LevelSelectTitle", "Bolum Sec", 52, chalkWhite);
        RectTransform lsTitleRect = lsTitle.GetComponent<RectTransform>();
        lsTitleRect.anchorMin = new Vector2(0.2f, 0.85f);
        lsTitleRect.anchorMax = new Vector2(0.8f, 0.95f);
        lsTitleRect.offsetMin = Vector2.zero;
        lsTitleRect.offsetMax = Vector2.zero;

        // Level Grid
        GameObject gridGO = new GameObject("LevelGrid");
        gridGO.transform.SetParent(lsPanel.transform, false);
        RectTransform gridRect = gridGO.AddComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0.1f, 0.2f);
        gridRect.anchorMax = new Vector2(0.9f, 0.82f);
        gridRect.offsetMin = Vector2.zero;
        gridRect.offsetMax = Vector2.zero;
        GridLayoutGroup glg = gridGO.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(200, 120);
        glg.spacing = new Vector2(30, 25);
        glg.childAlignment = TextAnchor.MiddleCenter;
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 5;

        for (int i = 0; i < 10; i++)
        {
            CreateMenuButton(gridGO, "LevelButton_" + (i + 1), "Bolum " + (i + 1),
                new Color(0.25f, 0.45f, 0.25f));
        }

        // Back Button
        GameObject backBtn = CreateMenuButton(lsPanel, "BackButton", "Geri", new Color(0.5f, 0.2f, 0.2f));
        RectTransform backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.35f, 0.05f);
        backRect.anchorMax = new Vector2(0.65f, 0.16f);
        backRect.offsetMin = Vector2.zero;
        backRect.offsetMax = Vector2.zero;

        lsPanel.SetActive(false);

        // --- MainMenuManager ---
        MainMenuManager mmm = canvasGO.AddComponent<MainMenuManager>();
        mmm.mainPanel = mainPanel;
        mmm.playButton = playBtn.GetComponent<Button>();
        mmm.levelSelectPanel = lsPanel;
        mmm.backButton = backBtn.GetComponent<Button>();

        mmm.levelButtons = new Button[10];
        mmm.levelTexts = new TextMeshProUGUI[10];
        for (int i = 0; i < 10; i++)
        {
            Transform child = gridGO.transform.GetChild(i);
            mmm.levelButtons[i] = child.GetComponent<Button>();
            mmm.levelTexts[i] = child.GetComponentInChildren<TextMeshProUGUI>();
        }

        // --- EventSystem ---
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Ana Menu sahnesi basariyla kuruldu!");
    }

    // --- Helper Methods ---

    static GameObject CreateImage(GameObject parent, string name, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static GameObject CreateTMPText(GameObject parent, string name, string text, float fontSize, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = true;
        return go;
    }

    static GameObject CreateOptionButton(GameObject parent, string name, string text)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent.transform, false);
        RectTransform rect = btnGO.AddComponent<RectTransform>();
        Image img = btnGO.AddComponent<Image>();
        img.color = buttonColor;
        Button btn = btnGO.AddComponent<Button>();
        btnGO.AddComponent<ButtonPressScale>();
        btnGO.AddComponent<ButtonClickSound>();

        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(0.85f, 0.85f, 0.75f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.6f);
        cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        btn.colors = cb;

        CreateTMPText(btnGO, "Text", text, 36, chalkWhite);
        StretchFull(btnGO.transform.GetChild(0).GetComponent<RectTransform>());

        return btnGO;
    }

    static GameObject CreateMenuButton(GameObject parent, string name, string text, Color bgColor)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent.transform, false);
        RectTransform rect = btnGO.AddComponent<RectTransform>();
        Image img = btnGO.AddComponent<Image>();
        img.color = bgColor;
        Button btn = btnGO.AddComponent<Button>();
        btnGO.AddComponent<ButtonPressScale>();
        btnGO.AddComponent<ButtonClickSound>();

        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(0.9f, 0.9f, 0.85f);
        cb.pressedColor = new Color(0.75f, 0.75f, 0.65f);
        cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        btn.colors = cb;

        CreateTMPText(btnGO, "Text", text, 32, chalkWhite);
        StretchFull(btnGO.transform.GetChild(0).GetComponent<RectTransform>());

        return btnGO;
    }

    static GameObject CreatePanel(GameObject parent, string name, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        Image img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
