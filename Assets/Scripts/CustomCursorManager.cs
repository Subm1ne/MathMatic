using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomCursorManager : MonoBehaviour
{
    public static CustomCursorManager Instance { get; private set; }

    [Header("Varsayilan (tum oyun)")]
    public Texture2D cursorTexture;
    public Vector2 hotspot = Vector2.zero;

    [Header("Istege bagli: sahneye ozel")]
    public Texture2D menuCursor;
    public Vector2 menuHotspot = Vector2.zero;
    public Texture2D gameCursor;
    public Vector2 gameHotspot = Vector2.zero;

    public CursorMode cursorMode = CursorMode.Auto;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        ApplyForScene(SceneManager.GetActiveScene().name);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyForScene(scene.name);
    }

    void ApplyForScene(string sceneName)
    {
        Texture2D texture = GetTextureForScene(sceneName);
        Vector2 spot = GetHotspotForScene(sceneName);

        if (texture != null)
            Cursor.SetCursor(texture, spot, cursorMode);
        else
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }

    Texture2D GetTextureForScene(string sceneName)
    {
        if (sceneName == "MainMenu" && menuCursor != null)
            return menuCursor;

        if (sceneName == "GameScene" && gameCursor != null)
            return gameCursor;

        return cursorTexture;
    }

    Vector2 GetHotspotForScene(string sceneName)
    {
        if (sceneName == "MainMenu" && menuCursor != null)
            return menuHotspot;

        if (sceneName == "GameScene" && gameCursor != null)
            return gameHotspot;

        return hotspot;
    }

    public void SetCursor(Texture2D texture, Vector2 cursorHotspot)
    {
        if (texture == null)
        {
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
            return;
        }

        Cursor.SetCursor(texture, cursorHotspot, cursorMode);
    }

    public void ResetToSystemCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }
}
