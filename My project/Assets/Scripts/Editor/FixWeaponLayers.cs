using UnityEngine;
using UnityEditor;

public class FixWeaponLayers
{
    public static void Fix()
    {
        // Fix Player Weapon
        GameObject playerAgent = GameObject.Find("PlayerAgent");
        if (playerAgent != null)
        {
            Weapon w = playerAgent.GetComponent<Weapon>();
            if (w != null)
            {
                w.hitLayers = LayerMask.GetMask("Enemy", "Wall", "Default");
                EditorUtility.SetDirty(w);
                Debug.Log($"Player hitLayers set to: {w.hitLayers.value}");
            }
        }

        // Fix Enemy Weapon
        GameObject enemyAgent = GameObject.Find("EnemyAgent");
        if (enemyAgent != null)
        {
            Weapon w = enemyAgent.GetComponent<Weapon>();
            if (w != null)
            {
                w.hitLayers = LayerMask.GetMask("Player", "Wall", "Default");
                EditorUtility.SetDirty(w);
                Debug.Log($"Enemy hitLayers set to: {w.hitLayers.value}");
            }
        }

        // Also set Enemy tag on EnemyAgent
        try { GameObject.Find("EnemyAgent").tag = "Enemy"; } catch { }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("Weapon layers fixed!");
    }
}
