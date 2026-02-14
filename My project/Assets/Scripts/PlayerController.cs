using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Input Settings")]
    public float mouseSensitivity = 1.0f;
    public float controllerSensitivity = 1.0f;

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

    private InputSystem_Actions inputActions;

    // Visual targets for stance transitions
    private Vector3 targetBodyPos;
    private Vector3 targetBodyScale;
    private Vector3 targetVisorPos;
    private Vector3 targetFirePointPos;

    private enum InputDevice { Mouse, Gamepad }
    private InputDevice currentInputDevice = InputDevice.Mouse;
    public bool IsGamepadActive => currentInputDevice == InputDevice.Gamepad;
    
    // Virtual Cursor State
    private Vector2 virtualMousePosition;
    public Vector2 CrosshairScreenPosition { get; private set; }

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

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

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

        // Init Virtual Mouse
        virtualMousePosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    void Update()
    {
        // Don't process player input if game is paused
        if (Time.timeScale == 0f) return;

        HandleMovement();
        HandleStance();
        UpdateNoiseLevel();
        HandleShooting();
        UpdateVisuals();
    }

    void HandleShooting()
    {
        if (inputActions.Player.Attack.WasPressedThisFrame())
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

        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();
        float h = input.x;
        float v = input.y;

        Vector3 move = new Vector3(h, 0, v);
        // Normalize only if length > 1 (allows partial stick movement for sneak)
        if (move.magnitude > 1f) move.Normalize();
        
        float speed = walkSpeed;
        if (currentStance == Stance.Standing)
        {
            if (inputActions.Player.Sprint.IsPressed())
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
        
        // Check for Input Device Change
        // Use direct device reads to prevent "fighting" and ambiguity
        
        // Check Gamepad first (Priority)
        bool gamepadActive = false;
        if (Gamepad.current != null)
        {
            Vector2 stickInput = Gamepad.current.rightStick.ReadValue();
            if (stickInput.sqrMagnitude > 0.04f) // Slight deadzone (0.2^2 = 0.04)
            {
                currentInputDevice = InputDevice.Gamepad;
                gamepadActive = true;
            }
        }
        
        // Check Mouse only if Gamepad isn't actively looking
        if (!gamepadActive && Mouse.current != null)
        {
            if (Mouse.current.delta.ReadValue().sqrMagnitude > 2.0f) // Higher threshold to ignore jitter
            {
                currentInputDevice = InputDevice.Mouse;
            }
        }

        // Handle Rotation
        
        // Lock cursor when game starts/is active
        if (Cursor.lockState != CursorLockMode.Locked) 
            Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Update Virtual Mouse Position
        if (currentInputDevice == InputDevice.Mouse)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            // Apply sensitivity to the DELTA, not the rotation speed.
            // 1.0 is default, range 0.1 - 3.0. 
            // Multiplier needs to be tuned. 
            virtualMousePosition += mouseDelta * mouseSensitivity; 
            
            // Clamp to screen bounds
            virtualMousePosition.x = Mathf.Clamp(virtualMousePosition.x, 0, Screen.width);
            virtualMousePosition.y = Mathf.Clamp(virtualMousePosition.y, 0, Screen.height);
        }

        // Handle Rotation
        Vector3 targetDirection = transform.forward;
        float turnSpeed = 20f; // Fast turn speed for responsiveness

        if (currentInputDevice == InputDevice.Gamepad)
        {
            Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();
            if (lookInput.sqrMagnitude > 0.01f)
            {
                // Gamepad: Character faces stick direction
                targetDirection = new Vector3(lookInput.x, 0, lookInput.y).normalized;
                
                // Controller Sensitivity controls turn speed
                turnSpeed = controllerSensitivity * 30f; 
            }
            
            // ALWAYS update crosshair for gamepad to follow player
            // even if stick is released (keep aiming forward)
            if (Camera.main != null)
            {
                // Use FirePoint position if available to align crosshair with bullet path height
                Vector3 startPos = firePoint != null ? firePoint.position : (transform.position + Vector3.up * 1.5f);
                Vector3 aimPoint = startPos + transform.forward * 5f; // 5m ahead at gun height
                CrosshairScreenPosition = Camera.main.WorldToScreenPoint(aimPoint);
            }
        }
        else // Mouse
        {
            if (Camera.main != null)
            {
                // Raycast from VIRTUAL mouse position
                Ray ray = Camera.main.ScreenPointToRay(virtualMousePosition);
                
                // Define plane at Gun Height (approx 1.5f or actual component height)
                // This ensures we aim at the "slice" of the world where bullets travel, minimizing parallax.
                float aimHeight = firePoint != null ? firePoint.position.y : 1.5f;
                Plane aimPlane = new Plane(Vector3.up, new Vector3(0, aimHeight, 0));
                
                float rayDistance;

                if (aimPlane.Raycast(ray, out rayDistance))
                {
                    Vector3 lookPoint = ray.GetPoint(rayDistance);
                    // lookPoint is now at aimHeight.
                    // transform.position is at feet (approx 0).
                    // We want a flat direction vector on the XZ plane.
                    Vector3 flatLookPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
                    targetDirection = (flatLookPoint - transform.position).normalized;
                    
                    // Mouse Mode: Turn speed is high to track cursor
                    turnSpeed = 40f; 
                }
                
                // Crosshair is exactly where the virtual cursor is
                CrosshairScreenPosition = virtualMousePosition; 
            }
        }

        // Apply Rotation
        if (targetDirection != Vector3.zero)
        {
            Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDirection, turnSpeed * Time.deltaTime, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDir);
        }
        
        // Fallback: If moving but not looking with stick/mouse (rare), look forward? 
        // Existing logic did: if (move != Vector3.zero) gameObject.transform.forward = move;
        // But for a twin stick shooter, you often move and look in different directions.
        // We'll keep independent look.
        
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
        if (inputActions.Player.Crouch.WasPressedThisFrame())
        {
            if (currentStance == Stance.Crouching)
                SetStance(Stance.Standing);
            else
                SetStance(Stance.Crouching);
        }
        else if (inputActions.Player.Crawl.WasPressedThisFrame())
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
                if (inputActions.Player.Sprint.IsPressed())
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
