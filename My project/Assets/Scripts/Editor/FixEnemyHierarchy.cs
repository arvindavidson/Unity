using UnityEngine;
using UnityEditor;
using UnityEditor.AI;

public class FixEnemyHierarchy
{
    public static void Fix()
    {
        GameObject enemyAgent = GameObject.Find("EnemyAgent");
        if (enemyAgent == null)
        {
            Debug.LogError("EnemyAgent not found!");
            return;
        }

        Transform enemyChild = enemyAgent.transform.Find("Enemy");
        if (enemyChild == null)
        {
            Debug.LogError("Enemy child not found under EnemyAgent!");
            return;
        }

        // Remove components from child
        DestroyComponent<UnityEngine.AI.NavMeshAgent>(enemyChild.gameObject);
        DestroyComponent<EnemyAI>(enemyChild.gameObject);
        DestroyComponent<Health>(enemyChild.gameObject);
        DestroyComponent<Weapon>(enemyChild.gameObject);
        
        // Ensure parent has components
        EnsureComponent<UnityEngine.AI.NavMeshAgent>(enemyAgent);
        EnsureComponent<EnemyAI>(enemyAgent);
        EnsureComponent<Health>(enemyAgent);
        EnsureComponent<Weapon>(enemyAgent);

        // Fix NavMeshAgent on parent
        UnityEngine.AI.NavMeshAgent agent = enemyAgent.GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.baseOffset = 0;
        agent.height = 2;
        agent.speed = 3.5f;
        agent.angularSpeed = 120;
        agent.acceleration = 8;

        // Fix Weapon FirePoint on parent
        Weapon weapon = enemyAgent.GetComponent<Weapon>();
        Transform firePoint = enemyAgent.transform.Find("FirePoint");
        if (firePoint == null)
        {
            firePoint = new GameObject("FirePoint").transform;
            firePoint.parent = enemyAgent.transform;
            firePoint.localPosition = new Vector3(0, 1.5f, 0.5f);
        }
        weapon.firePoint = firePoint;
        weapon.hitLayers = LayerMask.GetMask("Player", "Wall");
        
        // Assign Tracer Prefab
        string prefabPath = "Assets/Prefabs/BulletTracer.prefab";
        GameObject tracerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (tracerPrefab != null)
        {
            weapon.tracerPrefab = tracerPrefab;
        }
        else
        {
            Debug.LogWarning("BulletTracer prefab not found at " + prefabPath);
        }

        // Fix EnemyAI references
        EnemyAI ai = enemyAgent.GetComponent<EnemyAI>();
        if (ai.patrolPoints == null || ai.patrolPoints.Count == 0)
        {
            GameObject patrolPointsParent = GameObject.Find("PatrolPoints");
            if (patrolPointsParent != null)
            {
                ai.patrolPoints = new System.Collections.Generic.List<Transform>();
                foreach (Transform child in patrolPointsParent.transform)
                {
                    ai.patrolPoints.Add(child);
                }
            }
        }

        // Ensure Layers
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
        {
            enemyAgent.layer = enemyLayer;
            enemyChild.gameObject.layer = enemyLayer;
            // Also set layer for Visor if it exists
            Transform visor = enemyAgent.transform.Find("Visor");
            if (visor != null) visor.gameObject.layer = enemyLayer;
        }

        Debug.Log("Enemy hierarchy fixed!");
    }

    static void DestroyComponent<T>(GameObject go) where T : Component
    {
        var comp = go.GetComponent<T>();
        if (comp != null) Object.DestroyImmediate(comp);
    }

    static void EnsureComponent<T>(GameObject go) where T : Component
    {
        if (go.GetComponent<T>() == null) go.AddComponent<T>();
    }
}
