using UnityEngine;
using UnityEditor;
using UnityEditor.AI;
using System.Collections.Generic;

public class RecreateEnemyAgentComplete
{
    public static void Recreate()
    {
        GameObject oldAgent = GameObject.Find("EnemyAgent");
        if (oldAgent != null)
        {
            Object.DestroyImmediate(oldAgent);
        }

        // Create Parent
        GameObject enemyAgent = new GameObject("EnemyAgent");
        enemyAgent.transform.position = new Vector3(25, 1, 25); // Start pos from level setup
        
        // Add Components to Parent
        UnityEngine.AI.NavMeshAgent agent = enemyAgent.AddComponent<UnityEngine.AI.NavMeshAgent>();
        agent.baseOffset = 0;
        agent.height = 2;
        agent.speed = 3.5f;
        agent.angularSpeed = 120;
        agent.acceleration = 8;

        EnemyAI ai = enemyAgent.AddComponent<EnemyAI>();
        enemyAgent.AddComponent<Health>();
        Weapon weapon = enemyAgent.AddComponent<Weapon>();

        // Create Visual Child
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "EnemyVisual";
        visual.transform.SetParent(enemyAgent.transform);
        visual.transform.localPosition = new Vector3(0, 1, 0);
        
        // Set Visual Layer & Color
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) 
        {
            enemyAgent.layer = enemyLayer;
            visual.layer = enemyLayer;
        }

        Renderer r = visual.GetComponent<Renderer>();
        if (r != null)
        {
            r.material = new Material(Shader.Find("HDRP/Lit"));
            r.material.color = Color.red;
        }

        // Create FirePoint
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(enemyAgent.transform);
        firePoint.transform.localPosition = new Vector3(0, 1.5f, 0.5f);
        
        // Setup Weapon
        weapon.firePoint = firePoint.transform;
        weapon.hitLayers = LayerMask.GetMask("Player", "Wall");
        
        string prefabPath = "Assets/Prefabs/BulletTracer.prefab";
        GameObject tracerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (tracerPrefab != null) weapon.tracerPrefab = tracerPrefab;

        // Setup AI
        GameObject patrolPointsParent = GameObject.Find("PatrolPoints");
        if (patrolPointsParent != null)
        {
            ai.patrolPoints = new List<Transform>();
            foreach (Transform child in patrolPointsParent.transform)
            {
                ai.patrolPoints.Add(child);
            }
        }

        // Add Visor
        AddEnemyVisor.Add(); // This finds "EnemyAgent" which we just created

        Debug.Log("EnemyAgent completely recreated!");
    }
}
