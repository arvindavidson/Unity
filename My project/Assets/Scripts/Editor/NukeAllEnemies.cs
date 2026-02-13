using UnityEngine;
using UnityEditor;

public class NukeAllEnemies
{
    public static void Nuke()
    {
        GameObject[] enemies = GameObject.FindObjectsOfType<GameObject>();
        int count = 0;
        foreach (GameObject go in enemies)
        {
            if (go.name == "EnemyAgent")
            {
                Object.DestroyImmediate(go);
                count++;
            }
        }
        Debug.Log($"Nuked {count} EnemyAgents.");
        
        // Now recreate one
        RecreateEnemyAgentComplete.Recreate();
    }
}
