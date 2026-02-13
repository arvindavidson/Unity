using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AssignPatrolPoints : MonoBehaviour
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
        
        // Get all patrol point children
        List<Transform> patrolPoints = new List<Transform>();
        foreach (Transform child in patrolPoints2Parent.transform)
        {
            patrolPoints.Add(child);
        }
        
        Debug.Log($"Found {enemies.Length} enemies and {patrolPoints.Count} patrol points in PatrolPoints2");
        
        // Find the enemy without patrol points assigned
        foreach (EnemyAI enemy in enemies)
        {
            if (enemy.patrolPoints == null || enemy.patrolPoints.Count == 0)
            {
                Debug.Log($"Assigning patrol points to {enemy.gameObject.name}");
                enemy.patrolPoints = patrolPoints;
                EditorUtility.SetDirty(enemy);
                Debug.Log($"Successfully assigned {patrolPoints.Count} patrol points to {enemy.gameObject.name}");
                return;
            }
        }
        
        Debug.LogWarning("All enemies already have patrol points assigned!");
    }
}
