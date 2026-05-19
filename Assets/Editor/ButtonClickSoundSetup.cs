using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class ButtonClickSoundSetup
{
    [MenuItem("Toplama Oyunu/Tum Butonlara Tiklama Sesi Ekle")]
    public static void AddToAllButtons()
    {
        EnsureSoundManager();

        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int added = 0;

        foreach (Button btn in buttons)
        {
            if (btn.GetComponent<ButtonClickSound>() != null) continue;
            btn.gameObject.AddComponent<ButtonClickSound>();
            added++;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("ButtonClickSound eklendi: " + added + " buton. UIButtonSoundManager'a ses dosyasini surukle.");
    }

    static void EnsureSoundManager()
    {
        if (Object.FindAnyObjectByType<UIButtonSoundManager>() != null) return;

        GameObject go = new GameObject("UIButtonSoundManager");
        go.AddComponent<UIButtonSoundManager>();
        Debug.Log("UIButtonSoundManager olusturuldu. Click Sound alanina AudioClip surukle.");
    }
}
