using UnityEngine;
using UnityEditor;

public class RecreateEnemyChild
{
    public static void Recreate()
    {
        GameObject enemyAgent = GameObject.Find("EnemyAgent");
        if (enemyAgent == null)
        {
            Debug.LogError("EnemyAgent not found!");
            return;
        }

        // Find and destroy old child
        Transform oldChild = enemyAgent.transform.Find("Enemy");
        if (oldChild != null)
        {
            Object.DestroyImmediate(oldChild.gameObject);
        }

        // Create new child
        GameObject newChild = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        newChild.name = "Enemy";
        newChild.transform.SetParent(enemyAgent.transform);
        newChild.transform.localPosition = new Vector3(0, 1, 0);
        newChild.transform.localRotation = Quaternion.identity;
        newChild.transform.localScale = Vector3.one;

        // Set Layer
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) newChild.layer = enemyLayer;

        // Set Color
        Renderer r = newChild.GetComponent<Renderer>();
        if (r != null)
        {
            r.material = new Material(Shader.Find("HDRP/Lit"));
            r.material.color = Color.red;
        }

        // Ensure FirePoint exists on Parent (it should, but let's check)
        Transform firePoint = enemyAgent.transform.Find("FirePoint");
        if (firePoint == null)
        {
            firePoint = new GameObject("FirePoint").transform;
            firePoint.parent = enemyAgent.transform;
            firePoint.localPosition = new Vector3(0, 1.5f, 0.5f);
        }
        
        // Re-add Visor if missing (it was child of EnemyAgent, so it should be fine, but let's check)
        if (enemyAgent.transform.Find("Visor") == null)
        {
            AddEnemyVisor.Add();
        }

        Debug.Log("Enemy child recreated!");
    }
}
