using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class OrganizeHierarchy
{
    public static string Execute()
    {
        var scene = SceneManager.GetActiveScene();
        var rootObjects = scene.GetRootGameObjects().ToList();
        var log = new List<string>();

        // Create parent groups at origin
        var lighting = CreateGroup("Lighting");
        var environment = CreateGroup("Environment");
        var agents = CreateGroup("Agents");
        var ai = CreateGroup("AI");
        var cameras = CreateGroup("Cameras");
        var ui = CreateGroup("UI");
        var systems = CreateGroup("Systems");

        // Define groupings
        var lightingNames = new HashSet<string> { "Sun", "Sky and Fog Volume", "StaticLightingSky", "Adaptive Probe Volume", "ProbeVolumePerSceneData" };
        var cameraNames = new HashSet<string> { "Main Camera", "CM_TopDownCamera" };
        var uiNames = new HashSet<string> { "Canvas", "EventSystem" };
        var systemNames = new HashSet<string> { "GameManager" };
        var agentNames = new HashSet<string> { "PlayerAgent", "EnemyAgent_01", "EnemyAgent_02" };
        var aiNames = new HashSet<string> { "PatrolPoints", "PatrolPoints2" };

        foreach (var go in rootObjects)
        {
            Transform target = null;
            string groupName = "";

            if (lightingNames.Contains(go.name))
            {
                target = lighting.transform;
                groupName = "Lighting";
            }
            else if (cameraNames.Contains(go.name))
            {
                target = cameras.transform;
                groupName = "Cameras";
            }
            else if (uiNames.Contains(go.name))
            {
                target = ui.transform;
                groupName = "UI";
            }
            else if (systemNames.Contains(go.name))
            {
                target = systems.transform;
                groupName = "Systems";
            }
            else if (agentNames.Contains(go.name))
            {
                target = agents.transform;
                groupName = "Agents";
            }
            else if (aiNames.Contains(go.name))
            {
                target = ai.transform;
                groupName = "AI";
            }
            else if (go.name.StartsWith("Wall_") || go.name.StartsWith("Ground_") || go.name.StartsWith("Treasure_"))
            {
                target = environment.transform;
                groupName = "Environment";
            }

            if (target != null)
            {
                Undo.SetTransformParent(go.transform, target, "Parent " + go.name);
                log.Add($"  {go.name} -> {groupName}");
            }
        }

        // Set sibling order for groups
        lighting.transform.SetSiblingIndex(0);
        environment.transform.SetSiblingIndex(1);
        agents.transform.SetSiblingIndex(2);
        ai.transform.SetSiblingIndex(3);
        cameras.transform.SetSiblingIndex(4);
        ui.transform.SetSiblingIndex(5);
        systems.transform.SetSiblingIndex(6);

        EditorSceneManager.MarkSceneDirty(scene);
        return $"Organized {log.Count} objects into groups:\n" + string.Join("\n", log);
    }

    private static GameObject CreateGroup(string name)
    {
        var go = new GameObject(name);
        go.transform.position = Vector3.zero;
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return go;
    }
}
