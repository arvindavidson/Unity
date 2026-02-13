using UnityEngine;
using UnityEditor;
using System.IO;

public class AssignLevelMaterials : MonoBehaviour
{
    public static void Execute()
    {
        string matFolder = "Assets/Materials";
        if (!AssetDatabase.IsValidFolder(matFolder))
            AssetDatabase.CreateFolder("Assets", "Materials");

        // --- Create materials ---
        // Ground: dark muted green-gray (like concrete/outdoor ground)
        Material groundMat = CreateOrGetMat(matFolder, "M_Ground", new Color(0.25f, 0.3f, 0.22f, 1f));

        // Walls: medium warm gray (like stone/concrete walls)
        Material wallMat = CreateOrGetMat(matFolder, "M_Wall", new Color(0.45f, 0.42f, 0.4f, 1f));

        // Treasure: gold/yellow
        Material treasureMat = CreateOrGetMat(matFolder, "M_Treasure", new Color(0.85f, 0.7f, 0.15f, 1f));
        SetMetallic(treasureMat, 0.7f, 0.4f);

        // Player body: deep blue
        Material playerBodyMat = CreateOrGetMat(matFolder, "M_PlayerBody", new Color(0.15f, 0.3f, 0.65f, 1f));

        // Player visor: cyan/teal glow
        Material playerVisorMat = CreateOrGetMat(matFolder, "M_PlayerVisor", new Color(0.1f, 0.8f, 0.9f, 1f));
        SetEmission(playerVisorMat, new Color(0.1f, 0.8f, 0.9f) * 1.5f);

        // Enemy body: dark red
        Material enemyBodyMat = CreateOrGetMat(matFolder, "M_EnemyBody", new Color(0.65f, 0.15f, 0.12f, 1f));

        // Enemy visor: orange-red glow
        Material enemyVisorMat = CreateOrGetMat(matFolder, "M_EnemyVisor", new Color(1f, 0.35f, 0.1f, 1f));
        SetEmission(enemyVisorMat, new Color(1f, 0.35f, 0.1f) * 1.5f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // --- Assign to scene objects ---
        int groundCount = 0, wallCount = 0, treasureCount = 0;

        // Find all renderers in the scene
        MeshRenderer[] allRenderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);

        foreach (var renderer in allRenderers)
        {
            string name = renderer.gameObject.name;
            string parentName = renderer.transform.parent != null ? renderer.transform.parent.name : "";

            // Ground
            if (name == "Ground")
            {
                renderer.sharedMaterial = groundMat;
                groundCount++;
            }
            // Walls
            else if (name == "Wall")
            {
                renderer.sharedMaterial = wallMat;
                wallCount++;
            }
            // Treasure
            else if (name == "Treasure")
            {
                renderer.sharedMaterial = treasureMat;
                treasureCount++;
            }
            // Player body
            else if (name == "PlayerBody" && parentName == "PlayerAgent")
            {
                renderer.sharedMaterial = playerBodyMat;
            }
            // Player visor
            else if (name == "Visor" && parentName == "PlayerAgent")
            {
                renderer.sharedMaterial = playerVisorMat;
            }
            // Enemy body
            else if (name == "EnemyBody" && parentName == "EnemyAgent")
            {
                renderer.sharedMaterial = enemyBodyMat;
            }
            // Enemy visor
            else if (name == "Visor" && parentName == "EnemyAgent")
            {
                renderer.sharedMaterial = enemyVisorMat;
            }

            EditorUtility.SetDirty(renderer);
        }

        Debug.Log($"Materials assigned! Ground: {groundCount}, Walls: {wallCount}, Treasures: {treasureCount}");
        Debug.Log("Player: blue body + cyan visor | Enemies: red body + orange visor | Treasure: gold");
    }

    static Material CreateOrGetMat(string folder, string matName, Color color)
    {
        string path = $"{folder}/{matName}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (mat == null)
        {
            // Use HDRP Lit shader
            Shader hdrpLit = Shader.Find("HDRP/Lit");
            if (hdrpLit == null)
            {
                hdrpLit = Shader.Find("Standard");
            }

            mat = new Material(hdrpLit);
            mat.name = matName;
            mat.SetColor("_BaseColor", color);
            mat.color = color;
            AssetDatabase.CreateAsset(mat, path);
            Debug.Log($"Created material: {path}");
        }
        else
        {
            mat.SetColor("_BaseColor", color);
            mat.color = color;
            EditorUtility.SetDirty(mat);
            Debug.Log($"Updated material: {path}");
        }

        return mat;
    }

    static void SetMetallic(Material mat, float metallic, float smoothness)
    {
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(mat);
    }

    static void SetEmission(Material mat, Color emissionColor)
    {
        mat.EnableKeyword("_EMISSION");
        // HDRP uses _EmissiveColor
        if (mat.HasProperty("_EmissiveColor"))
        {
            mat.SetColor("_EmissiveColor", emissionColor);
            mat.SetFloat("_UseEmissiveIntensity", 0);
        }
        else if (mat.HasProperty("_EmissionColor"))
        {
            mat.SetColor("_EmissionColor", emissionColor);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        EditorUtility.SetDirty(mat);
    }
}
