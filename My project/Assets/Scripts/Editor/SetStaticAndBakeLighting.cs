using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class SetStaticAndBakeLighting : MonoBehaviour
{
    public static void Execute()
    {
        int staticCount = 0;

        // Find all GameObjects in the scene
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (var obj in allObjects)
        {
            string name = obj.name;

            // Set Ground, Wall, Treasure, Sky/Fog, StaticLightingSky as static
            if (name == "Ground" || name == "Wall" || name == "Treasure" ||
                name == "Sky and Fog Volume" || name == "StaticLightingSky")
            {
                if (!obj.isStatic)
                {
                    GameObjectUtility.SetStaticEditorFlags(obj,
                        StaticEditorFlags.ContributeGI |
                        StaticEditorFlags.OccluderStatic |
                        StaticEditorFlags.OccludeeStatic |
                        StaticEditorFlags.BatchingStatic |
                        StaticEditorFlags.ReflectionProbeStatic);
                    EditorUtility.SetDirty(obj);
                    staticCount++;
                    Debug.Log($"Set '{name}' to static.");
                }
                else
                {
                    Debug.Log($"'{name}' was already static.");
                }
            }
        }

        Debug.Log($"Total objects set to static: {staticCount}");

        // Start lightmap baking
        Debug.Log("Starting lightmap bake...");
        Lightmapping.BakeAsync();
        Debug.Log("Lightmap bake started asynchronously. Check Unity Editor for progress.");
    }
}
