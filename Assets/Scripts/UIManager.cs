using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Soru Alani")]
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI levelInfoText;
    public ChalkboardWipeAnimator chalkboardWipe;

    [Header("Oyun Panelleri")]
    public GameObject chalkboardImage;
    public GameObject optionsPanel;

    [Header("Sik Butonlari (4 adet)")]
    public Button[] optionButtons;
    public TextMeshProUGUI[] optionTexts;

    [Header("Geri Bildirim Paneli")]
    public GameObject feedbackPanel;
    public TextMeshProUGUI feedbackText;
    public FeedbackImageAnimator feedbackImageAnimator;

    [Header("Bolum Sonu Paneli")]
    public GameObject levelCompletePanel;
    public TextMeshProUGUI levelCompleteTitle;
    public TextMeshProUGUI levelResultText;
    public GameObject[] starImages;
    public Button nextLevelButton;
    public Button retryButton;
    public Button menuButton;

    [Header("Renkler")]
    public Color correctColor = new Color(0.3f, 0.9f, 0.3f);
    public Color wrongColor = new Color(0.9f, 0.3f, 0.3f);
    public Color normalColor = Color.white;
    public Color timerWarningColor = new Color(1f, 0.4f, 0.4f);
    public Color chalkColor = new Color(0.95f, 0.95f, 0.9f);

    Color[] originalButtonColors;

    void Start()
    {
        originalButtonColors = new Color[optionButtons.Length];
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].onClick.AddListener(() => GameManager.Instance.OnAnswerSelected(index));
            originalButtonColors[i] = optionButtons[i].image.color;
        }

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(() => GameManager.Instance.OnNextLevel());
        if (retryButton != null)
            retryButton.onClick.AddListener(() => GameManager.Instance.OnRetryLevel());
        if (menuButton != null)
            menuButton.onClick.AddListener(() => GameManager.Instance.OnGoToMenu());

        if (chalkboardWipe == null && chalkboardImage != null)
            chalkboardWipe = chalkboardImage.GetComponent<ChalkboardWipeAnimator>();

        if (chalkboardWipe != null)
        {
            if (chalkboardWipe.levelInfoText == null)
                chalkboardWipe.levelInfoText = levelInfoText;
            if (chalkboardWipe.optionTexts == null || chalkboardWipe.optionTexts.Length == 0)
                chalkboardWipe.optionTexts = optionTexts;
        }

        HideAllPanels();
    }

    public string FormatQuestion(Question question)
    {
        return question.number1 + " " + question.GetOperationSymbol() + " " + question.number2 + " = ?";
    }

    public string FormatLevelInfo(int level, int questionNum, int totalQuestions)
    {
        return "Bolum " + level + "  -  Soru " + questionNum + "/" + totalQuestions;
    }

    BoardWipeContent BuildBoardContent(Question question, int level, int questionNum, int totalQuestions)
    {
        string[] options = new string[optionTexts.Length];
        for (int i = 0; i < options.Length; i++)
            options[i] = question.options[i].ToString();

        return new BoardWipeContent
        {
            question = FormatQuestion(question),
            levelInfo = FormatLevelInfo(level, questionNum, totalQuestions),
            options = options
        };
    }

    public void ShowQuestion(Question question, int level, int questionNum, int totalQuestions)
    {
        levelInfoText.text = FormatLevelInfo(level, questionNum, totalQuestions);
        questionText.text = FormatQuestion(question);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionTexts[i].text = question.options[i].ToString();
            optionButtons[i].image.color = originalButtonColors[i];
        }
    }

    public void ShowQuestionWithWipe(Question question, int level, int questionNum, int totalQuestions, Action onComplete)
    {
        if (chalkboardWipe == null)
        {
            ShowQuestion(question, level, questionNum, totalQuestions);
            onComplete?.Invoke();
            return;
        }

        CancelInvoke(nameof(HideFeedbackPanel));
        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);
        SetGamePanelsVisible(true);

        SetOptionsInteractable(false);

        BoardWipeContent content = BuildBoardContent(question, level, questionNum, totalQuestions);
        chalkboardWipe.PlayWipe(content, () =>
        {
            for (int i = 0; i < optionButtons.Length; i++)
                optionButtons[i].image.color = originalButtonColors[i];

            onComplete?.Invoke();
        });
    }

    public void UpdateTimer(float time)
    {
        int seconds = Mathf.CeilToInt(Mathf.Max(0, time));
        timerText.text = seconds.ToString();
        timerText.color = (time <= 10f) ? timerWarningColor : chalkColor;

        float scale = (time <= 5f) ? 1f + Mathf.PingPong(Time.time * 2f, 0.15f) : 1f;
        timerText.transform.localScale = Vector3.one * scale;
    }

    public void UpdateLevelInfo(int level, int questionNum, int totalQuestions)
    {
        levelInfoText.text = FormatLevelInfo(level, questionNum, totalQuestions);
    }

    bool lastFeedbackCorrect;

    public void ShowFeedback(bool isCorrect, int correctAnswer)
    {
        lastFeedbackCorrect = isCorrect;
        SetGamePanelsVisible(false);
        feedbackPanel.SetActive(true);

        if (isCorrect)
        {
            feedbackText.text = "Dogru!";
            feedbackText.color = correctColor;
        }
        else
        {
            feedbackText.text = "Yanlis! Cevap: " + correctAnswer;
            feedbackText.color = wrongColor;
        }

        float feedbackDuration = 1.5f;
        if (feedbackImageAnimator != null)
        {
            feedbackImageAnimator.Play(isCorrect);
            feedbackDuration = feedbackImageAnimator.TotalDuration + 0.1f;
        }

        CancelInvoke(nameof(HideFeedbackPanel));
        Invoke(nameof(HideFeedbackPanel), feedbackDuration);
    }

    public void HighlightOption(int index, bool isCorrect)
    {
        if (index < 0 || index >= optionButtons.Length) return;
        optionButtons[index].image.color = isCorrect ? correctColor : wrongColor;
    }

    void HideFeedbackPanel()
    {
        feedbackPanel.SetActive(false);
        if (lastFeedbackCorrect)
            SetGamePanelsVisible(true);
    }

    public void ShowFailed(int correct, int answeredCount, int total)
    {
        SetGamePanelsVisible(false);
        levelCompletePanel.SetActive(true);

        levelCompleteTitle.text = "Yanlis Cevap!";
        levelResultText.text = correct + " / " + answeredCount + " dogru.\nBolumu tekrar deneyin!";

        for (int i = 0; i < starImages.Length; i++)
            starImages[i].SetActive(false);

        nextLevelButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(true);
        menuButton.gameObject.SetActive(true);
    }

    public void ShowTimeUp(int answered, int total)
    {
        SetGamePanelsVisible(false);
        levelCompletePanel.SetActive(true);

        levelCompleteTitle.text = "Sure Doldu!";
        levelResultText.text = answered + " / " + total + " soru cevaplanabildi.\nTekrar deneyin!";

        for (int i = 0; i < starImages.Length; i++)
            starImages[i].SetActive(false);

        nextLevelButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(true);
        menuButton.gameObject.SetActive(true);
    }

    public void ShowLevelComplete(int correct, int total, int stars, bool isLastLevel)
    {
        SetGamePanelsVisible(false);
        levelCompletePanel.SetActive(true);

        if (isLastLevel && stars > 0)
            levelCompleteTitle.text = "Tebrikler! Oyunu Bitirdin!";
        else
            levelCompleteTitle.text = "Bolum Tamamlandi!";

        levelResultText.text = correct + " / " + total + " Dogru";

        for (int i = 0; i < starImages.Length; i++)
        {
            starImages[i].SetActive(i < stars);
        }

        nextLevelButton.gameObject.SetActive(!isLastLevel && stars > 0);
        retryButton.gameObject.SetActive(true);
        menuButton.gameObject.SetActive(true);
    }

    public void SetOptionsInteractable(bool interactable)
    {
        foreach (var btn in optionButtons)
            btn.interactable = interactable;
    }

    public void HideAllPanels()
    {
        if (feedbackImageAnimator != null)
            feedbackImageAnimator.Stop();

        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        SetGamePanelsVisible(true);
    }

    void SetGamePanelsVisible(bool visible)
    {
        if (chalkboardImage != null) chalkboardImage.SetActive(visible);
        if (optionsPanel != null) optionsPanel.SetActive(visible);
    }
}
