using UnityEngine;
using UnityEditor;

public class FixPlayerHierarchy
{
    public static void Fix()
    {
        GameObject playerAgent = GameObject.Find("PlayerAgent");
        if (playerAgent == null)
        {
            Debug.LogError("PlayerAgent not found!");
            return;
        }

        Transform playerChild = playerAgent.transform.Find("Player");
        if (playerChild == null)
        {
            Debug.LogError("Player child not found under PlayerAgent!");
            return;
        }

        // Remove components from child
        DestroyComponent<PlayerController>(playerChild.gameObject);
        DestroyComponent<Health>(playerChild.gameObject);
        DestroyComponent<Weapon>(playerChild.gameObject);
        DestroyComponent<DebugInput>(playerChild.gameObject);
        DestroyComponent<CapsuleCollider>(playerChild.gameObject);
        DestroyComponent<CharacterController>(playerChild.gameObject); // Just in case

        // Ensure parent has components
        EnsureComponent<PlayerController>(playerAgent);
        EnsureComponent<Health>(playerAgent);
        EnsureComponent<Weapon>(playerAgent);
        EnsureComponent<DebugInput>(playerAgent);
        
        // Fix CharacterController on parent
        CharacterController cc = playerAgent.GetComponent<CharacterController>();
        if (cc == null) cc = playerAgent.AddComponent<CharacterController>();
        cc.center = new Vector3(0, 1, 0);
        cc.height = 2f;
        cc.radius = 0.5f;
        cc.skinWidth = 0.01f;
        cc.minMoveDistance = 0f;

        // Fix Weapon FirePoint on parent
        Weapon weapon = playerAgent.GetComponent<Weapon>();
        Transform firePoint = playerAgent.transform.Find("FirePoint");
        if (firePoint == null)
        {
            firePoint = new GameObject("FirePoint").transform;
            firePoint.parent = playerAgent.transform;
            firePoint.localPosition = new Vector3(0, 1.5f, 0.5f);
        }
        weapon.firePoint = firePoint;
        weapon.hitLayers = LayerMask.GetMask("Enemy", "Wall");

        Debug.Log("Player hierarchy fixed!");
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
