using UnityEngine;
using UnityEditor;

public class DebugAssetLoad : MonoBehaviour
{
    [MenuItem("Tools/Debug Asset Load")]
    public static void DebugLoad()
    {
        string path = "Assets/Textures/UI/Icon_Eye.png";
        
        // 1. Check file existence
        if (!System.IO.File.Exists(path))
        {
             Debug.LogError("File does NOT exist at project path: " + path);
             // Try absolute path check
             Debug.Log("Absolute path check: " + System.IO.Path.GetFullPath(path));
             return;
        }
        else
        {
             Debug.Log("File exists at: " + path);
        }

        // 2. Refresh
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        // 3. Load as Object
        Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
        Debug.Log($"LoadAssetAtPath<Object>: {obj} (Type: {(obj != null ? obj.GetType().Name : "null")})");

        // 4. Load as Texture2D
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        Debug.Log($"LoadAssetAtPath<Texture2D>: {tex}");

        // 5. Load as Sprite
        Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        Debug.Log($"LoadAssetAtPath<Sprite>: {spr}");
        
        // 6. Check Importer
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            Debug.Log($"Importer Type: {importer.textureType}");
            if (importer.textureType != TextureImporterType.Sprite)
            {
                Debug.Log("Fixing Importer to Sprite...");
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
                
                // Retry load
                spr = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                 Debug.Log($"Retry LoadAssetAtPath<Sprite>: {spr}");
            }
        }
        else
        {
            Debug.LogError("No TextureImporter found!");
        }
    }
}
