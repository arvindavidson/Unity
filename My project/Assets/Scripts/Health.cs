using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        
        if (gameObject.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.UpdateHealth(currentHealth);
        }
        
        // Alert enemy AI when taking damage
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                ai.OnTookDamage(playerObj.transform.position);
            }
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " died!");
        
        if (gameObject.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
            Destroy(gameObject);
            return;
        }
        
        // Enemy death — leave a corpse instead of destroying
        if (gameObject.CompareTag("Enemy"))
        {
            CreateCorpse();
            return;
        }
        
        Destroy(gameObject);
    }

    void CreateCorpse()
    {
        // Disable all AI/logic components but keep the visual
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.enabled = false;
        }

        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }

        Weapon weapon = GetComponent<Weapon>();
        if (weapon != null)
        {
            weapon.enabled = false;
        }

        // Disable this Health component so it can't take more damage
        this.enabled = false;

        // Tip the entire parent object on its side so the body falls over naturally
        // The EnemyBody child is at local (0,1,0), so rotating the parent tips it
        transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
        // Lower the position so it rests on the ground
        transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);

        // Hide the visor
        Transform visor = transform.Find("Visor");
        if (visor != null)
        {
            visor.gameObject.SetActive(false);
        }

        // Hide the FirePoint
        Transform firePoint = transform.Find("FirePoint");
        if (firePoint != null)
        {
            firePoint.gameObject.SetActive(false);
        }

        // Disable colliders on the body so it doesn't block movement
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // Add corpse marker component
        EnemyCorpse corpse = gameObject.AddComponent<EnemyCorpse>();
        corpse.discovered = false;
        corpse.discoveryRadius = 8f;

        // Change tag so it's no longer targeted as a living enemy
        gameObject.tag = "Untagged";

        Debug.Log(gameObject.name + " is now a corpse.");
    }
}
