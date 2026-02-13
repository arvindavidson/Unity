using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 0.5f;
    public float nextFireTime = 0f;

    public Transform firePoint;
    public LayerMask hitLayers;
    public GameObject tracerPrefab;

    void Start()
    {
        // Auto-configure hitLayers based on who owns this weapon
        if (hitLayers == 0)
        {
            if (gameObject.CompareTag("Player"))
            {
                // Player shoots enemies and walls
                hitLayers = LayerMask.GetMask("Enemy", "Wall", "Default");
            }
            else
            {
                // Enemy shoots player and walls
                hitLayers = LayerMask.GetMask("Player", "Wall", "Default");
            }
        }
    }

    public void Shoot()
    {
        if (Time.time < nextFireTime) return;
        
        nextFireTime = Time.time + fireRate;
        
        if (firePoint == null)
        {
            Debug.LogWarning("Weapon on " + gameObject.name + " has no firePoint!");
            return;
        }
        
        RaycastHit hit;
        Vector3 targetPoint;
        Vector3 shootDirection = firePoint.forward;
        
        if (Physics.Raycast(firePoint.position, shootDirection, out hit, range, hitLayers))
        {
            targetPoint = hit.point;
            
            Health targetHealth = hit.collider.GetComponentInParent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }
        }
        else
        {
            targetPoint = firePoint.position + shootDirection * range;
        }

        if (tracerPrefab != null)
        {
            GameObject tracerObj = Instantiate(tracerPrefab, Vector3.zero, Quaternion.identity);
            BulletTracer tracer = tracerObj.GetComponent<BulletTracer>();
            if (tracer != null)
            {
                tracer.Init(firePoint.position, targetPoint);
            }
        }
    }
}
