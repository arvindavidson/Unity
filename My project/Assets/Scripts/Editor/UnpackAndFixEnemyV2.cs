using UnityEngine;
using UnityEditor;

public class UnpackAndFixEnemyV2
{
    public static void Fix()
    {
        GameObject enemyAgent = GameObject.Find("EnemyAgent");
        if (enemyAgent == null)
        {
            Debug.LogError("EnemyAgent not found!");
            return;
        }

        if (PrefabUtility.IsPartOfAnyPrefab(enemyAgent))
        {
            PrefabUtility.UnpackPrefabInstance(enemyAgent, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        Transform oldChild = enemyAgent.transform.Find("Enemy");
        if (oldChild != null)
        {
            Object.DestroyImmediate(oldChild.gameObject);
        }

        GameObject newChild = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        newChild.name = "EnemyVisual"; // New Name
        newChild.transform.SetParent(enemyAgent.transform);
        newChild.transform.localPosition = new Vector3(0, 1, 0);
        newChild.transform.localRotation = Quaternion.identity;
        newChild.transform.localScale = Vector3.one;

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) newChild.layer = enemyLayer;

        Renderer r = newChild.GetComponent<Renderer>();
        if (r != null)
        {
            r.material = new Material(Shader.Find("HDRP/Lit"));
            r.material.color = Color.red;
        }

        // Ensure FirePoint exists on Parent
        Transform firePoint = enemyAgent.transform.Find("FirePoint");
        if (firePoint == null)
        {
            firePoint = new GameObject("FirePoint").transform;
            firePoint.parent = enemyAgent.transform;
            firePoint.localPosition = new Vector3(0, 1.5f, 0.5f);
        }
        
        if (enemyAgent.transform.Find("Visor") == null)
        {
            AddEnemyVisor.Add();
        }

        Debug.Log("Enemy hierarchy fixed V2!");
    }
}
