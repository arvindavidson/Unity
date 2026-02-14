using UnityEngine;
using UnityEditor;

/// <summary>
/// Fixes material assets to have non-emissive, properly dark HDRP settings.
/// </summary>
public class FixMaterialAssets
{
    public static string Execute()
    {
        string matFolder = "Assets/Materials";
        int fixed_ = 0;

        // Fix ground material
        var ground = AssetDatabase.LoadAssetAtPath<Material>($"{matFolder}/M_Ground.mat");
        if (ground != null)
        {
            ground.DisableKeyword("_EMISSION");
            SetSafe(ground, "_EmissiveColor", Color.black);
            SetSafe(ground, "_EmissionColor", Color.black);
            SetSafe(ground, "_BaseColor", new Color(0.25f, 0.28f, 0.22f, 1f));
            SetSafe(ground, "_Color", new Color(0.25f, 0.28f, 0.22f, 1f));
            SetSafe(ground, "_Smoothness", 0.15f);
            SetSafe(ground, "_EmissiveIntensity", 0f);
            EditorUtility.SetDirty(ground);
            fixed_++;
        }

        // Fix wall material
        var wall = AssetDatabase.LoadAssetAtPath<Material>($"{matFolder}/M_Wall.mat");
        if (wall != null)
        {
            wall.DisableKeyword("_EMISSION");
            SetSafe(wall, "_EmissiveColor", Color.black);
            SetSafe(wall, "_EmissionColor", Color.black);
            SetSafe(wall, "_BaseColor", new Color(0.45f, 0.42f, 0.38f, 1f));
            SetSafe(wall, "_Color", new Color(0.45f, 0.42f, 0.38f, 1f));
            SetSafe(wall, "_Smoothness", 0.25f);
            SetSafe(wall, "_EmissiveIntensity", 0f);
            EditorUtility.SetDirty(wall);
            fixed_++;
        }

        // Fix cover material (same as wall but slightly different shade)
        // Cover uses Wall_ prefix so same material

        // Fix treasure material
        var treasure = AssetDatabase.LoadAssetAtPath<Material>($"{matFolder}/M_Treasure.mat");
        if (treasure != null)
        {
            treasure.DisableKeyword("_EMISSION");
            SetSafe(treasure, "_EmissiveColor", Color.black);
            SetSafe(treasure, "_EmissionColor", Color.black);
            SetSafe(treasure, "_BaseColor", new Color(0.9f, 0.7f, 0.1f, 1f));
            SetSafe(treasure, "_Color", new Color(0.9f, 0.7f, 0.1f, 1f));
            SetSafe(treasure, "_Smoothness", 0.6f);
            SetSafe(treasure, "_EmissiveIntensity", 0f);
            EditorUtility.SetDirty(treasure);
            fixed_++;
        }

        AssetDatabase.SaveAssets();
        return $"Fixed {fixed_} material assets (emission disabled, colors corrected).";
    }

    static void SetSafe(Material mat, string prop, Color c)
    {
        if (mat.HasProperty(prop)) mat.SetColor(prop, c);
    }

    static void SetSafe(Material mat, string prop, float v)
    {
        if (mat.HasProperty(prop)) mat.SetFloat(prop, v);
    }
}
