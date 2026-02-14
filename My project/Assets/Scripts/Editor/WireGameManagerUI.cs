using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WireGameManagerUI
{
    public static string Execute()
    {
        // 1. Consolidate Canvases
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        GameObject mainCanvasGO = null;

        // Prefer one named "UICanvas"
        foreach (var c in canvases)
        {
            if (c.name == "UICanvas")
            {
                mainCanvasGO = c.gameObject;
                break;
            }
        }

        // If not found, use the first one, or create new
        if (mainCanvasGO == null)
        {
            if (canvases.Length > 0)
            {
                mainCanvasGO = canvases[0].gameObject;
                mainCanvasGO.name = "UICanvas";
            }
            else
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Canvas.prefab");
                if (prefab)
                {
                    mainCanvasGO = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    mainCanvasGO.name = "UICanvas";
                }
                else
                {
                    mainCanvasGO = new GameObject("UICanvas");
                    mainCanvasGO.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                    mainCanvasGO.AddComponent<CanvasScaler>();
                    mainCanvasGO.AddComponent<GraphicRaycaster>();
                }
            }
        }

        // Destroy all other canvases
        foreach (var c in canvases)
        {
            if (c.gameObject != mainCanvasGO)
            {
                Object.DestroyImmediate(c.gameObject);
            }
        }

        var canvas = mainCanvasGO.GetComponent<Canvas>();
        var canvasRT = mainCanvasGO.GetComponent<RectTransform>();

        // Ensure CanvasScaler
        var scaler = mainCanvasGO.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = mainCanvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // GLOBAL CLEANUP: Find ALL objects with specific UI names and destroy them
        // This ensures no duplicates exist anywhere in the scene (active or inactive)
        string[] uiNames = new string[] { "HealthText", "ObjectiveText", "GameOverPanel", "WinPanel", "SettingsPanel" };
        foreach (string name in uiNames)
        {
            var objects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var obj in objects)
            {
                if (obj.name == name && obj.scene.isLoaded) // Ensure it's in the scene, not an asset
                {
                    Object.DestroyImmediate(obj);
                }
            }
        }

        // Create HealthText (top-left) with padding
        var healthGO = CreateText("HealthText", "Health: 100", canvasRT,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(80, -80), new Vector2(400, 60), TextAnchor.MiddleLeft, 40);

        // Count treasures to set initial text correctly
        int totalTreasures = GameObject.FindGameObjectsWithTag("Treasure").Length;
        if (totalTreasures == 0)
        {
             var allObjs = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
             foreach (var t in allObjs)
             {
                 if (t.name.Contains("Treasure")) totalTreasures++;
             }
        }
        string objText = totalTreasures > 0 ? $"Objective: Find Treasures (0/{totalTreasures})" : "Objective: Find Treasure";

        // Create ObjectiveText (top-center)
        var objectiveGO = CreateText("ObjectiveText", objText, canvasRT,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -80), new Vector2(800, 60), TextAnchor.MiddleCenter, 40);

        // Create GameOverPanel (centered, initially hidden)
        var gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasRT, false);
        var gopRT = gameOverPanel.AddComponent<RectTransform>();
        gopRT.anchorMin = new Vector2(0.5f, 0.5f);
        gopRT.anchorMax = new Vector2(0.5f, 0.5f);
        gopRT.pivot = new Vector2(0.5f, 0.5f);
        gopRT.sizeDelta = new Vector2(800, 400);
        gopRT.anchoredPosition = Vector2.zero;
        var gopImg = gameOverPanel.AddComponent<Image>();
        gopImg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Darker background
        gameOverPanel.SetActive(false);
        
        // GameOverText inside panel
        CreateText("GameOverText", "GAME OVER", gopRT,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 50), new Vector2(600, 120), TextAnchor.MiddleCenter, 72);

        // GameOver Restart Button
        CreateButton("RestartBtn", "Restart", gopRT, new Vector2(0, -50), new Vector2(200, 60), () => {
             var gmRef = Object.FindFirstObjectByType<GameManager>();
             if (gmRef) gmRef.RestartGame();
        });

        // GameOver Quit Button
        CreateButton("QuitBtn", "Quit", gopRT, new Vector2(0, -120), new Vector2(200, 60), () => {
             var gmRef = Object.FindFirstObjectByType<GameManager>();
             if (gmRef) gmRef.QuitGame();
        });

        // Create WinPanel (centered, initially hidden)
        var winPanel = new GameObject("WinPanel");
        winPanel.transform.SetParent(canvasRT, false);
        var wpRT = winPanel.AddComponent<RectTransform>();
        wpRT.anchorMin = new Vector2(0.5f, 0.5f);
        wpRT.anchorMax = new Vector2(0.5f, 0.5f);
        wpRT.pivot = new Vector2(0.5f, 0.5f);
        wpRT.sizeDelta = new Vector2(800, 400);
        wpRT.anchoredPosition = Vector2.zero;
        var wpImg = winPanel.AddComponent<Image>();
        wpImg.color = new Color(0.1f, 0.6f, 0.1f, 0.95f);
        winPanel.SetActive(false);

        // WinText inside panel
        CreateText("WinText", "YOU WIN!", wpRT,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 50), new Vector2(600, 120), TextAnchor.MiddleCenter, 72);

        // Win Restart Button
        CreateButton("RestartBtn", "Restart", wpRT, new Vector2(0, -50), new Vector2(200, 60), () => {
             var gmRef = Object.FindFirstObjectByType<GameManager>();
             if (gmRef) gmRef.RestartGame();
        });

        // Win Quit Button
        CreateButton("QuitBtn", "Quit", wpRT, new Vector2(0, -120), new Vector2(200, 60), () => {
             var gmRef = Object.FindFirstObjectByType<GameManager>();
             if (gmRef) gmRef.QuitGame();
        });

        // ---------------- SETTINGS PANEL ----------------
        var settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(canvasRT, false);
        var spRT = settingsPanel.AddComponent<RectTransform>();
        spRT.anchorMin = new Vector2(0.5f, 0.5f);
        spRT.anchorMax = new Vector2(0.5f, 0.5f);
        spRT.pivot = new Vector2(0.5f, 0.5f);
        spRT.sizeDelta = new Vector2(600, 500);
        spRT.anchoredPosition = Vector2.zero;
        var spImg = settingsPanel.AddComponent<Image>();
        spImg.color = new Color(0.2f, 0.2f, 0.2f, 0.98f);
        
        CreateText("SettingsTitle", "SETTINGS", spRT,
             new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
             new Vector2(0, -50), new Vector2(400, 60), TextAnchor.MiddleCenter, 48);
             
        // Mouse Sens
        CreateText("MouseSensLabel", "Mouse Sensitivity", spRT,
             new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
             new Vector2(-150, 50), new Vector2(200, 40), TextAnchor.MiddleLeft, 24);
        
        var mouseSlider = CreateSlider("MouseSensSlider", spRT, new Vector2(100, 50), new Vector2(200, 20));
        var mouseSensVal = CreateText("MouseSensVal", "1.0", spRT,
             new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
             new Vector2(250, 50), new Vector2(50, 40), TextAnchor.MiddleLeft, 24).GetComponent<Text>();

        // Controller Sens
        CreateText("ControllerSensLabel", "Controller Sensitivity", spRT,
             new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
             new Vector2(-150, -20), new Vector2(250, 40), TextAnchor.MiddleLeft, 24);
             
        var controllerSlider = CreateSlider("ControllerSensSlider", spRT, new Vector2(100, -20), new Vector2(200, 20));
        var controllerSensVal = CreateText("ControllerSensVal", "1.0", spRT,
             new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
             new Vector2(250, -20), new Vector2(50, 40), TextAnchor.MiddleLeft, 24).GetComponent<Text>();

        // Close Button
        CreateButton("CloseBtn", "Close", spRT, new Vector2(0, -150), new Vector2(200, 50), () => { });
        
        // Add SettingsUI component to CANVAS (must be active to receive input)
        // Check if exists first to avoid duplicates if re-running without full cleanup
        var settingsUI = mainCanvasGO.GetComponent<SettingsUI>();
        if (settingsUI == null) settingsUI = mainCanvasGO.AddComponent<SettingsUI>();
        
        settingsUI.settingsPanel = settingsPanel;
        settingsUI.mouseSensSlider = mouseSlider.GetComponent<Slider>();
        settingsUI.controllerSensSlider = controllerSlider.GetComponent<Slider>();
        settingsUI.mouseSensAmount = mouseSensVal;
        settingsUI.controllerSensAmount = controllerSensVal;
        
        // Hook up Close button to SettingsUI.ToggleSettings
        var closeBtn = spRT.Find("CloseBtn").GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(closeBtn.onClick, settingsUI.ToggleSettings);
        
        // Wire up Game Over / Win buttons using UnityEventTools
        // Note: The lambda above was a placeholder, we need persistent listeners for runtime.
        
        // Helper to wire Restart/Quit
        void WireGameManagerButtons(RectTransform panelRT)
        {
            var restartBtn = panelRT.Find("RestartBtn").GetComponent<Button>();
            var quitBtn = panelRT.Find("QuitBtn").GetComponent<Button>();
            
            // We need a reference to GameManager. 
            // In Editor, we can find the prefab or component in scene.
            var gmComp = Object.FindFirstObjectByType<GameManager>();
            if (gmComp)
            {
               UnityEditor.Events.UnityEventTools.AddPersistentListener(restartBtn.onClick, gmComp.RestartGame);
               UnityEditor.Events.UnityEventTools.AddPersistentListener(quitBtn.onClick, gmComp.QuitGame);
            }
        }
        
        WireGameManagerButtons(gopRT);
        WireGameManagerButtons(wpRT);

        settingsPanel.SetActive(false);

        // Wire GameManager
        var gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            var so = new SerializedObject(gm);
            so.FindProperty("healthText").objectReferenceValue = healthGO.GetComponent<Text>();
            so.FindProperty("objectiveText").objectReferenceValue = objectiveGO.GetComponent<Text>();
            so.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
            so.FindProperty("winPanel").objectReferenceValue = winPanel;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(gm);
        }

        // Also move Canvas under UI group if it exists
        var uiGroup = GameObject.Find("UI");
        if (uiGroup != null)
            mainCanvasGO.transform.parent = uiGroup.transform;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        return "UI recreated with CanvasScaler and wired to GameManager";
    }

    static GameObject CreateText(string name, string content, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 position, Vector2 size, TextAnchor alignment, int fontSize = 24)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        var text = go.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        // Add outline for visibility
        var outline = go.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        return go;
    }

    static GameObject CreateButton(string name, string textContent, RectTransform parent, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction onClickPlaceholder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        img.color = new Color(0.8f, 0.8f, 0.8f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.normalColor = new Color(0.8f, 0.8f, 0.8f);
        colors.highlightedColor = new Color(1f, 1f, 1f);
        colors.pressedColor = new Color(0.6f, 0.6f, 0.6f);
        btn.colors = colors;
        
        // Note: Runtime onClick listeners are lost when saving scene in Editor script unless using PersistentListeners.
        // We use the placeholder just to signify intent, but the calling code needs to use UnityEventTools for persistence.

        CreateText("Text", textContent, rt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, 24)
            .GetComponent<RectTransform>().offsetMin = Vector2.zero; // Stretch
            
        return go;
    }

    static GameObject CreateSlider(string name, RectTransform parent, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        var slider = go.AddComponent<Slider>();

        // Background
        var bg = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        var bgRT = bg.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero;
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.5f, 0.5f, 0.5f);
        slider.targetGraphic = bgImg;

        // Fill Area
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var faRT = fillArea.AddComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero;
        faRT.anchorMax = Vector2.one;
        faRT.sizeDelta = new Vector2(-20, 0); // Padding for handle

        // Fill
        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillRT = fill.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.sizeDelta = Vector2.zero;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.8f, 0.2f);
        
        slider.fillRect = fillRT;

        // Handle Area
        var handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(go.transform, false);
        var haRT = handleArea.AddComponent<RectTransform>();
        haRT.anchorMin = Vector2.zero;
        haRT.anchorMax = Vector2.one;
        haRT.sizeDelta = new Vector2(-20, 0);

        // Handle
        var handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        var hRT = handle.AddComponent<RectTransform>();
        hRT.anchorMin = Vector2.zero;
        hRT.anchorMax = Vector2.one;
        hRT.sizeDelta = new Vector2(20, 0);
        var hImg = handle.AddComponent<Image>();
        hImg.color = Color.white;
        
        slider.handleRect = hRT;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0.1f;
        slider.maxValue = 3.0f;
        slider.value = 1.0f;

        return go;
    }
}
