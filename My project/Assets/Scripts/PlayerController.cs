using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2.5f;
    public float crawlSpeed = 1.5f;

    public float noiseWalk = 5f;
    public float noiseSprint = 10f;
    public float noiseCrouch = 0f;
    public float noiseCrawl = 0f;
    public float noiseShoot = 20f; // Gunshot noise radius
    public float shootNoiseDuration = 0.5f; // How long the gunshot noise lingers

    public float currentNoiseLevel;

    [Header("Visual References")]
    public Transform playerBody;
    public Transform visor;
    public Transform firePoint;
    public float stanceTransitionSpeed = 8f;

    private CharacterController controller;
    private Weapon weapon;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float gravityValue = -9.81f;
    private float shootNoiseTimer;

    // Visual targets for stance transitions
    private Vector3 targetBodyPos;
    private Vector3 targetBodyScale;
    private Vector3 targetVisorPos;
    private Vector3 targetFirePointPos;

    // Standing defaults
    private readonly Vector3 standBodyPos = new Vector3(0, 1f, 0);
    private readonly Vector3 standBodyScale = new Vector3(1f, 1f, 1f);
    private readonly Vector3 standVisorPos = new Vector3(0, 1.5f, 0.4f);
    private readonly Vector3 standFirePointPos = new Vector3(0, 1.5f, 0.6f);

    // Crouching targets
    private readonly Vector3 crouchBodyPos = new Vector3(0, 0.5f, 0);
    private readonly Vector3 crouchBodyScale = new Vector3(1f, 0.5f, 1f);
    private readonly Vector3 crouchVisorPos = new Vector3(0, 0.85f, 0.4f);
    private readonly Vector3 crouchFirePointPos = new Vector3(0, 0.85f, 0.6f);

    // Crawling targets
    private readonly Vector3 crawlBodyPos = new Vector3(0, 0.25f, 0);
    private readonly Vector3 crawlBodyScale = new Vector3(1f, 0.25f, 1.4f);
    private readonly Vector3 crawlVisorPos = new Vector3(0, 0.4f, 0.5f);
    private readonly Vector3 crawlFirePointPos = new Vector3(0, 0.4f, 0.7f);

    // Event for when player fires — enemies can subscribe
    public delegate void PlayerShotFired(Vector3 shotOrigin, float noiseRadius);
    public static event PlayerShotFired OnPlayerShotFired;

    public enum Stance { Standing, Crouching, Crawling }
    public Stance currentStance = Stance.Standing;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.center = new Vector3(0, 1, 0);
            controller.skinWidth = 0.01f;
            controller.minMoveDistance = 0f;
        }
        else
        {
            controller.skinWidth = 0.01f;
            controller.minMoveDistance = 0f;
        }
        
        weapon = GetComponent<Weapon>();
        
        // Auto-find visual references if not assigned
        if (playerBody == null) playerBody = transform.Find("PlayerBody");
        if (visor == null) visor = transform.Find("Visor");
        if (firePoint == null) firePoint = transform.Find("FirePoint");
        
        // Initialize visual targets to standing
        targetBodyPos = standBodyPos;
        targetBodyScale = standBodyScale;
        targetVisorPos = standVisorPos;
        targetFirePointPos = standFirePointPos;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        HandleMovement();
        HandleStance();
        UpdateNoiseLevel();
        HandleShooting();
        UpdateVisuals();
    }

    void HandleShooting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (weapon != null)
            {
                weapon.Shoot();
                
                // Generate gunshot noise
                shootNoiseTimer = shootNoiseDuration;
                
                // Notify all enemies about the gunshot
                OnPlayerShotFired?.Invoke(transform.position, noiseShoot);
            }
        }
    }

    void HandleMovement()
    {
        if (controller == null) 
        {
            Debug.LogError("CharacterController is null on " + gameObject.name);
            return;
        }

        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        
        if (transform.position.y < 0.1f && groundedPlayer)
        {
            controller.Move(Vector3.up * 0.1f * Time.deltaTime);
        }
        
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0, v);
        if (move.magnitude > 1f) move.Normalize();
        
        float speed = walkSpeed;
        if (currentStance == Stance.Standing)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed = sprintSpeed;
            }
        }
        else if (currentStance == Stance.Crouching)
        {
            speed = crouchSpeed;
        }
        else if (currentStance == Stance.Crawling)
        {
            speed = crawlSpeed;
        }

        Vector3 movement = move * speed;
        
        // Handle Rotation (Look at Mouse)
        if (Camera.main != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayDistance;

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 point = ray.GetPoint(rayDistance);
                Vector3 lookPoint = new Vector3(point.x, transform.position.y, point.z);
                transform.LookAt(lookPoint);
            }
            else if (move != Vector3.zero)
            {
                gameObject.transform.forward = move;
            }
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        Vector3 finalMovement = movement + Vector3.up * playerVelocity.y;
        controller.Move(finalMovement * Time.deltaTime);
        
        if (transform.position.y < -5)
        {
            controller.enabled = false;
            transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
            controller.enabled = true;
            playerVelocity.y = 0;
        }
    }

    void HandleStance()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (currentStance == Stance.Crouching)
                SetStance(Stance.Standing);
            else
                SetStance(Stance.Crouching);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            if (currentStance == Stance.Crawling)
                SetStance(Stance.Standing);
            else
                SetStance(Stance.Crawling);
        }
    }

    void SetStance(Stance newStance)
    {
        currentStance = newStance;
        switch (currentStance)
        {
            case Stance.Standing:
                controller.height = 2f;
                controller.center = new Vector3(0, 1, 0);
                targetBodyPos = standBodyPos;
                targetBodyScale = standBodyScale;
                targetVisorPos = standVisorPos;
                targetFirePointPos = standFirePointPos;
                break;
            case Stance.Crouching:
                controller.height = 1f;
                controller.center = new Vector3(0, 0.5f, 0);
                targetBodyPos = crouchBodyPos;
                targetBodyScale = crouchBodyScale;
                targetVisorPos = crouchVisorPos;
                targetFirePointPos = crouchFirePointPos;
                break;
            case Stance.Crawling:
                controller.height = 0.5f;
                controller.center = new Vector3(0, 0.25f, 0);
                targetBodyPos = crawlBodyPos;
                targetBodyScale = crawlBodyScale;
                targetVisorPos = crawlVisorPos;
                targetFirePointPos = crawlFirePointPos;
                break;
        }
    }

    void UpdateVisuals()
    {
        float t = stanceTransitionSpeed * Time.deltaTime;

        if (playerBody != null)
        {
            playerBody.localPosition = Vector3.Lerp(playerBody.localPosition, targetBodyPos, t);
            playerBody.localScale = Vector3.Lerp(playerBody.localScale, targetBodyScale, t);
        }

        if (visor != null)
        {
            visor.localPosition = Vector3.Lerp(visor.localPosition, targetVisorPos, t);
        }

        if (firePoint != null)
        {
            firePoint.localPosition = Vector3.Lerp(firePoint.localPosition, targetFirePointPos, t);
        }
    }

    void UpdateNoiseLevel()
    {
        // Decay shoot noise timer
        if (shootNoiseTimer > 0)
        {
            shootNoiseTimer -= Time.deltaTime;
            currentNoiseLevel = noiseShoot;
            return; // Gunshot noise overrides movement noise
        }
        
        if (controller.velocity.magnitude > 0.1f)
        {
            if (currentStance == Stance.Standing)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                    currentNoiseLevel = noiseSprint;
                else
                    currentNoiseLevel = noiseWalk;
            }
            else
            {
                currentNoiseLevel = 0f;
            }
        }
        else
        {
            currentNoiseLevel = 0f;
        }
    }
}
