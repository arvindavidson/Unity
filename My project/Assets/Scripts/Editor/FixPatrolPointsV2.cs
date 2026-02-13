using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FixPatrolPointsV2
{
    public static void Fix()
    {
        // Delete ALL objects named "PatrolPoints"
        while (true)
        {
            GameObject pp = GameObject.Find("PatrolPoints");
            if (pp == null) break;
            Object.DestroyImmediate(pp);
        }

        // Also delete any stray PatrolPoint objects
        while (true)
        {
            GameObject pp = GameObject.Find("PatrolPoint");
            if (pp == null) break;
            Object.DestroyImmediate(pp);
        }

        // Create new patrol points parent
        GameObject patrolParent = new GameObject("PatrolPoints");

        Vector3[] positions = new Vector3[]
        {
            new Vector3(-22, 0, -22),  // Bottom-left
            new Vector3(-22, 0, 22),   // Top-left
            new Vector3(22, 0, 22),    // Top-right
            new Vector3(22, 0, -22),   // Bottom-right
            new Vector3(0, 0, 10),     // Near center north
            new Vector3(0, 0, -10),    // Near center south
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

        // Assign to enemy
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
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log($"Created {points.Count} patrol points and assigned to EnemyAgent.");
    }
}
