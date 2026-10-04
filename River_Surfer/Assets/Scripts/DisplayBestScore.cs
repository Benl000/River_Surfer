using UnityEngine;
using TMPro;

public class DisplayBestScore : MonoBehaviour
{
    void Start()
    {
        int savedBest = PlayerPrefs.GetInt("HighScore", 0);
        GetComponent<TMP_Text>().text = savedBest.ToString();
    }
}