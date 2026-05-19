using UnityEngine;

public class UIButtonSoundManager : MonoBehaviour
{
    public static UIButtonSoundManager Instance { get; private set; }

    [Header("Tiklama Sesi")]
    public AudioClip clickSound;
    [Range(0f, 1f)]
    public float volume = 1f;

    AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    public void PlayClick()
    {
        if (clickSound == null || audioSource == null) return;
        audioSource.PlayOneShot(clickSound, volume);
    }
}
