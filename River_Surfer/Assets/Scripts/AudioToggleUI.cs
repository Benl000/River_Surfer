using UnityEngine;
using UnityEngine.UI;

public class AudioToggleUI : MonoBehaviour
{
    [Header("Settings")]
    public bool isBGM = true;

    [Header("UI Elements")]
    public Image iconImage;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            if (isBGM) UpdateIcon(AudioManager.Instance.bgmSource.mute);
            else UpdateIcon(AudioManager.Instance.sfxSource.mute);
        }
    }

    public void OnToggleClicked()
    {
        if (AudioManager.Instance == null) return;

        if (isBGM)
        {
            AudioManager.Instance.ToggleBGM();
            UpdateIcon(AudioManager.Instance.bgmSource.mute);
        }
        else
        {
            AudioManager.Instance.ToggleSFX();
            UpdateIcon(AudioManager.Instance.sfxSource.mute);
        }
    }

    private void UpdateIcon(bool isMuted)
    {
        if (iconImage != null)
        {
            iconImage.sprite = isMuted ? soundOffSprite : soundOnSprite;
        }
    }
}