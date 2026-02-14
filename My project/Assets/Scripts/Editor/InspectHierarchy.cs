using UnityEngine;
using UnityEditor;

public class InspectHierarchy : MonoBehaviour
{
    [MenuItem("Tools/Inspect Enemy Hierarchy")]
    public static void Inspect()
    {
        EnemyAI enemy = FindFirstObjectByType<EnemyAI>();
        if (enemy == null) 
        {
            Debug.LogError("No EnemyAI found in scene.");
            return;
        }

        Debug.Log("Inspecting Enemy: " + enemy.name);
        foreach (Transform child in enemy.transform)
        {
            Debug.Log("- Child: " + child.name);
            EnemyStatusUI ui = child.GetComponent<EnemyStatusUI>();
            if (ui != null) Debug.Log("  -> Has EnemyStatusUI component");
        }
        
        EnemyStatusUI rootUI = enemy.GetComponent<EnemyStatusUI>();
        if (rootUI != null) Debug.Log("Root has EnemyStatusUI component (unexpected!)");
    }
}
