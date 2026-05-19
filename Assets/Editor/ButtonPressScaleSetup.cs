using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class ButtonPressScaleSetup
{
    [MenuItem("Toplama Oyunu/Tum Butonlara Basma Efekti Ekle")]
    public static void AddToAllButtonsInScene()
    {
        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int added = 0;

        foreach (Button btn in buttons)
        {
            if (btn.GetComponent<ButtonPressScale>() != null) continue;
            btn.gameObject.AddComponent<ButtonPressScale>();
            added++;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("ButtonPressScale eklendi: " + added + " buton.");
    }
}
