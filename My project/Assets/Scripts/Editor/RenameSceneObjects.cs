using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RenameSceneObjects
{
    public static string Execute()
    {
        var scene = SceneManager.GetActiveScene();
        var rootObjects = scene.GetRootGameObjects();
        var log = new List<string>();

        // Track counts for duplicate naming
        var nameCounts = new Dictionary<string, int>();

        // First pass: count occurrences of each name
        foreach (var go in rootObjects)
        {
            string name = go.name;
            if (!nameCounts.ContainsKey(name))
                nameCounts[name] = 0;
            nameCounts[name]++;
        }

        // Second pass: rename duplicates
        var seenCount = new Dictionary<string, int>();
        foreach (var go in rootObjects)
        {
            string originalName = go.name;

            if (nameCounts[originalName] > 1)
            {
                if (!seenCount.ContainsKey(originalName))
                    seenCount[originalName] = 0;
                seenCount[originalName]++;

                string newName = $"{originalName}_{seenCount[originalName]:D2}";
                log.Add($"Renamed: {originalName} -> {newName}");
                Undo.RecordObject(go, "Rename " + originalName);
                go.name = newName;
                EditorUtility.SetDirty(go);
            }
        }

        // Also rename the nested Wall/Wall child if it exists
        foreach (var go in rootObjects)
        {
            if (go.name.StartsWith("Wall_"))
            {
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    var child = go.transform.GetChild(i);
                    if (child.name == "Wall")
                    {
                        // This is the nested Wall/Wall - unnest it and give it a wall number
                        string parentName = go.name;
                        string newName = "Wall_20";
                        log.Add($"Renamed nested child: {parentName}/Wall -> {newName} (will need manual unnesting)");
                        Undo.RecordObject(child.gameObject, "Rename nested Wall");
                        child.gameObject.name = newName;
                        // Move to root
                        Undo.SetTransformParent(child, null, "Unnest Wall_20");
                        EditorUtility.SetDirty(child.gameObject);
                    }
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        return $"Renamed {log.Count} objects:\n" + string.Join("\n", log);
    }
}
