using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip coinSound;
    public AudioClip jumpSound;
    public AudioClip diveSound;
    public AudioClip laneSwitchSound;
    public AudioClip crashSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    private void Start()
    {
        if (backgroundMusic != null)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void ToggleBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.mute = !bgmSource.mute;
        }
    }

    public void ToggleSFX()
    {
        if (sfxSource != null)
        {
            sfxSource.mute = !sfxSource.mute;
        }
    }
}