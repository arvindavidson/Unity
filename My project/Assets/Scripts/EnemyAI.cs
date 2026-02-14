using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public Transform player;
    public float viewDistance = 15f;
    public float viewAngle = 60f;
    public float hearingDistance = 15f;
    public float shootDistance = 10f;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float waitTime = 2f;

    [Header("Chase Settings")]
    public float loseSightTime = 3.5f; // Seconds without LOS before giving up chase

    [Header("Call For Help")]
    public float callForHelpRadius = 25f; // Radius to alert nearby enemies
    private float callForHelpCooldown = 0f; // Prevent spamming
    private float callForHelpInterval = 3f; // Minimum seconds between calls

    public List<Transform> patrolPoints;
    private int currentPatrolIndex;
    private bool waiting;
    private float waitTimer;

    private NavMeshAgent agent;
    private PlayerController playerController;
    private Weapon weapon;
    private LayerMask wallLayer;

    // Chase / LOS tracking
    private Vector3 lastKnownPlayerPosition;
    private float timeSinceLastSeen;
    private bool hadLOSThisFrame;

    // Investigation
    private Vector3 investigatePosition;
    private float investigateTimer;
    private float investigateDuration = 4f; // How long to look around at investigate point

    // Corpse detection
    private float corpseCheckInterval = 1f; // Check for corpses every second
    private float corpseCheckTimer;

    [Header("Stealth")]
    public float alertLevel; // 0 to 100
    public float detectionRate = 20f; // Base detection per second
    public float decayRate = 10f; // Decay per second when hidden
    public EnemyStatusUI statusUI;

    public enum State { Idle, Patrol, Chase, Attack, Investigate }
    public State currentState;

    void OnEnable()
    {
        PlayerController.OnPlayerShotFired += OnHeardGunshot;
    }

    void OnDisable()
    {
        PlayerController.OnPlayerShotFired -= OnHeardGunshot;
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerController = player.GetComponent<PlayerController>();
        }
        
        weapon = GetComponent<Weapon>();
        wallLayer = LayerMask.GetMask("Wall", "Default");
        
        if (statusUI == null) 
        {
            statusUI = GetComponentInChildren<EnemyStatusUI>();
        }
        
        // Remove any null entries first
        if (patrolPoints != null)
        {
            patrolPoints.RemoveAll(p => p == null);
        }

        // Auto-find patrol points if none assigned or all were null
        if (patrolPoints == null || patrolPoints.Count == 0)
        {
            GameObject patrolParent = GameObject.Find("PatrolPoints");
            if (patrolParent != null)
            {
                patrolPoints = new List<Transform>();
                foreach (Transform child in patrolParent.transform)
                {
                    patrolPoints.Add(child);
                }
                Debug.Log(gameObject.name + " auto-found " + patrolPoints.Count + " patrol points.");
            }
        }

        if (patrolPoints == null || patrolPoints.Count == 0)
        {
            currentState = State.Idle;
        }
        else
        {
            currentState = State.Patrol;
            agent.speed = patrolSpeed;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    void Update()
    {
        if (player == null) return;
        
        // Track LOS every frame
        hadLOSThisFrame = HasLineOfSight();
        
        // Update Alert Level Logic
        UpdateStealth();
        
        if (hadLOSThisFrame)
        {
            lastKnownPlayerPosition = player.position;
            timeSinceLastSeen = 0f;
        }
        else
        {
            timeSinceLastSeen += Time.deltaTime;
        }
        
        // Tick cooldowns
        if (callForHelpCooldown > 0f)
            callForHelpCooldown -= Time.deltaTime;

        // Periodically check for corpses while patrolling or idle
        corpseCheckTimer -= Time.deltaTime;
        if (corpseCheckTimer <= 0f)
        {
            corpseCheckTimer = corpseCheckInterval;
            if (currentState == State.Patrol || currentState == State.Idle)
            {
                CheckForCorpses();
            }
        }

        switch (currentState)
        {
            case State.Idle:
                // DetectPlayer(); // Handled by UpdateStealth now
                break;
            case State.Patrol:
                Patrol();
                // DetectPlayer(); // Handled by UpdateStealth now
                break;
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                Attack();
                break;
            case State.Investigate:
                Investigate();
                break;
        }
    }

    void UpdateStealth()
    {
        bool seeingPlayer = CanSeePlayer();
        bool hearingPlayer = CanHearPlayer();
        bool detected = seeingPlayer || hearingPlayer;

        if (currentState == State.Chase || currentState == State.Attack)
        {
            alertLevel = 100f;
        }
        else
        {
            if (seeingPlayer)
            {
                // Visual Detection - Uncapped, distance based
                float dist = Vector3.Distance(transform.position, player.position);
                float rateMultiplier = Mathf.Clamp(15f / (dist + 1f), 0.5f, 5f); // Closer = Faster
                alertLevel += detectionRate * rateMultiplier * Time.deltaTime;
            }
            else if (hearingPlayer)
            {
                // Audio Detection - Capped at 50% (Suspicious) unless it's a gunshot
                float noise = (playerController != null) ? playerController.currentNoiseLevel : 0f;
                float dist = Vector3.Distance(transform.position, player.position);
                
                if (noise > 15f) // Gunshot / Loud noise
                {
                    alertLevel += detectionRate * 10f * Time.deltaTime; // Instant alert
                }
                else
                {
                    // Footsteps: Cap at 50% (Suspicious)
                    if (alertLevel < 50f)
                    {
                        // Scale based on noise type (Run vs Walk) and Distance
                        // Sprint (10) vs Walk (5)
                        float noiseMultiplier = (noise >= 10f) ? 3.0f : 1.0f; 
                        
                        // Closer = Much louder
                        // At 1m: 10/2 = 5x boost. At 10m: 10/11 = ~0.9x.
                        float proximityMultiplier = Mathf.Clamp(10f / (dist + 1f), 0.5f, 5f);
                        
                        alertLevel += detectionRate * noiseMultiplier * proximityMultiplier * Time.deltaTime;
                    }
                }
            }
            else
            {
                // Decay
                alertLevel -= decayRate * Time.deltaTime;
            }
        }

        alertLevel = Mathf.Clamp(alertLevel, 0f, 100f);

        // State Transitions based on Alert Level
        if (currentState != State.Chase && currentState != State.Attack)
        {
            if (alertLevel >= 100f)
            {
                // Full Alert!
                currentState = State.Chase;
                CallForHelp(player.position);
            }
            else if (alertLevel >= 50f)
            {
                // Suspicious - Investigate
                if (currentState != State.Investigate)
                {
                    StartInvestigation(player.position);
                }
                else
                {
                    // Already investigating, update position if we see them
                    // But don't snap to them instantly, just look
                    investigatePosition = player.position;
                    agent.SetDestination(investigatePosition);
                    
                    // Look at player
                    Vector3 lookDir = player.position - transform.position;
                    lookDir.y = 0;
                    if (lookDir != Vector3.zero)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(lookDir);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 120f * Time.deltaTime);
                    }
                }
            }
            // If < 50, we might return to patrol? 
            // The Investigate state handles return to patrol on timer.
            // But if we were investigating and level drops below 50 (e.g. player hid), maybe strictly return?
            // Let's stick to the timer logic in Investigate() for now to avoid state flickering.
        }
        
        // Update UI
        if (statusUI != null)
        {
            statusUI.UpdateStatus(alertLevel, currentState == State.Chase || currentState == State.Attack, seeingPlayer);
        }
    }

    // ==================== GUNSHOT ALERT ====================
    
    /// <summary>
    /// Called when the player fires a weapon. Enemy will investigate if within range.
    /// </summary>
    void OnHeardGunshot(Vector3 shotOrigin, float noiseRadius)
    {
        float distance = Vector3.Distance(transform.position, shotOrigin);
        if (distance > noiseRadius) return;
        
        // If already chasing or attacking, just update last known position
        if (currentState == State.Chase || currentState == State.Attack)
        {
            lastKnownPlayerPosition = shotOrigin;
            return;
        }
        
        // Alert! Investigate the shot origin
        StartInvestigation(shotOrigin);
        CallForHelp(shotOrigin);
    }

    /// <summary>
    /// Called when this enemy is hit by a bullet (from Health.TakeDamage via EnemyAI)
    /// </summary>
    public void OnTookDamage(Vector3 damageSourcePosition)
    {
        lastKnownPlayerPosition = damageSourcePosition;
        
        if (currentState != State.Chase && currentState != State.Attack)
        {
            // We got shot! Chase the source
            currentState = State.Chase;
            agent.isStopped = false;
            agent.speed = chaseSpeed;
            agent.SetDestination(lastKnownPlayerPosition);
        }
        
        // Always call for help when taking damage
        CallForHelp(damageSourcePosition);
    }

    // ==================== STATES ====================

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Count == 0) return;
        
        if (agent.remainingDistance < 0.5f && !waiting)
        {
            waiting = true;
            waitTimer = waitTime;
        }

        if (waiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                waiting = false;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
        }
    }

    void Chase()
    {
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        
        // If we can see the player, chase directly
        if (hadLOSThisFrame)
        {
            agent.SetDestination(player.position);
            
            // Close enough and have LOS? Attack!
            if (Vector3.Distance(transform.position, player.position) < shootDistance)
            {
                currentState = State.Attack;
                return;
            }
        }
        else
        {
            // Can't see player — go to last known position
            agent.SetDestination(lastKnownPlayerPosition);
            
            // If we've lost sight for too long, switch to investigate
            if (timeSinceLastSeen >= loseSightTime)
            {
                StartInvestigation(lastKnownPlayerPosition);
                return;
            }
        }
    }

    void Attack()
    {
        agent.isStopped = true;
        
        // Look at player (only Y rotation)
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDir);
        
        if (weapon != null)
        {
            weapon.Shoot();
        }

        float dist = Vector3.Distance(transform.position, player.position);
        
        // If player moved out of shoot range or behind a wall, chase again
        if (dist > shootDistance || !hadLOSThisFrame)
        {
            currentState = State.Chase;
            agent.isStopped = false;
            timeSinceLastSeen = 0f; // Reset so they don't immediately give up
        }
    }

    void Investigate()
    {
        agent.isStopped = false;
        agent.speed = chaseSpeed * 0.75f; // Move cautiously
        
        // Always check if we can see the player while investigating
        if (CanSeePlayer() || CanHearPlayer())
        {
            currentState = State.Chase;
            return;
        }
        
        // Have we arrived at the investigation point?
        if (agent.remainingDistance < 1.5f)
        {
            // Look around
            investigateTimer += Time.deltaTime;
            
            // Slowly rotate to look around
            transform.Rotate(0, 90f * Time.deltaTime, 0);
            
            if (investigateTimer >= investigateDuration)
            {
                // Nothing found, go back to patrol
                ReturnToPatrol();
            }
        }
    }

    // ==================== CALL FOR HELP ====================

    /// <summary>
    /// Alert nearby enemies to the player's last known position.
    /// Called when this enemy becomes alerted (sees player, hears gunshot, takes damage).
    /// </summary>
    void CallForHelp(Vector3 alertPosition)
    {
        if (callForHelpCooldown > 0f) return;
        callForHelpCooldown = callForHelpInterval;

        // Find all EnemyAI in the scene
        EnemyAI[] allEnemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI other in allEnemies)
        {
            if (other == this) continue;
            if (other == null) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist > callForHelpRadius) continue;

            // Only alert enemies that aren't already engaged
            if (other.currentState == State.Chase || other.currentState == State.Attack) continue;

            other.ReceiveAlert(alertPosition);
        }
    }

    /// <summary>
    /// Receive an alert from a nearby enemy. Investigate the given position.
    /// </summary>
    public void ReceiveAlert(Vector3 alertPosition)
    {
        // If already chasing or attacking, just update last known position
        if (currentState == State.Chase || currentState == State.Attack)
        {
            lastKnownPlayerPosition = alertPosition;
            return;
        }

        lastKnownPlayerPosition = alertPosition;
        StartInvestigation(alertPosition);
    }

    // ==================== CORPSE DETECTION ====================

    /// <summary>
    /// Check for nearby undiscovered enemy corpses. If found, investigate the area.
    /// </summary>
    void CheckForCorpses()
    {
        EnemyCorpse[] corpses = FindObjectsByType<EnemyCorpse>(FindObjectsSortMode.None);
        foreach (EnemyCorpse corpse in corpses)
        {
            if (corpse == null) continue;
            if (corpse.discovered) continue;

            float dist = Vector3.Distance(transform.position, corpse.transform.position);
            if (dist > corpse.discoveryRadius) continue;

            // Check if we can actually see the corpse (not through walls)
            Vector3 eyePos = transform.position + Vector3.up * 1.5f;
            Vector3 corpsePos = corpse.transform.position + Vector3.up * 0.25f;
            Vector3 direction = corpsePos - eyePos;

            if (Physics.Raycast(eyePos, direction.normalized, out RaycastHit hit, dist, wallLayer))
            {
                continue; // Wall blocking view of corpse
            }

            // Found a corpse! Mark it as discovered so others don't re-trigger
            corpse.discovered = true;

            Debug.Log(gameObject.name + " discovered a corpse at " + corpse.transform.position);

            // Investigate the area around the corpse
            StartInvestigation(corpse.transform.position);
            CallForHelp(corpse.transform.position);
            break; // Only react to one corpse at a time
        }
    }

    // ==================== HELPERS ====================

    void StartInvestigation(Vector3 position)
    {
        currentState = State.Investigate;
        investigatePosition = position;
        investigateTimer = 0f;
        agent.isStopped = false;
        agent.speed = chaseSpeed * 0.75f;
        agent.SetDestination(investigatePosition);
    }

    void ReturnToPatrol()
    {
        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            currentState = State.Patrol;
            agent.speed = patrolSpeed;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
        else
        {
            currentState = State.Idle;
        }
    }

    void DetectPlayer()
    {
        if (CanSeePlayer() || CanHearPlayer())
        {
            lastKnownPlayerPosition = player.position;
            timeSinceLastSeen = 0f;
            currentState = State.Chase;
            CallForHelp(lastKnownPlayerPosition);
        }
    }

    bool HasLineOfSight()
    {
        if (player == null) return false;
        
        Vector3 eyePos = transform.position + Vector3.up * 1.5f;
        
        // Target the player's actual center (adjusts for crouching/crawling)
        Vector3 targetPos;
        var col = player.GetComponent<Collider>();
        if (col != null)
            targetPos = col.bounds.center;
        else
            targetPos = player.position + Vector3.up * 1.0f; // Fallback
            
        Vector3 direction = targetPos - eyePos;
        float distance = direction.magnitude;
        
        // Check for walls
        if (Physics.Raycast(eyePos, direction.normalized, out RaycastHit hit, distance, wallLayer))
        {
            return false;
        }
        
        return true;
    }

    bool CanSeePlayer()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        
        // Stealth modifiers: Harder to see crouching/crawling players at range
        float effectiveViewDistance = viewDistance;
        if (playerController != null)
        {
            if (playerController.currentStance == PlayerController.Stance.Crouching) 
                effectiveViewDistance *= 0.75f; // 25% harder to see
            else if (playerController.currentStance == PlayerController.Stance.Crawling) 
                effectiveViewDistance *= 0.5f;  // 50% harder to see
        }
        
        if (dist > effectiveViewDistance) return false;
        
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > viewAngle / 2f) return false;
        
        return HasLineOfSight();
    }

    bool CanHearPlayer()
    {
        if (playerController == null) return false;
        
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > hearingDistance) return false;
        
        if (playerController.currentNoiseLevel <= 0) return false;
        
        if (playerController.currentNoiseLevel <= distance / 2f) return false;
        
        // Sound travels through walls but is muffled — only block if thick walls
        // For gunshots (high noise), don't require LOS
        if (playerController.currentNoiseLevel >= 15f)
        {
            return true; // Gunshots can be heard through walls
        }
        
        // Footsteps require LOS
        return HasLineOfSight();
    }
}
