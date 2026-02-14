using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateStealthIcons : MonoBehaviour
{
    [MenuItem("Tools/Generate Stealth Icons")]
    public static void GenerateIcons()
    {
        string path = "Assets/Textures/UI";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        CreateIcon(path + "/Icon_Eye.png", Color.white, "Eye");
        CreateIcon(path + "/Icon_Question.png", Color.yellow, "?");
        CreateIcon(path + "/Icon_Exclamation.png", Color.red, "!");
        CreateIcon(path + "/Meter_Circle.png", Color.white, "Circle");
        
        AssetDatabase.Refresh();
        Debug.Log("Stealth Icons Generated in " + path);
    }

    static void CreateIcon(string filePath, Color color, string symbol)
    {
        int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        // Fill transparent
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
        texture.SetPixels(pixels);

        // Draw Circle Background
        Vector2 center = new Vector2(size / 2, size / 2);
        float radius = size / 2 - 4;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                {
                    // circular background (semi-transparent black)
                    texture.SetPixel(x, y, new Color(0, 0, 0, 0.5f));
                }
                // Border
                if (dist > radius - 4 && dist <= radius)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
        
        // Simple Text drawing is hard in Texture2D without font, 
        // so let's just draw simple shapes.
        
        if (symbol == "!")
        {
            // Draw Exclamation
            DrawRect(texture, size/2 - 10, 20, 20, 60, color);
            DrawRect(texture, size/2 - 10, 90, 20, 20, color);
        }
        else if (symbol == "?")
        {
            // Draw Box for now (Question mark is hard to draw pixel by pixel manually quickly)
            DrawRect(texture, size/2 - 10, 20, 20, 20, color); // Dot
            DrawRect(texture, size/2 - 20, 50, 40, 10, color); // Bottom Line
            DrawRect(texture, size/2 + 10, 60, 10, 30, color); // Right Line
            DrawRect(texture, size/2 - 20, 90, 40, 10, color); // Top Line
        }
        else if (symbol == "Eye")
        {
            // Draw Eye Shape (Diamond-ish)
             for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (Mathf.Abs(x - size/2) + Mathf.Abs(y - size/2) < 40)
                    {
                         texture.SetPixel(x, y, color);
                    }
                    if (Mathf.Abs(x - size/2) + Mathf.Abs(y - size/2) < 15)
                    {
                         texture.SetPixel(x, y, Color.black); // Pupil
                    }
                }
            }
        }
        else if (symbol == "Circle")
        {
            // Draw a solid circle (for Radial Fill)
            // Re-clear to fully transparent first (override background)
            for (int i = 0; i < pixels.Length; i++) texture.SetPixel(i % size, i / size, Color.clear);
            
            float r = size / 2 - 2;
            center = new Vector2(size/2, size/2);
             for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= r)
                    {
                        texture.SetPixel(x, y, color);
                    }
                    // Anti-aliasing-ish edge
                    else if (dist <= r + 1)
                    {
                        texture.SetPixel(x, y, new Color(color.r, color.g, color.b, 0.5f));
                    }
                }
            }
        }

        texture.Apply();
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
    }

    static void DrawRect(Texture2D tex, int x, int y, int w, int h, Color col)
    {
        for (int i = x; i < x + w; i++)
        {
            for (int j = y; j < y + h; j++)
            {
                tex.SetPixel(i, j, col);
            }
        }
    }
}
