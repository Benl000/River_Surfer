using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameState { StartMenu, Playing, Paused, GameOver }
    
    [Header("Current State")]
    public GameState currentState = GameState.StartMenu;
    private GameState previousState;

    [Header("UI Panels & Elements")]
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;
    public GameObject hudPanel; 

    [Header("Score UI")]
    public TextMeshProUGUI[] liveScoreTexts; 
    public TextMeshProUGUI[] highScoreTexts; 
    public TextMeshProUGUI finalScoreText; 

    [Header("Game State")]
    public bool isGameOver = false;

    [Header("Score Progression")]
    public int score = 0;
    private int highScore = 0; 

    [Header("Speed Progression")]
    public float passiveSpeedIncreasePerSecond = 2f;
    public float speedIncreasePerToken = 1;
    public float maxForwardSpeed = 200f;

    [Header("Start Environment")]
    public GameObject startingEnvironment;
    private PlayerController player;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        player = Object.FindFirstObjectByType<PlayerController>();
        
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        
        UpdateScoreUI(); 
        UpdateHighScoreUI();

        ChangeState(GameState.StartMenu);
    }
    
    public void StartGame()
    {
        ChangeState(GameState.Playing);
        if (startingEnvironment != null) Destroy(startingEnvironment, 4f);
    }

    void Update()
    {
        if (currentState == GameState.StartMenu)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || 
                Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                StartGame();
            }
        }
        else if (currentState == GameState.Playing)
        {
            if (player != null && player.forwardSpeed < maxForwardSpeed)
            {
                player.forwardSpeed = Mathf.Min(player.forwardSpeed + (passiveSpeedIncreasePerSecond * Time.deltaTime), maxForwardSpeed);
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
        else if (currentState == GameState.Paused)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
        else if (currentState == GameState.GameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RestartGame();
            }
        }
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case GameState.StartMenu:
                Time.timeScale = 0f; 
                isGameOver = false;
                if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
                if (gameOverPanel) gameOverPanel.SetActive(false);
                if (hudPanel) hudPanel.SetActive(true); 
                break;

            case GameState.Playing:
                Time.timeScale = 1f; 
                isGameOver = false;
                if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
                if (hudPanel) hudPanel.SetActive(true); 
                break;

            case GameState.Paused:
                Time.timeScale = 0f; 
                if (pauseMenuPanel) pauseMenuPanel.SetActive(true);
                if (hudPanel) hudPanel.SetActive(false); 
                break;

            case GameState.GameOver:
                Time.timeScale = 1f; 
                isGameOver = true;
                if (gameOverPanel) gameOverPanel.SetActive(true);
                if (hudPanel) hudPanel.SetActive(true); 
                
                if (finalScoreText != null) finalScoreText.text = score.ToString("D4");
                break;
        }
    }

    public void TogglePause()
    {
        if (currentState == GameState.Playing || currentState == GameState.StartMenu)
        {
            previousState = currentState; 
            ChangeState(GameState.Paused);
        }
        else if (currentState == GameState.Paused)
        {
            ChangeState(previousState);
        }
    }

    public void AddToken()
    {
        if (isGameOver || currentState != GameState.Playing) return;

        score++;
        
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore); 
            PlayerPrefs.Save();
            UpdateHighScoreUI(); 
        }

        UpdateScoreUI(); 
        
        if (player != null && player.forwardSpeed < maxForwardSpeed)
        {
            player.forwardSpeed = Mathf.Min(player.forwardSpeed + speedIncreasePerToken, maxForwardSpeed);
        }
    }

    private void UpdateScoreUI()
    {
        foreach (TextMeshProUGUI txt in liveScoreTexts)
        {
            if (txt != null) txt.text = score.ToString("D4");
        }
    }

    private void UpdateHighScoreUI()
    {
        foreach (TextMeshProUGUI txt in highScoreTexts)
        {
            if (txt != null) txt.text = "Best: " + highScore.ToString("D4");
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        ChangeState(GameState.GameOver);
    }

    public void RestartGame()
    {
        DOTween.KillAll(); 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}