using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Oyun Ayarlari")]
    public int questionsPerLevel = 5;
    public int totalLevels = 10;
    public float timePerLevel = 30f;

    [Header("Referanslar")]
    public UIManager uiManager;

    [Header("Ses Efektleri")]
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip levelCompleteSound;
    public AudioClip tickSound;

    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    int currentLevel = 1;
    int currentQuestionIndex = 0;
    int correctAnswers = 0;
    float remainingTime;
    bool isPlaying = false;
    bool tickPlayed = false;

    Question currentQuestion;
    AudioSource sfxSource;
    AudioSource tickSource;
    bool isFirstQuestionInLevel = true;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;

        tickSource = gameObject.AddComponent<AudioSource>();
        tickSource.playOnAwake = false;
        tickSource.loop = true;
        tickSource.spatialBlend = 0f;
    }

    void Start()
    {
        int selectedLevel = PlayerPrefs.GetInt("SelectedLevel", 1);
        currentLevel = selectedLevel;
        StartLevel(currentLevel);
    }

    void Update()
    {
        if (!isPlaying) return;

        remainingTime -= Time.deltaTime;
        uiManager.UpdateTimer(remainingTime);

        if (remainingTime <= 5f && !tickPlayed && tickSound != null)
        {
            tickPlayed = true;
            tickSource.clip = tickSound;
            tickSource.Play();
        }

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            TimeUp();
        }
    }

    public void StartLevel(int level)
    {
        currentLevel = level;
        currentQuestionIndex = 0;
        correctAnswers = 0;
        remainingTime = timePerLevel;
        isPlaying = true;
        tickPlayed = false;

        StopTick();

        isFirstQuestionInLevel = true;
        uiManager.HideAllPanels();
        uiManager.UpdateLevelInfo(currentLevel, currentQuestionIndex + 1, questionsPerLevel);

        ShowNextQuestion();
    }

    void ShowNextQuestion()
    {
        if (currentQuestionIndex >= questionsPerLevel)
        {
            LevelComplete();
            return;
        }

        currentQuestion = QuestionGenerator.Generate(currentLevel);
        int questionNum = currentQuestionIndex + 1;

        if (isFirstQuestionInLevel)
        {
            isFirstQuestionInLevel = false;
            uiManager.ShowQuestion(currentQuestion, currentLevel, questionNum, questionsPerLevel);
            uiManager.SetOptionsInteractable(true);
        }
        else
        {
            uiManager.ShowQuestionWithWipe(
                currentQuestion,
                currentLevel,
                questionNum,
                questionsPerLevel,
                () => uiManager.SetOptionsInteractable(true));
        }
    }

    public void OnAnswerSelected(int optionIndex)
    {
        if (!isPlaying) return;

        uiManager.SetOptionsInteractable(false);

        bool isCorrect = optionIndex == currentQuestion.correctOptionIndex;

        if (isCorrect)
        {
            correctAnswers++;
            PlaySound(correctSound);

            uiManager.ShowFeedback(true, currentQuestion.correctAnswer);
            uiManager.HighlightOption(currentQuestion.correctOptionIndex, true);

            currentQuestionIndex++;
            Invoke(nameof(ShowNextQuestion), GetFeedbackWaitTime());
        }
        else
        {
            PlaySound(wrongSound);
            isPlaying = false;
            CancelInvoke(nameof(ShowNextQuestion));
            StopTick();

            uiManager.ShowFeedback(false, currentQuestion.correctAnswer);
            uiManager.HighlightOption(currentQuestion.correctOptionIndex, true);
            uiManager.HighlightOption(optionIndex, false);

            Invoke(nameof(ShowFailScreen), GetFeedbackWaitTime());
        }
    }

    void ShowFailScreen()
    {
        uiManager.ShowFailed(correctAnswers, currentQuestionIndex, questionsPerLevel);
    }

    void TimeUp()
    {
        isPlaying = false;
        CancelInvoke(nameof(ShowNextQuestion));
        StopTick();

        if (currentQuestionIndex < questionsPerLevel)
        {
            PlaySound(wrongSound);
            uiManager.ShowTimeUp(currentQuestionIndex, questionsPerLevel);
        }
        else
        {
            LevelComplete();
        }
    }

    void LevelComplete()
    {
        isPlaying = false;
        StopTick();

        int stars = CalculateStars();
        SaveProgress(stars);
        PlaySound(levelCompleteSound);

        bool isLastLevel = currentLevel >= totalLevels;
        uiManager.ShowLevelComplete(correctAnswers, questionsPerLevel, stars, isLastLevel);
    }

    int CalculateStars()
    {
        if (correctAnswers >= 5) return 3;
        if (correctAnswers >= 3) return 2;
        if (correctAnswers >= 1) return 1;
        return 0;
    }

    void SaveProgress(int stars)
    {
        int savedStars = PlayerPrefs.GetInt("Level_" + currentLevel + "_Stars", 0);
        if (stars > savedStars)
            PlayerPrefs.SetInt("Level_" + currentLevel + "_Stars", stars);

        if (currentLevel < totalLevels)
        {
            int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
            if (currentLevel + 1 > unlockedLevel && stars > 0)
                PlayerPrefs.SetInt("UnlockedLevel", currentLevel + 1);
        }

        PlayerPrefs.Save();
    }

    float GetFeedbackWaitTime()
    {
        if (uiManager != null && uiManager.feedbackImageAnimator != null)
            return uiManager.feedbackImageAnimator.TotalDuration + 0.15f;
        return 1.2f;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    void StopTick()
    {
        if (tickSource != null && tickSource.isPlaying)
            tickSource.Stop();
    }

    public void OnNextLevel()
    {
        if (currentLevel < totalLevels)
            StartLevel(currentLevel + 1);
    }

    public void OnRetryLevel()
    {
        StartLevel(currentLevel);
    }

    public void OnGoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
