using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [Header("Crosshair Settings")]
    public float crosshairSize = 20f;
    public Color crosshairColor = Color.white;

    [Header("Aim Point")]
    public Vector3 aimPoint; // World position where the crosshair is aiming (on the ground plane)
    public bool hasAimPoint;

    private RectTransform rectTransform;
    private Image image;

    public static Crosshair Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(crosshairSize, crosshairSize);
        }

        if (image != null)
        {
            image.color = crosshairColor;
        }

        // Hide the OS cursor
        Cursor.visible = false;
    }

    void Update()
    {
        // Move crosshair UI to follow mouse
        if (rectTransform != null)
        {
            rectTransform.position = Input.mousePosition;
        }

        // Raycast from mouse to find the aim point on the ground plane
        UpdateAimPoint();
    }

    void UpdateAimPoint()
    {
        hasAimPoint = false;

        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float distance;

        if (groundPlane.Raycast(ray, out distance))
        {
            aimPoint = ray.GetPoint(distance);
            hasAimPoint = true;
        }
    }

    void OnDestroy()
    {
        Cursor.visible = true;
        if (Instance == this) Instance = null;
    }
}
