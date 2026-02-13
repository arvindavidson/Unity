using UnityEngine;
using UnityEditor;
using UnityEditor.AI;
using System.Collections.Generic;

public class AssignScripts
{
    public static void Assign()
    {
        SetupPlayer();
        SetupEnemy();
        SetupTreasure();
        SetupNavigation();
        SetupCamera();
    }

    static void SetupPlayer()
    {
        GameObject player = GameObject.Find("Player");
        GameObject playerAgent = null;

        if (player != null)
        {
            if (player.GetComponent<DebugInput>() == null)
                player.AddComponent<DebugInput>();
        }
        
        if (player != null)
        {
            if (player.transform.parent != null && player.transform.parent.name == "PlayerAgent")
            {
                playerAgent = player.transform.parent.gameObject;
            }
            else
            {
                playerAgent = new GameObject("PlayerAgent");
                playerAgent.transform.position = new Vector3(player.transform.position.x, 0, player.transform.position.z);
                playerAgent.tag = "Player"; 
                
                player.transform.SetParent(playerAgent.transform);
                player.transform.localPosition = new Vector3(0, 1, 0);
                player.tag = "Untagged"; 
                
                // Clean up components from child
                DestroyComponent<PlayerController>(player);
                DestroyComponent<Health>(player);
                DestroyComponent<Weapon>(player);
                DestroyComponent<CharacterController>(player);
                DestroyComponent<Collider>(player); // Remove collider from visual mesh
            }
        }

        if (playerAgent != null)
        {
            if (playerAgent.GetComponent<CharacterController>() == null)
            {
                CharacterController cc = playerAgent.AddComponent<CharacterController>();
                cc.center = new Vector3(0, 1, 0);
                cc.height = 2f;
                cc.radius = 0.5f;
            }

            if (playerAgent.GetComponent<PlayerController>() == null)
                playerAgent.AddComponent<PlayerController>();
            
            if (playerAgent.GetComponent<Health>() == null)
                playerAgent.AddComponent<Health>();
            
            if (playerAgent.GetComponent<DebugInput>() == null)
                playerAgent.AddComponent<DebugInput>();
            
            Weapon weapon = playerAgent.GetComponent<Weapon>();
            if (weapon == null)
                weapon = playerAgent.AddComponent<Weapon>();
            
            weapon.hitLayers = LayerMask.GetMask("Enemy", "Wall");

            Transform firePoint = playerAgent.transform.Find("FirePoint");
            if (firePoint == null)
            {
                firePoint = new GameObject("FirePoint").transform;
                firePoint.parent = playerAgent.transform;
                firePoint.localPosition = new Vector3(0, 1.5f, 0.5f);
            }
            weapon.firePoint = firePoint;

            // Set Layer
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1)
            {
                playerAgent.layer = playerLayer;
                foreach (Transform child in playerAgent.transform)
                {
                    child.gameObject.layer = playerLayer;
                }
            }
        }
    }

    static void SetupEnemy()
    {
        GameObject enemy = GameObject.Find("Enemy");
        GameObject enemyAgent = null;

        if (enemy != null)
        {
            if (enemy.transform.parent != null && enemy.transform.parent.name == "EnemyAgent")
            {
                enemyAgent = enemy.transform.parent.gameObject;
            }
            else
            {
                enemyAgent = new GameObject("EnemyAgent");
                enemyAgent.transform.position = new Vector3(enemy.transform.position.x, 0, enemy.transform.position.z);
                
                enemy.transform.SetParent(enemyAgent.transform);
                enemy.transform.localPosition = new Vector3(0, 1, 0); 
                
                // Clean up components from child
                DestroyComponent<UnityEngine.AI.NavMeshAgent>(enemy);
                DestroyComponent<EnemyAI>(enemy);
                DestroyComponent<Health>(enemy);
                DestroyComponent<Weapon>(enemy);
                // Keep collider on Enemy child for shooting detection
            }
        }

        if (enemyAgent != null)
        {
            if (enemyAgent.GetComponent<UnityEngine.AI.NavMeshAgent>() == null)
            {
                var agent = enemyAgent.AddComponent<UnityEngine.AI.NavMeshAgent>();
                agent.baseOffset = 0;
                agent.height = 2;
            }

            if (enemyAgent.GetComponent<Health>() == null)
                enemyAgent.AddComponent<Health>();

            EnemyAI ai = enemyAgent.GetComponent<EnemyAI>();
            if (ai == null)
                ai = enemyAgent.AddComponent<EnemyAI>();
            
            Weapon weapon = enemyAgent.GetComponent<Weapon>();
            if (weapon == null)
                weapon = enemyAgent.AddComponent<Weapon>();
            
            weapon.hitLayers = LayerMask.GetMask("Player", "Wall");

            Transform firePoint = enemyAgent.transform.Find("FirePoint");
            if (firePoint == null)
            {
                firePoint = new GameObject("FirePoint").transform;
                firePoint.parent = enemyAgent.transform;
                firePoint.localPosition = new Vector3(0, 1.5f, 0.5f);
            }
            weapon.firePoint = firePoint;

            // Set Layer
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer != -1)
            {
                enemyAgent.layer = enemyLayer;
                foreach (Transform child in enemyAgent.transform)
                {
                    child.gameObject.layer = enemyLayer;
                }
            }

            GameObject patrolPointsParent = GameObject.Find("PatrolPoints");
            if (patrolPointsParent != null)
            {
                ai.patrolPoints = new List<Transform>();
                foreach (Transform child in patrolPointsParent.transform)
                {
                    ai.patrolPoints.Add(child);
                }
            }
        }
    }

    static void SetupTreasure()
    {
        GameObject treasure = GameObject.Find("Treasure");
        if (treasure != null)
        {
            if (treasure.GetComponent<Treasure>() == null)
                treasure.AddComponent<Treasure>();
            
            Collider col = treasure.GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;
        }
    }

    static void SetupNavigation()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground != null)
        {
            GameObjectUtility.SetStaticEditorFlags(ground, StaticEditorFlags.NavigationStatic);
        }

        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        int wallLayer = LayerMask.NameToLayer("Wall");
        foreach (GameObject wall in walls)
        {
            if (wallLayer != -1) wall.layer = wallLayer;
            GameObjectUtility.SetStaticEditorFlags(wall, StaticEditorFlags.NavigationStatic);
        }
        
        NavMeshBuilder.BuildNavMesh();
    }

    static void SetupCamera()
    {
        GameObject camera = GameObject.Find("Main Camera");
        GameObject playerAgent = GameObject.Find("PlayerAgent");

        if (camera != null)
        {
            TopDownCamera camScript = camera.GetComponent<TopDownCamera>();
            if (camScript == null)
                camScript = camera.AddComponent<TopDownCamera>();
            
            if (playerAgent != null)
                camScript.target = playerAgent.transform;
        }
    }

    static void DestroyComponent<T>(GameObject go) where T : Component
    {
        var comp = go.GetComponent<T>();
        if (comp != null) Object.DestroyImmediate(comp);
    }
}
