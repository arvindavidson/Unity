using UnityEngine;
using UnityEngine.UI;

public class UISetup
{
    public static void CreateUI()
    {
        if (GameObject.Find("Canvas") != null) return;

        GameObject canvas = new GameObject("Canvas");
        Canvas c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();

        // Health Text
        GameObject healthTextObj = new GameObject("HealthText");
        healthTextObj.transform.SetParent(canvas.transform);
        Text healthText = healthTextObj.AddComponent<Text>();
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        healthText.fontSize = 24;
        healthText.color = Color.white;
        healthText.alignment = TextAnchor.UpperLeft;
        RectTransform healthRect = healthText.GetComponent<RectTransform>();
        healthRect.anchorMin = new Vector2(0, 1);
        healthRect.anchorMax = new Vector2(0, 1);
        healthRect.pivot = new Vector2(0, 1);
        healthRect.anchoredPosition = new Vector2(10, -10);
        healthRect.sizeDelta = new Vector2(200, 30);

        // Objective Text
        GameObject objectiveTextObj = new GameObject("ObjectiveText");
        objectiveTextObj.transform.SetParent(canvas.transform);
        Text objectiveText = objectiveTextObj.AddComponent<Text>();
        objectiveText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        objectiveText.fontSize = 24;
        objectiveText.color = Color.white;
        objectiveText.alignment = TextAnchor.UpperRight;
        RectTransform objectiveRect = objectiveText.GetComponent<RectTransform>();
        objectiveRect.anchorMin = new Vector2(1, 1);
        objectiveRect.anchorMax = new Vector2(1, 1);
        objectiveRect.pivot = new Vector2(1, 1);
        objectiveRect.anchoredPosition = new Vector2(-10, -10);
        objectiveRect.sizeDelta = new Vector2(300, 30);

        // Game Over Panel
        GameObject gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvas.transform);
        Image gameOverImage = gameOverPanel.AddComponent<Image>();
        gameOverImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform gameOverRect = gameOverPanel.GetComponent<RectTransform>();
        gameOverRect.anchorMin = Vector2.zero;
        gameOverRect.anchorMax = Vector2.one;
        gameOverRect.offsetMin = Vector2.zero;
        gameOverRect.offsetMax = Vector2.zero;
        
        GameObject gameOverTextObj = new GameObject("GameOverText");
        gameOverTextObj.transform.SetParent(gameOverPanel.transform);
        Text gameOverText = gameOverTextObj.AddComponent<Text>();
        gameOverText.text = "GAME OVER";
        gameOverText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        gameOverText.fontSize = 48;
        gameOverText.color = Color.red;
        gameOverText.alignment = TextAnchor.MiddleCenter;
        RectTransform gameOverTextRect = gameOverText.GetComponent<RectTransform>();
        gameOverTextRect.anchorMin = Vector2.zero;
        gameOverTextRect.anchorMax = Vector2.one;
        gameOverTextRect.offsetMin = Vector2.zero;
        gameOverTextRect.offsetMax = Vector2.zero;

        // Win Panel
        GameObject winPanel = new GameObject("WinPanel");
        winPanel.transform.SetParent(canvas.transform);
        Image winImage = winPanel.AddComponent<Image>();
        winImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform winRect = winPanel.GetComponent<RectTransform>();
        winRect.anchorMin = Vector2.zero;
        winRect.anchorMax = Vector2.one;
        winRect.offsetMin = Vector2.zero;
        winRect.offsetMax = Vector2.zero;

        GameObject winTextObj = new GameObject("WinText");
        winTextObj.transform.SetParent(winPanel.transform);
        Text winText = winTextObj.AddComponent<Text>();
        winText.text = "YOU WIN!";
        winText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        winText.fontSize = 48;
        winText.color = Color.green;
        winText.alignment = TextAnchor.MiddleCenter;
        RectTransform winTextRect = winText.GetComponent<RectTransform>();
        winTextRect.anchorMin = Vector2.zero;
        winTextRect.anchorMax = Vector2.one;
        winTextRect.offsetMin = Vector2.zero;
        winTextRect.offsetMax = Vector2.zero;

        // Assign GameManager
        GameObject gameManagerObj = new GameObject("GameManager");
        GameManager gameManager = gameManagerObj.AddComponent<GameManager>();
        gameManager.healthText = healthText;
        gameManager.objectiveText = objectiveText;
        gameManager.gameOverPanel = gameOverPanel;
        gameManager.winPanel = winPanel;

        // Add EventSystem
        if (GameObject.Find("EventSystem") == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }
}
