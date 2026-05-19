using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GameSoundSetup
{
    const string WrongPath = "Assets/648462__andreas__wrong-answer.mp3";
    const string CorrectPath = "Assets/752565__arawn1991__feel-good.mp3";
    const string LevelCompletePath = "Assets/752565__arawn1991__feel-good.mp3";

    [MenuItem("Toplama Oyunu/Oyun Seslerini Bagla")]
    public static void AssignGameSounds()
    {
        GameManager gm = Object.FindAnyObjectByType<GameManager>();
        if (gm == null)
        {
            Debug.LogError("Sahnede GameManager yok.");
            return;
        }

        gm.correctSound = AssetDatabase.LoadAssetAtPath<AudioClip>(CorrectPath);
        gm.wrongSound = AssetDatabase.LoadAssetAtPath<AudioClip>(WrongPath);
        gm.levelCompleteSound = AssetDatabase.LoadAssetAtPath<AudioClip>(LevelCompletePath);

        EditorUtility.SetDirty(gm);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("Sesler baglandi. Correct: " + (gm.correctSound != null) +
                  ", Wrong: " + (gm.wrongSound != null) +
                  ", LevelComplete: " + (gm.levelCompleteSound != null));
    }
}
