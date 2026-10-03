using UnityEngine;
using TMPro;

public class DisplayBestScore : MonoBehaviour
{
    void Start()
    {
        // מושך את ה-High Score שנשמר בזיכרון של המשחק
        int savedBest = PlayerPrefs.GetInt("HighScore", 0);
        
        // מעדכן את הטקסט על השלט ברגע שהמשחק עולה
        GetComponent<TMP_Text>().text = savedBest.ToString();
    }
}