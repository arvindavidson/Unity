using UnityEngine;
using UnityEngine.UI;

public class EnemyStatusUI : MonoBehaviour
{
    [Header("UI References")]
    public Image meterImage;
    public Image iconImage;
    public Canvas canvas;

    [Header("Sprites")]
    public Sprite eyeIcon;
    public Sprite questionIcon;
    public Sprite exclamationIcon;

    [Header("Settings")]
    public Color normalColor = Color.white;
    public Color suspiciousColor = Color.yellow;
    public Color alertColor = Color.red;

    private Camera mainCamera;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;

        // Auto-find references if not assigned
        if (meterImage == null) meterImage = transform.Find("Meter")?.GetComponent<Image>();
        if (iconImage == null) iconImage = transform.Find("Icon")?.GetComponent<Image>();
        if (canvas == null) canvas = GetComponent<Canvas>();

        // Ensure canvas is World Space (though prefab should handle this)
        if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }
        
        // Hide initially
        UpdateStatus(0f, false, false);
    }

    void LateUpdate()
    {
        // Billboard: Always face the camera
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }

    public void UpdateStatus(float alertLevel, bool isCombat, bool isSeeing)
    {
        if (canvas == null) return;

        // Hide if unaware (alertLevel <= 0) and not in combat
        if (alertLevel <= 0 && !isCombat)
        {
            canvas.enabled = false;
            return;
        }

        canvas.enabled = true;

        // Update Meter (0 to 1)
        if (meterImage != null)
        {
            meterImage.fillAmount = alertLevel / 100f;
            
            // Color logic
            if (isCombat || alertLevel >= 100f)
                meterImage.color = alertColor;
            else if (alertLevel >= 50f)
                meterImage.color = suspiciousColor;
            else
                meterImage.color = normalColor;
        }

        // Update Icon
        if (iconImage != null)
        {
            if (isCombat || alertLevel >= 100f)
            {
                iconImage.sprite = exclamationIcon;
                iconImage.color = alertColor;
            }
            else
            {
                // If we see them, show Eye (Spotting)
                // If we don't see them (Audio/Suspicion), show Question (Searching)
                if (isSeeing)
                {
                    iconImage.sprite = eyeIcon;
                    iconImage.color = (alertLevel >= 50) ? suspiciousColor : normalColor;
                }
                else
                {
                    iconImage.sprite = questionIcon;
                    iconImage.color = (alertLevel >= 50) ? suspiciousColor : normalColor;
                }
            }
        }
    }
}
