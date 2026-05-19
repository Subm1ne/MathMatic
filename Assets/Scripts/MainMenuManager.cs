using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Ana Menu")]
    public GameObject mainPanel;
    public Button playButton;

    [Header("Bolum Secimi")]
    public GameObject levelSelectPanel;
    public Button[] levelButtons;
    public TextMeshProUGUI[] levelTexts;
    public Button backButton;

    void Start()
    {
        levelSelectPanel.SetActive(false);
        mainPanel.SetActive(true);

        playButton.onClick.AddListener(ShowLevelSelect);

        if (backButton != null)
            backButton.onClick.AddListener(ShowMainPanel);

        SetupLevelButtons();
    }

    void SetupLevelButtons()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int level = i + 1;
            bool isUnlocked = level <= unlockedLevel;

            levelButtons[i].interactable = isUnlocked;
            levelTexts[i].text = "Bolum " + level;

            if (isUnlocked)
            {
                int lvl = level;
                levelButtons[i].onClick.AddListener(() => LoadLevel(lvl));
            }
        }
    }

    void ShowLevelSelect()
    {
        mainPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    void ShowMainPanel()
    {
        levelSelectPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    void LoadLevel(int level)
    {
        PlayerPrefs.SetInt("SelectedLevel", level);
        SceneManager.LoadScene("GameScene");
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainMenu");
    }
}
