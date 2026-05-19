using UnityEditor;

using UnityEditor.SceneManagement;

using UnityEngine;

using UnityEngine.UI;

using TMPro;



public static class ChalkboardWipeSetup

{

    static readonly Color eraserColor = new Color(0.15f, 0.35f, 0.15f, 1f);



    [MenuItem("Toplama Oyunu/Tahta Silme Efektini Kur")]

    public static void Setup()

    {

        UIManager ui = Object.FindAnyObjectByType<UIManager>();

        if (ui == null || ui.chalkboardImage == null)

        {

            Debug.LogError("UIManager veya ChalkboardImage bulunamadi.");

            return;

        }



        RectTransform canvasRect = ui.GetComponent<RectTransform>();

        Transform board = ui.chalkboardImage.transform;



        ChalkboardWipeAnimator wipe = board.GetComponent<ChalkboardWipeAnimator>();

        if (wipe == null)

            wipe = board.gameObject.AddComponent<ChalkboardWipeAnimator>();



        RectTransform boardBounds = board.GetComponent<RectTransform>();

        RectTransform fullBounds = EnsureBoardWipeBounds(canvasRect, boardBounds, ui.optionsPanel);

        Transform oldBarOnBoard = board.Find("WipeBar");
        if (oldBarOnBoard != null && oldBarOnBoard.parent != fullBounds)
            Object.DestroyImmediate(oldBarOnBoard.gameObject);

        Transform existingBar = fullBounds.Find("WipeBar");

        GameObject wipeGO;

        if (existingBar != null)

        {

            wipeGO = existingBar.gameObject;

        }

        else

        {

            wipeGO = new GameObject("WipeBar");

            wipeGO.transform.SetParent(fullBounds, false);

            Image img = wipeGO.AddComponent<Image>();

            img.color = eraserColor;

            img.raycastTarget = false;

        }



        RectTransform wipeRect = wipeGO.GetComponent<RectTransform>();
        wipeRect.anchorMin = new Vector2(0.5f, 0.5f);
        wipeRect.anchorMax = new Vector2(0.5f, 0.5f);
        wipeRect.pivot = new Vector2(0.5f, 0.5f);
        wipeRect.sizeDelta = new Vector2(200f, 200f);
        wipeRect.anchoredPosition = Vector2.zero;



        wipe.wipeBar = wipeRect;

        wipe.wipeBounds = boardBounds;

        wipe.boardWipeBounds = fullBounds;

        wipe.questionText = ui.questionText;

        wipe.levelInfoText = ui.levelInfoText;

        wipe.optionTexts = ui.optionTexts;

        ui.chalkboardWipe = wipe;



        wipeGO.transform.SetAsLastSibling();



        EditorUtility.SetDirty(ui);

        EditorUtility.SetDirty(wipe);

        EditorUtility.SetDirty(fullBounds.gameObject);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("Tahta silme efekti kuruldu (soru + level info + siklar). WipeBar rengini veya sprite'ini istedigin gibi degistir.");

    }



    static RectTransform EnsureBoardWipeBounds(RectTransform canvas, RectTransform chalkboard, GameObject optionsPanel)

    {

        Transform existing = canvas.Find("BoardWipeBounds");

        RectTransform boundsRect;

        if (existing != null)

        {

            boundsRect = existing.GetComponent<RectTransform>();

        }

        else

        {

            GameObject boundsGO = new GameObject("BoardWipeBounds");

            boundsGO.transform.SetParent(canvas, false);

            boundsRect = boundsGO.AddComponent<RectTransform>();

        }



        Vector2 minAnchor = chalkboard.anchorMin;

        Vector2 maxAnchor = chalkboard.anchorMax;

        Vector2 minPos = chalkboard.anchoredPosition;

        Vector2 maxPos = chalkboard.anchoredPosition;



        if (optionsPanel != null)

        {

            RectTransform options = optionsPanel.GetComponent<RectTransform>();

            minAnchor = Vector2.Min(minAnchor, options.anchorMin);

            maxAnchor = Vector2.Max(maxAnchor, options.anchorMax);

            minPos = Vector2.Min(minPos, options.anchoredPosition);

            maxPos = Vector2.Max(maxPos, options.anchoredPosition);

        }



        boundsRect.anchorMin = new Vector2(0.1f, 0.05f);

        boundsRect.anchorMax = new Vector2(0.9f, 0.95f);

        boundsRect.pivot = new Vector2(0.5f, 0.5f);

        boundsRect.anchoredPosition = Vector2.zero;

        boundsRect.sizeDelta = Vector2.zero;



        boundsRect.SetAsLastSibling();

        return boundsRect;

    }

}

