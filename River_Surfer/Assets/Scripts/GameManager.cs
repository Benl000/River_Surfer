using UnityEngine;

public class GameManager : MonoBehaviour
{
    // הגדרת משתנה סטטי מאפשרת לכל סקריפט אחר במשחק לגשת ל-GameManager בקלות
    public static GameManager instance;

    public int score = 0;
    public bool isGameOver = false;

    void Awake()
    {
        // מוודא שיש רק GameManager אחד בסצנה
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int amount)
    {
        if (isGameOver) return;
        
        score += amount;
        Debug.Log("Score: " + score);
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("GAME OVER! You hit an obstacle.");
        
        // כאן בהמשך נוסיף קוד שעוצר את תנועת המסלול/הסירה ומקפיץ מסך הפסד
    }
}