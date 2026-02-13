using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AssignPatrolPointsToEnemy
{
    public static void Assign()
    {
        GameObject enemyAgent = GameObject.Find("EnemyAgent");
        if (enemyAgent == null) { Debug.LogError("EnemyAgent not found!"); return; }

        EnemyAI ai = enemyAgent.GetComponent<EnemyAI>();
        if (ai == null) { Debug.LogError("EnemyAI not found!"); return; }

        GameObject patrolParent = GameObject.Find("PatrolPoints");
        if (patrolParent == null) { Debug.LogError("PatrolPoints not found!"); return; }

        // Use SerializedObject to properly serialize the list
        SerializedObject so = new SerializedObject(ai);
        SerializedProperty patrolProp = so.FindProperty("patrolPoints");
        
        // Get all children
        List<Transform> children = new List<Transform>();
        foreach (Transform child in patrolParent.transform)
        {
            children.Add(child);
        }

        patrolProp.ClearArray();
        for (int i = 0; i < children.Count; i++)
        {
            patrolProp.InsertArrayElementAtIndex(i);
            patrolProp.GetArrayElementAtIndex(i).objectReferenceValue = children[i];
        }

        // Also update viewDistance and viewAngle
        so.FindProperty("viewDistance").floatValue = 15f;
        so.FindProperty("viewAngle").floatValue = 60f;

        so.ApplyModifiedProperties();

        Debug.Log($"Assigned {children.Count} patrol points to EnemyAgent via SerializedObject.");
        
        foreach (var c in children)
        {
            Debug.Log($"  - {c.name} at {c.position}");
        }
    }
}
