using UnityEngine;
using UnityEditor;
using UnityEditor.AI;

public class BakeNavMeshForScene
{
    public static string Execute()
    {
        // Mark ground and wall objects as Navigation Static
        var allObjects = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var mr in allObjects)
        {
            var go = mr.gameObject;
            if (go.name.StartsWith("Ground") || go.name.StartsWith("Wall"))
            {
                GameObjectUtility.SetStaticEditorFlags(go,
                    GameObjectUtility.GetStaticEditorFlags(go) | StaticEditorFlags.NavigationStatic);
                count++;
            }
        }

        // Bake NavMesh
        NavMeshBuilder.BuildNavMesh();

        return $"Marked {count} objects as NavigationStatic and baked NavMesh.";
    }
}
