using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Crosshair : MonoBehaviour
{
    [Header("Crosshair Settings")]
    public float crosshairSize = 20f;
    public Color crosshairColor = Color.white;
    public float aimDistance = 5f; // Distance for controller aiming

    [Header("Aim Point")]
    public Vector3 aimPoint; // World position where the crosshair is aiming (on the ground plane)
    public bool hasAimPoint;

    private RectTransform rectTransform;
    private Image image;
    private InputSystem_Actions inputActions;
    private PlayerController playerController;

    public static Crosshair Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        
        // Ensure Pivot is Center so position updates correctly
        if (rectTransform != null)
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            
        inputActions = new InputSystem_Actions();

        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(crosshairSize, crosshairSize);
        }

        if (image != null)
        {
            image.color = crosshairColor;
        }
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Update()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        if (playerController != null)
        {
            if (rectTransform != null)
            {
                rectTransform.position = playerController.CrosshairScreenPosition;
            }
            
            // Optional: Raycast from the Crosshair position to update 'aimPoint' visualizer if needed
            // But since PlayerController now handles the logic, we might just want to 
            // set 'aimPoint' to where the player is looking for debug purposes.
            // For now, let's keep it simple.
            
            // To maintain compatibility with existing 'aimPoint' usage in this script:
            Ray ray = Camera.main.ScreenPointToRay(playerController.CrosshairScreenPosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float distance;
            if (groundPlane.Raycast(ray, out distance))
            {
                aimPoint = ray.GetPoint(distance);
                hasAimPoint = true;
            }
        }
    }

    void OnDestroy()
    {
        Cursor.visible = true;
        if (Instance == this) Instance = null;
    }
}
