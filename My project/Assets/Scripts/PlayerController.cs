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

    private CharacterController controller;
    private Weapon weapon;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float gravityValue = -9.81f;
    private float shootNoiseTimer;

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
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        HandleMovement();
        HandleStance();
        UpdateNoiseLevel();
        HandleShooting();
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
                break;
            case Stance.Crouching:
                controller.height = 1f;
                controller.center = new Vector3(0, 0.5f, 0);
                break;
            case Stance.Crawling:
                controller.height = 0.5f;
                controller.center = new Vector3(0, 0.25f, 0);
                break;
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
