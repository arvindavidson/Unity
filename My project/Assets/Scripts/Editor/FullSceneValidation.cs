using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FullSceneValidation
{
    public static void Validate()
    {
        Debug.Log("=== Starting Full Scene Validation ===");
        
        FixEnemyAgent();
        FixPlayerAgent();
        AssignTracers();
        
        Debug.Log("=== Full Scene Validation Complete ===");
    }

    static void FixEnemyAgent()
    {
        GameObject enemyAgent = GameObject.Find("EnemyAgent");
        if (enemyAgent == null)
        {
            Debug.LogError("EnemyAgent not found!");
            return;
        }

        // Unpack if prefab
        if (PrefabUtility.IsPartOfAnyPrefab(enemyAgent))
        {
            PrefabUtility.UnpackPrefabInstance(enemyAgent, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        // Delete ALL children except FirePoint and Visor
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in enemyAgent.transform)
        {
            if (child.name != "FirePoint" && child.name != "Visor")
            {
                toDestroy.Add(child.gameObject);
            }
        }
        foreach (var go in toDestroy)
        {
            Object.DestroyImmediate(go);
        }

        // Create clean visual child - just mesh, no logic
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "EnemyBody";
        visual.transform.SetParent(enemyAgent.transform);
        visual.transform.localPosition = new Vector3(0, 1, 0);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
        {
            enemyAgent.layer = enemyLayer;
            visual.layer = enemyLayer;
        }

        // Set tag on parent
        try { enemyAgent.tag = "Enemy"; } catch { }

        // Color it red
        Renderer r = visual.GetComponent<Renderer>();
        if (r != null)
        {
            Material mat = new Material(Shader.Find("HDRP/Lit"));
            mat.SetColor("_BaseColor", Color.red);
            r.material = mat;
        }

        // Ensure FirePoint
        Transform firePoint = enemyAgent.transform.Find("FirePoint");
        if (firePoint == null)
        {
            firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(enemyAgent.transform);
        }
        firePoint.localPosition = new Vector3(0, 1.5f, 0.6f);

        // Ensure Visor
        Transform visor = enemyAgent.transform.Find("Visor");
        if (visor == null)
        {
            GameObject visorGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visorGO.name = "Visor";
            visorGO.transform.SetParent(enemyAgent.transform);
            Object.DestroyImmediate(visorGO.GetComponent<Collider>());
            visor = visorGO.transform;
        }
        visor.localPosition = new Vector3(0, 1.5f, 0.4f);
        visor.localRotation = Quaternion.identity;
        visor.localScale = new Vector3(0.6f, 0.2f, 0.2f);
        if (enemyLayer != -1) visor.gameObject.layer = enemyLayer;

        Renderer visorR = visor.GetComponent<Renderer>();
        if (visorR != null)
        {
            Material visorMat = new Material(Shader.Find("HDRP/Lit"));
            visorMat.SetColor("_BaseColor", Color.black);
            visorR.material = visorMat;
        }

        // Ensure parent components
        EnsureComponent<UnityEngine.AI.NavMeshAgent>(enemyAgent);
        EnsureComponent<EnemyAI>(enemyAgent);
        EnsureComponent<Health>(enemyAgent);
        EnsureComponent<Weapon>(enemyAgent);

        // Configure NavMeshAgent
        var agent = enemyAgent.GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.baseOffset = 0;
        agent.height = 2;
        agent.radius = 0.5f;
        agent.speed = 3.5f;
        agent.angularSpeed = 120;
        agent.acceleration = 8;

        // Configure Weapon
        Weapon weapon = enemyAgent.GetComponent<Weapon>();
        weapon.firePoint = firePoint;
        weapon.hitLayers = LayerMask.GetMask("Player", "Wall");

        // Configure EnemyAI patrol points
        EnemyAI ai = enemyAgent.GetComponent<EnemyAI>();
        GameObject patrolPointsParent = GameObject.Find("PatrolPoints");
        if (patrolPointsParent != null)
        {
            ai.patrolPoints = new List<Transform>();
            foreach (Transform child in patrolPointsParent.transform)
            {
                ai.patrolPoints.Add(child);
            }
        }

        // Position enemy
        enemyAgent.transform.position = new Vector3(25, 0, 25);

        Debug.Log("EnemyAgent fixed: parent has logic, children are visual only.");
    }

    static void FixPlayerAgent()
    {
        GameObject playerAgent = GameObject.Find("PlayerAgent");
        if (playerAgent == null)
        {
            Debug.LogError("PlayerAgent not found!");
            return;
        }

        // Unpack if prefab
        if (PrefabUtility.IsPartOfAnyPrefab(playerAgent))
        {
            PrefabUtility.UnpackPrefabInstance(playerAgent, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        // Delete ALL children except FirePoint
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in playerAgent.transform)
        {
            if (child.name != "FirePoint")
            {
                toDestroy.Add(child.gameObject);
            }
        }
        foreach (var go in toDestroy)
        {
            Object.DestroyImmediate(go);
        }

        // Create clean visual child
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "PlayerBody";
        visual.transform.SetParent(playerAgent.transform);
        visual.transform.localPosition = new Vector3(0, 1, 0);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer != -1)
        {
            playerAgent.layer = playerLayer;
            visual.layer = playerLayer;
        }

        playerAgent.tag = "Player";

        // Color it blue
        Renderer r = visual.GetComponent<Renderer>();
        if (r != null)
        {
            Material mat = new Material(Shader.Find("HDRP/Lit"));
            mat.SetColor("_BaseColor", new Color(0.2f, 0.4f, 0.8f));
            r.material = mat;
        }

        // Add a visor to player too for direction
        GameObject playerVisor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        playerVisor.name = "Visor";
        playerVisor.transform.SetParent(playerAgent.transform);
        playerVisor.transform.localPosition = new Vector3(0, 1.5f, 0.4f);
        playerVisor.transform.localRotation = Quaternion.identity;
        playerVisor.transform.localScale = new Vector3(0.6f, 0.15f, 0.2f);
        Object.DestroyImmediate(playerVisor.GetComponent<Collider>());
        if (playerLayer != -1) playerVisor.layer = playerLayer;

        Renderer visorR = playerVisor.GetComponent<Renderer>();
        if (visorR != null)
        {
            Material visorMat = new Material(Shader.Find("HDRP/Lit"));
            visorMat.SetColor("_BaseColor", new Color(0.1f, 0.1f, 0.3f));
            visorR.material = visorMat;
        }

        // Ensure FirePoint
        Transform firePoint = playerAgent.transform.Find("FirePoint");
        if (firePoint == null)
        {
            firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(playerAgent.transform);
        }
        firePoint.localPosition = new Vector3(0, 1.5f, 0.6f);

        // Ensure parent components
        EnsureComponent<CharacterController>(playerAgent);
        EnsureComponent<PlayerController>(playerAgent);
        EnsureComponent<Health>(playerAgent);
        EnsureComponent<Weapon>(playerAgent);

        // Configure CharacterController
        CharacterController cc = playerAgent.GetComponent<CharacterController>();
        cc.center = new Vector3(0, 1, 0);
        cc.height = 2f;
        cc.radius = 0.5f;
        cc.skinWidth = 0.01f;
        cc.minMoveDistance = 0f;

        // Configure Weapon
        Weapon weapon = playerAgent.GetComponent<Weapon>();
        weapon.firePoint = firePoint;
        weapon.hitLayers = LayerMask.GetMask("Enemy", "Wall");

        // Remove DebugInput from parent (cleanup)
        DebugInput di = playerAgent.GetComponent<DebugInput>();
        if (di != null) Object.DestroyImmediate(di);

        // Position player
        playerAgent.transform.position = new Vector3(-25, 0, -25);
        playerAgent.transform.rotation = Quaternion.Euler(0, 45, 0);

        Debug.Log("PlayerAgent fixed: parent has logic, children are visual only.");
    }

    static void AssignTracers()
    {
        string prefabPath = "Assets/Prefabs/BulletTracer.prefab";
        GameObject tracerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (tracerPrefab == null)
        {
            Debug.LogWarning("BulletTracer prefab not found, skipping tracer assignment.");
            return;
        }

        GameObject playerAgent = GameObject.Find("PlayerAgent");
        if (playerAgent != null)
        {
            Weapon w = playerAgent.GetComponent<Weapon>();
            if (w != null) w.tracerPrefab = tracerPrefab;
        }

        GameObject enemyAgent = GameObject.Find("EnemyAgent");
        if (enemyAgent != null)
        {
            Weapon w = enemyAgent.GetComponent<Weapon>();
            if (w != null) w.tracerPrefab = tracerPrefab;
        }

        Debug.Log("Tracers assigned to both agents.");
    }

    static T EnsureComponent<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null) comp = go.AddComponent<T>();
        return comp;
    }
}
