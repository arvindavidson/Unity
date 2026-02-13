using UnityEngine;
using UnityEditor;

public class AddEnemyVisor
{
    public static void Add()
    {
        GameObject enemyAgent = GameObject.Find("EnemyAgent");
        if (enemyAgent == null)
        {
            Debug.Log("EnemyAgent not found.");
            return;
        }

        // Check if Visor already exists
        Transform existingVisor = enemyAgent.transform.Find("Visor");
        if (existingVisor != null)
        {
            Object.DestroyImmediate(existingVisor.gameObject);
        }

        // Create Visor
        GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visor.name = "Visor";
        visor.transform.SetParent(enemyAgent.transform);
        
        // Position: Front of the capsule (assuming capsule radius 0.5)
        // Height: Eye level (approx 1.5)
        visor.transform.localPosition = new Vector3(0, 1.5f, 0.4f);
        visor.transform.localRotation = Quaternion.identity;
        visor.transform.localScale = new Vector3(0.6f, 0.2f, 0.2f);
        
        // Color it black
        Renderer r = visor.GetComponent<Renderer>();
        if (r != null)
        {
            r.material = new Material(Shader.Find("HDRP/Lit"));
            r.material.color = Color.black;
        }
        
        // Remove collider so it doesn't interfere
        Object.DestroyImmediate(visor.GetComponent<Collider>());

        Debug.Log("Enemy Visor Added!");
    }
}
