using UnityEngine;
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck; // Assign this in inspector
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float lookUpLimit = 80f;
    [SerializeField] private float lookDownLimit = 80f;

    [Header("Audio Settings")]
    [SerializeField] public AudioSource walkAudioSource;
    [SerializeField] public AudioSource runAudioSource;

    [Header("Item Interaction")]
    [System.NonSerialized]
    [SerializeField] private LayerMask interactableLayers;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float cameraPitch = 0f;
    private bool canMove = true;
    private bool isMoving = false;
    private bool isRunning = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Ensure we have a ground check
        if (groundCheck == null)
        {
            Debug.LogWarning("GroundCheck not assigned! Creating a temporary one, but please assign one in the inspector.");
            groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.SetParent(transform);
            groundCheck.localPosition = new Vector3(0, -controller.height / 2f, 0);
        }

        // Ensure ground mask is set
        if (groundMask.value == 0)
        {
            Debug.LogWarning("Ground mask not set! Using default layer.");
            groundMask = 1 << LayerMask.NameToLayer("Default");
        }

        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                cameraTransform = mainCamera.transform;
        }

        // Initialize audio sources if not set
        if (walkAudioSource == null)
        {
            GameObject walkAudio = new GameObject("WalkAudio");
            walkAudio.transform.SetParent(transform);
            walkAudioSource = walkAudio.AddComponent<AudioSource>();
            walkAudioSource.playOnAwake = false;
            walkAudioSource.loop = true;
        }

        if (runAudioSource == null)
        {
            GameObject runAudio = new GameObject("RunAudio");
            runAudio.transform.SetParent(transform);
            runAudioSource = runAudio.AddComponent<AudioSource>();
            runAudioSource.playOnAwake = false;
            runAudioSource.loop = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (canMove)
        {
            HandleCameraRotation();
            CheckGrounded();
            HandleMovement();
        }
        HandleGravity();
        HandleAudio();
    }

    private void HandleCameraRotation()
    {
        if (cameraTransform == null) return;
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up, mouseX);
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -lookUpLimit, lookDownLimit);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
    }

    private void CheckGrounded()
    {
        // Simple sphere check at the ground check position
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        // Reset vertical velocity when grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = transform.right * horizontal + transform.forward * vertical;

        // Check if player is moving
        isMoving = direction.magnitude > 0.1f;
        isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);

        if (isMoving)
        {
            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            controller.Move(direction.normalized * currentSpeed * Time.deltaTime);
        }
    }

    private void HandleGravity()
    {
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleAudio()
    {
        // Only play sound if on the ground and moving
        if (!isGrounded || !isMoving)
        {
            walkAudioSource.Stop();
            runAudioSource.Stop();
            return;
        }

        if (isRunning)
        {
            // If running, play run sound and stop walk sound
            if (!runAudioSource.isPlaying)
            {
                walkAudioSource.Stop();
                runAudioSource.Play();
            }
        }
        else
        {
            // If walking, play walk sound and stop run sound
            if (!walkAudioSource.isPlaying)
            {
                runAudioSource.Stop();
                walkAudioSource.Play();
            }
        }
    }

    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
        if (!canMove)
        {
            velocity.x = 0;
            velocity.z = 0;
            // Stop audio when movement is disabled
            walkAudioSource.Stop();
            runAudioSource.Stop();
        }
    }

    public Transform GetCameraTransform()
    {
        return cameraTransform;
    }

    // Visualize the ground check in the Scene view
    private void OnDrawGizmos()
    {
        if (groundCheck == null) return;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}