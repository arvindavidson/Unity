using UnityEngine;
using UnityEditor;

public class ForceDeleteEnemyChild
{
    public static void Delete()
    {
        GameObject enemyAgent = GameObject.Find("EnemyAgent");
        if (enemyAgent == null) { Debug.LogError("EnemyAgent not found!"); return; }

        // Check if it's a prefab
        bool isPrefab = PrefabUtility.IsPartOfAnyPrefab(enemyAgent);
        Debug.Log($"EnemyAgent is prefab: {isPrefab}");

        if (isPrefab)
        {
            Debug.Log("Unpacking prefab completely...");
            PrefabUtility.UnpackPrefabInstance(
                PrefabUtility.GetNearestPrefabInstanceRoot(enemyAgent),
                PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction
            );
        }

        // Now try to destroy Enemy child
        Transform enemyChild = enemyAgent.transform.Find("Enemy");
        if (enemyChild != null)
        {
            Debug.Log($"Found Enemy child. isPrefab after unpack: {PrefabUtility.IsPartOfAnyPrefab(enemyChild.gameObject)}");
            Object.DestroyImmediate(enemyChild.gameObject);
            Debug.Log("Destroyed Enemy child.");
        }
        else
        {
            Debug.Log("Enemy child not found (already deleted).");
        }

        // Also delete EnemyBody if it exists (we'll recreate)
        Transform enemyBody = enemyAgent.transform.Find("EnemyBody");
        if (enemyBody != null)
        {
            Object.DestroyImmediate(enemyBody.gameObject);
        }

        // Verify
        Transform check = enemyAgent.transform.Find("Enemy");
        Debug.Log($"Enemy child after delete: {(check != null ? "STILL EXISTS" : "GONE")}");

        // Create fresh body
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

        // Assign tracer
        string prefabPath = "Assets/Prefabs/BulletTracer.prefab";
        GameObject tracerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Weapon w = enemyAgent.GetComponent<Weapon>();
        if (w != null && tracerPrefab != null) w.tracerPrefab = tracerPrefab;

        try { enemyAgent.tag = "Enemy"; } catch { }
        enemyAgent.transform.position = new Vector3(25, 0, 25);

        // Save scene
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("ForceDeleteEnemyChild complete!");
    }
}
