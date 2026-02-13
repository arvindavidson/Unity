using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Text healthText;
    public Text objectiveText;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    private bool isGameOver;
    private bool hasTreasure;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
        UpdateObjectiveText();
    }

    public void UpdateHealth(float health)
    {
        healthText.text = "Health: " + health;
    }

    public void CollectTreasure()
    {
        hasTreasure = true;
        UpdateObjectiveText();
        WinGame();
    }

    void UpdateObjectiveText()
    {
        if (hasTreasure)
            objectiveText.text = "Objective: Complete!";
        else
            objectiveText.text = "Objective: Find Treasure";
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void WinGame()
    {
        if (isGameOver) return;
        isGameOver = true;
        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
