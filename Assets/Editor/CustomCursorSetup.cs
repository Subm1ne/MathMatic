using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CustomCursorSetup
{
    [MenuItem("Toplama Oyunu/Ozel Imleci Kur")]
    public static void Setup()
    {
        CustomCursorManager existing = Object.FindAnyObjectByType<CustomCursorManager>();
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            Debug.Log("CustomCursor zaten var. PNG dosyasini Inspector'dan bagla.");
            return;
        }

        GameObject go = new GameObject("CustomCursor");
        go.AddComponent<CustomCursorManager>();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = go;

        Debug.Log("CustomCursor olusturuldu. MainMenu sahnesinde kaydet.");
    }
}
