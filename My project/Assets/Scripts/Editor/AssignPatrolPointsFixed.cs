using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class AssignPatrolPointsFixed : MonoBehaviour
{
    public static void Execute()
    {
        // Find all enemies
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        
        // Find PatrolPoints2 parent
        GameObject patrolPoints2Parent = GameObject.Find("PatrolPoints2");
        if (patrolPoints2Parent == null)
        {
            Debug.LogError("PatrolPoints2 not found!");
            return;
        }
        
        // Get all patrol point children from PatrolPoints2
        List<Transform> patrolPoints2 = new List<Transform>();
        foreach (Transform child in patrolPoints2Parent.transform)
        {
            patrolPoints2.Add(child);
        }
        
        Debug.Log($"Found {enemies.Length} enemies and {patrolPoints2.Count} patrol points in PatrolPoints2");
        
        // Check current assignments
        for (int i = 0; i < enemies.Length; i++)
        {
            var enemy = enemies[i];
            string pointsInfo = enemy.patrolPoints != null && enemy.patrolPoints.Count > 0 
                ? $"{enemy.patrolPoints.Count} points: {string.Join(", ", enemy.patrolPoints.Select(p => p != null ? p.name : "null"))}"
                : "none";
            Debug.Log($"Enemy {i} ({enemy.gameObject.name}): {pointsInfo}");
        }
        
        // Assign PatrolPoints2 to the second enemy (index 1)
        if (enemies.Length >= 2)
        {
            Debug.Log($"Assigning PatrolPoints2 to second enemy: {enemies[1].gameObject.name}");
            enemies[1].patrolPoints = patrolPoints2;
            EditorUtility.SetDirty(enemies[1]);
            Debug.Log($"Successfully assigned {patrolPoints2.Count} patrol points from PatrolPoints2 to {enemies[1].gameObject.name}");
        }
        else
        {
            Debug.LogWarning("Not enough enemies found!");
        }
    }
}
