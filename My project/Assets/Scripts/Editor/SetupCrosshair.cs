using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class SetupCrosshair : MonoBehaviour
{
    public static void Execute()
    {
        // 1. Check if Canvas exists, or create one
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        GameObject canvasObj;

        if (canvas == null)
        {
            canvasObj = new GameObject("UICanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // On top of everything
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
            Debug.Log("Created new UICanvas.");
        }
        else
        {
            canvasObj = canvas.gameObject;
            Debug.Log($"Using existing Canvas: {canvasObj.name}");
        }

        // 2. Remove existing crosshair if any
        Transform existingCrosshair = canvasObj.transform.Find("Crosshair");
        if (existingCrosshair != null)
        {
            DestroyImmediate(existingCrosshair.gameObject);
            Debug.Log("Removed existing Crosshair.");
        }

        // 3. Create the crosshair container as a proper UI object
        GameObject crosshairObj = new GameObject("Crosshair", typeof(RectTransform));
        crosshairObj.transform.SetParent(canvasObj.transform, false);
        Undo.RegisterCreatedObjectUndo(crosshairObj, "Create Crosshair");

        // 4. Set up RectTransform
        RectTransform rt = crosshairObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(20, 20);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);

        // 5. Create a procedural crosshair texture (small + shape)
        int texSize = 32;
        Texture2D tex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color transparent = new Color(0, 0, 0, 0);
        Color white = Color.white;
        Color outline = new Color(0, 0, 0, 0.8f);

        // Fill with transparent
        Color[] pixels = new Color[texSize * texSize];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;

        int center = texSize / 2;
        int thickness = 1;
        int gap = 3;  // Gap around center
        int armLength = 6;

        // Draw crosshair arms with outline
        for (int i = gap; i <= armLength + gap; i++)
        {
            for (int t = -thickness; t <= thickness; t++)
            {
                // Outline (thicker)
                for (int o = -1; o <= 1; o++)
                {
                    // Right
                    SetPixelSafe(pixels, texSize, center + i + o, center + t, outline);
                    // Left
                    SetPixelSafe(pixels, texSize, center - i + o, center + t, outline);
                    // Up
                    SetPixelSafe(pixels, texSize, center + t, center + i + o, outline);
                    // Down
                    SetPixelSafe(pixels, texSize, center + t, center - i + o, outline);
                }

                // White core
                // Right
                SetPixelSafe(pixels, texSize, center + i, center + t, white);
                // Left
                SetPixelSafe(pixels, texSize, center - i, center + t, white);
                // Up
                SetPixelSafe(pixels, texSize, center + t, center + i, white);
                // Down
                SetPixelSafe(pixels, texSize, center + t, center - i, white);
            }
        }

        // Small center dot
        SetPixelSafe(pixels, texSize, center, center, white);

        tex.SetPixels(pixels);
        tex.Apply();

        // 6. Add Image component with the procedural texture
        Image img = crosshairObj.AddComponent<Image>();
        Sprite crosshairSprite = Sprite.Create(tex, new Rect(0, 0, texSize, texSize), new Vector2(0.5f, 0.5f));
        img.sprite = crosshairSprite;
        img.raycastTarget = false;  // Don't block clicks
        img.color = Color.white;

        // 7. Add the Crosshair script
        Crosshair crosshairScript = crosshairObj.AddComponent<Crosshair>();
        crosshairScript.crosshairSize = 24f;
        crosshairScript.crosshairColor = Color.white;

        EditorUtility.SetDirty(crosshairObj);
        EditorUtility.SetDirty(canvasObj);

        Debug.Log("Crosshair setup complete! Small crosshair with center dot created.");
    }

    static void SetPixelSafe(Color[] pixels, int texSize, int x, int y, Color color)
    {
        if (x >= 0 && x < texSize && y >= 0 && y < texSize)
        {
            int idx = y * texSize + x;
            // Only overwrite if the new color has higher alpha (white overwrites outline)
            if (color.a >= pixels[idx].a || color == Color.white)
            {
                pixels[idx] = color;
            }
        }
    }
}
