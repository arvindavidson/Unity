using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FinalEnemyFix
{
    public static void Fix()
    {
        GameObject enemyAgent = GameObject.Find("EnemyAgent");
        if (enemyAgent == null) { Debug.LogError("EnemyAgent not found!"); return; }

        // Step 1: Delete the problematic "Enemy" child directly
        Transform enemyChild = enemyAgent.transform.Find("Enemy");
        if (enemyChild != null)
        {
            Debug.Log("Found Enemy child, destroying it...");
            Object.DestroyImmediate(enemyChild.gameObject);
        }

        // Step 2: Delete "EnemyVisual" if it exists (leftover from previous attempts)
        Transform enemyVisual = enemyAgent.transform.Find("EnemyVisual");
        if (enemyVisual != null)
        {
            Object.DestroyImmediate(enemyVisual.gameObject);
        }

        // Step 3: Create fresh visual body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "EnemyBody";
        body.transform.SetParent(enemyAgent.transform);
        body.transform.localPosition = new Vector3(0, 1, 0);
        body.transform.localRotation = Quaternion.identity;
        body.transform.localScale = Vector3.one;

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) body.layer = enemyLayer;

        Renderer r = body.GetComponent<Renderer>();
        if (r != null)
        {
            Material mat = new Material(Shader.Find("HDRP/Lit"));
            mat.SetColor("_BaseColor", Color.red);
            r.material = mat;
        }

        // Step 4: Assign tracer prefab
        string prefabPath = "Assets/Prefabs/BulletTracer.prefab";
        GameObject tracerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        Weapon enemyWeapon = enemyAgent.GetComponent<Weapon>();
        if (enemyWeapon != null && tracerPrefab != null)
        {
            enemyWeapon.tracerPrefab = tracerPrefab;
            Debug.Log("Tracer assigned to enemy weapon.");
        }

        // Step 5: Set enemy tag
        try { enemyAgent.tag = "Enemy"; } catch { Debug.LogWarning("Could not set Enemy tag"); }

        // Step 6: Position enemy at start
        enemyAgent.transform.position = new Vector3(25, 0, 25);

        Debug.Log("FinalEnemyFix complete!");
    }
}
