using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public GameObject settingsPanel;
    public Slider mouseSensSlider;
    public Slider controllerSensSlider;
    public Text mouseSensAmount;
    public Text controllerSensAmount;

    // References
    private PlayerController player;
    private bool isSettingsOpen = false;

    void Start()
    {
        // Find player
        player = FindAnyObjectByType<PlayerController>();

        // Setup sliders
        if (player != null)
        {
            mouseSensSlider.value = player.mouseSensitivity;
            controllerSensSlider.value = player.controllerSensitivity;
            UpdateText();
        }

        // Listeners
        mouseSensSlider.onValueChanged.AddListener(OnMouseSensChanged);
        controllerSensSlider.onValueChanged.AddListener(OnControllerSensChanged);

        // Hide panel initially
        settingsPanel.SetActive(false);
    }

    void Update()
    {
        // Toggle with Escape
        if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame || 
            UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.InputSystem.Gamepad.current.startButton.wasPressedThisFrame)
        {
            ToggleSettings();
        }
    }

    public void ToggleSettings()
    {
        isSettingsOpen = !isSettingsOpen;
        settingsPanel.SetActive(isSettingsOpen);
        
        if (isSettingsOpen)
        {
            Time.timeScale = 0f;
            // Unlock and show cursor for UI
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            // Lock handled by PlayerController, but let's be explicit
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void OnMouseSensChanged(float value)
    {
        if (player != null)
        {
            player.mouseSensitivity = value;
            UpdateText();
        }
    }

    void OnControllerSensChanged(float value)
    {
        if (player != null)
        {
            player.controllerSensitivity = value;
            UpdateText();
        }
    }

    void UpdateText()
    {
        if (mouseSensAmount != null) mouseSensAmount.text = mouseSensSlider.value.ToString("F1");
        if (controllerSensAmount != null) controllerSensAmount.text = controllerSensSlider.value.ToString("F1");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
