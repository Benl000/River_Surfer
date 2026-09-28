using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Game State")]
    public bool isGameOver = false;

    [Header("Score Progression")]
    public int score = 0;

    [Header("Speed Progression")]
    [Tooltip("תוספת מהירות פסיבית בכל שנייה שחולפת - מבטיח שהמשחק יקשה מעצמו")]
    public float passiveSpeedIncreasePerSecond = 2f;
    
    [Tooltip("תוספת מהירות בונוס על כל מטבע שנאסף (בנוסף להאצה הפסיבית)")]
    public float speedIncreasePerToken = 1;
    
    [Tooltip("מהירות מקסימלית שאליה הסירה יכולה להגיע (מומלץ 35-40 למשחק קשה)")]
    public float maxForwardSpeed = 200f;

    private PlayerController player;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        player = Object.FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        if (isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            return;
        }

        // האצה פסיבית רציפה לאורך זמן לפי DeltaTime
        if (player != null && player.forwardSpeed < maxForwardSpeed)
        {
            player.forwardSpeed = Mathf.Min(player.forwardSpeed + (passiveSpeedIncreasePerSecond * Time.deltaTime), maxForwardSpeed);
        }
    }

    public void AddToken()
    {
        if (isGameOver) return;

        score++;
        
        // האצת בונוס מיידית מהמטבע
        if (player != null && player.forwardSpeed < maxForwardSpeed)
        {
            player.forwardSpeed = Mathf.Min(player.forwardSpeed + speedIncreasePerToken, maxForwardSpeed);
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("GAME OVER! Final Score: " + score);
    }
}