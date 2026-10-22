using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip musicClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource is missing!");
            return;
        }

        if (musicClip != null)
        {
            audioSource.clip = musicClip;
        }

        audioSource.loop = true;

        // Apply saved settings volume
        audioSource.volume =
            PlayerPrefs.GetFloat("MusicVolume", 1f);

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public float GetVolume()
    {
        return audioSource.volume;
    }
}