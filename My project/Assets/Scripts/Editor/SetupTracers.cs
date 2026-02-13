using UnityEngine;
using UnityEditor;

public class SetupTracers
{
    public static void Setup()
    {
        // Create Tracer Prefab
        GameObject tracerGO = new GameObject("BulletTracer");
        LineRenderer lr = tracerGO.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.yellow;
        lr.endColor = new Color(1, 0.92f, 0.016f, 0f); // Yellow fading to transparent
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        
        BulletTracer bt = tracerGO.AddComponent<BulletTracer>();
        bt.lineRenderer = lr;
        bt.duration = 0.1f;

        // Save as Prefab
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
            
        string prefabPath = "Assets/Prefabs/BulletTracer.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tracerGO, prefabPath);
        Object.DestroyImmediate(tracerGO);

        // Assign to Player
        GameObject playerAgent = GameObject.Find("PlayerAgent");
        if (playerAgent != null)
        {
            Weapon w = playerAgent.GetComponent<Weapon>();
            if (w != null) w.tracerPrefab = prefab;
        }

        // Assign to Enemy
        GameObject enemyAgent = GameObject.Find("EnemyAgent");
        if (enemyAgent != null)
        {
            Weapon w = enemyAgent.GetComponent<Weapon>();
            if (w != null) w.tracerPrefab = prefab;
        }
        
        Debug.Log("Tracers setup and assigned!");
    }
}
