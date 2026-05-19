using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BackgroundMusicSetup
{
    [MenuItem("Toplama Oyunu/Arka Plan Muzigini Kur")]
    public static void Setup()
    {
        BackgroundMusicManager existing = Object.FindAnyObjectByType<BackgroundMusicManager>();
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            Debug.Log("BackgroundMusic zaten var. Muzik dosyasini Inspector'dan bagla.");
            return;
        }

        GameObject go = new GameObject("BackgroundMusic");
        go.AddComponent<BackgroundMusicManager>();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = go;

        Debug.Log("BackgroundMusic olusturuldu. MainMenu sahnesinde kaydet ve Music Clip alanina muzigi surukle.");
    }
}
