using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FixPatrolPoints
{
    public static void Fix()
    {
        // Delete old patrol points
        GameObject oldParent = GameObject.Find("PatrolPoints");
        if (oldParent != null)
        {
            Object.DestroyImmediate(oldParent);
        }

        // Create new patrol points parent
        GameObject patrolParent = new GameObject("PatrolPoints");

        // Create a patrol route that covers the maze
        // Route: Bottom-left corner -> Top-left -> Top-right -> Bottom-right -> Center area
        Vector3[] positions = new Vector3[]
        {
            new Vector3(-22, 0, -22),  // PatrolPoint_A - Bottom-left
            new Vector3(-22, 0, 22),   // PatrolPoint_B - Top-left
            new Vector3(22, 0, 22),    // PatrolPoint_C - Top-right
            new Vector3(22, 0, -22),   // PatrolPoint_D - Bottom-right
            new Vector3(0, 0, 10),     // PatrolPoint_E - Near center (north)
            new Vector3(0, 0, -10),    // PatrolPoint_F - Near center (south)
        };

        string[] names = new string[]
        {
            "PatrolPoint_A",
            "PatrolPoint_B",
            "PatrolPoint_C",
            "PatrolPoint_D",
            "PatrolPoint_E",
            "PatrolPoint_F",
        };

        List<Transform> points = new List<Transform>();
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject point = new GameObject(names[i]);
            point.transform.position = positions[i];
            point.transform.parent = patrolParent.transform;
            points.Add(point.transform);
        }

        // Assign patrol points to the enemy
        GameObject enemyAgent = GameObject.Find("EnemyAgent");
        if (enemyAgent != null)
        {
            EnemyAI ai = enemyAgent.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.patrolPoints = points;
                ai.viewDistance = 15f;
                ai.viewAngle = 60f;
                EditorUtility.SetDirty(ai);
                Debug.Log($"Assigned {points.Count} patrol points to EnemyAgent.");
            }
        }

        // Log positions for verification
        foreach (var p in points)
        {
            Debug.Log($"Patrol point: {p.name} at {p.position}");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("Patrol points fixed!");
    }
}
