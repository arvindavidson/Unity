using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FixSceneEnemies : MonoBehaviour
{
    [MenuItem("Tools/Fix Scene Enemies (Remove Duplicate UI)")]
    public static void FixEnemies()
    {
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        int removedCount = 0;
        List<GameObject> objectsToDestroy = new List<GameObject>();

        foreach (EnemyAI enemy in enemies)
        {
            // Find all children named "EnemyStatusCanvas"
            // GetComponentsInChildren can return the object's own components too if the name matched, 
            // but we are looking for Transforms to identify GameObjects.
            Transform[] children = enemy.GetComponentsInChildren<Transform>(true);
            
            // List all candidate canvases attached to this enemy
            List<Transform> canvases = new List<Transform>();
            foreach (Transform t in children)
            {
                if (t.name == "EnemyStatusCanvas" && t.parent == enemy.transform)
                {
                    canvases.Add(t);
                }
            }
            
            // If more than one found, keep the one that is part of the prefab, destroy others
            if (canvases.Count > 1)
            {
                Debug.Log("Found " + canvases.Count + " UI overlays on " + enemy.name);
                
                foreach (Transform t in canvases)
                {
                    // Check if part of prefab
                    // If it is NOT part of the prefab instance, it's a candidate for deletion
                    // checks if the object is part of a prefab instance
                    bool isPrefab = PrefabUtility.IsPartOfPrefabInstance(t.gameObject); 
                    
                    if (!isPrefab)
                    {
                        objectsToDestroy.Add(t.gameObject);
                        Debug.Log("Marked local overlay for deletion: " + t.name + " on " + enemy.name);
                    }
                    else
                    {
                        Debug.Log("Keeping prefab overlay: " + t.name + " on " + enemy.name);
                    }
                }
            }
        }
        
        // Execute destruction safely
        foreach (GameObject obj in objectsToDestroy)
        {
            if (obj != null)
            {
                Undo.DestroyObjectImmediate(obj); // Support Undo
                removedCount++;
            }
        }
        
        Debug.Log("Cleanup Complete. Removed " + removedCount + " duplicate UI objects.");
    }
}
