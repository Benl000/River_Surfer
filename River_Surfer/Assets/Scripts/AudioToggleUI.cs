using UnityEngine;
using UnityEngine.UI; // חובה כדי לעבוד עם רכיבי UI כמו Image

public class AudioToggleUI : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("סמן ב-V אם זה כפתור המוזיקה, השאר ריק אם זה כפתור האפקטים")]
    public bool isBGM = true;

    [Header("UI Elements")]
    public Image iconImage; // הרכיב שמציג את התמונה
    public Sprite soundOnSprite;  // תמונת "סאונד פועל"
    public Sprite soundOffSprite; // תמונת "סאונד כבוי"

    private void Start()
    {
        // מתעדכן אוטומטית ברגע שהתפריט נפתח כדי להציג את המצב הנכון
        if (AudioManager.Instance != null)
        {
            if (isBGM) UpdateIcon(AudioManager.Instance.bgmSource.mute);
            else UpdateIcon(AudioManager.Instance.sfxSource.mute);
        }
    }

    // הפונקציה הזו תחליף את הפעולה הישירה שהגדרנו קודם בכפתור
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
            // אם מושתק - שים תמונה כבויה, אחרת שים תמונה דולקת
            iconImage.sprite = isMuted ? soundOffSprite : soundOnSprite;
        }
    }
}