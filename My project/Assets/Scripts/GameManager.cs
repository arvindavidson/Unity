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
    private int treasuresCollected;
    private int totalTreasures;

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
        
        // Find all treasures to set the goal
        var treasures = GameObject.FindGameObjectsWithTag("Treasure");
        totalTreasures = treasures.Length;
        // If tag isn't used, look for objects with "Treasure" in name as fallback
        if (totalTreasures == 0)
        {
             var allObjs = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
             foreach (var t in allObjs)
             {
                 if (t.name.Contains("Treasure")) totalTreasures++;
             }
        }
        
        UpdateObjectiveText();
    }

    public void UpdateHealth(float health)
    {
        healthText.text = "Health: " + health;
    }

    public void CollectTreasure()
    {
        treasuresCollected++;
        UpdateObjectiveText();
        
        if (treasuresCollected >= totalTreasures && totalTreasures > 0)
        {
            WinGame();
        }
    }

    void UpdateObjectiveText()
    {
        if (totalTreasures > 0)
        {
            if (treasuresCollected >= totalTreasures)
                 objectiveText.text = $"Objective: Complete! ({treasuresCollected}/{totalTreasures})";
            else
                 objectiveText.text = $"Objective: Find Treasures ({treasuresCollected}/{totalTreasures})";
        }
        else
        {
            objectiveText.text = "Objective: Find Treasure";
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
        
        // Unlock cursor for UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void WinGame()
    {
        if (isGameOver) return;
        isGameOver = true;
        winPanel.SetActive(true);
        Time.timeScale = 0f;
        
        // Unlock cursor for UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
