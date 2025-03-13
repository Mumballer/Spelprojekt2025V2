using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Camera Settings")]
    [SerializeField] public Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float lookUpLimit = 80f;
    [SerializeField] private float lookDownLimit = 80f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip walkingSound;
    [SerializeField] private AudioClip runningSound;
    private AudioSource footstepAudioSource;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Transform groundCheck;
    private LayerMask groundMask;
    private float cameraPitch = 0f;
    private bool canMove = true;

    // Variables to track player movement state for audio
    private bool isMoving = false;
    private bool isRunning = false;
    private bool wasRunning = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        groundCheck = transform.Find("GroundCheck");
        if (groundCheck == null)
        {
            groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.SetParent(transform);
            groundCheck.localPosition = new Vector3(0, -controller.height / 2f, 0);
        }
        groundMask = LayerMask.GetMask("Ground");
        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                cameraTransform = mainCamera.transform;
        }

        // Setup a single audio source for footsteps
        footstepAudioSource = gameObject.AddComponent<AudioSource>();
        footstepAudioSource.loop = true;
        footstepAudioSource.playOnAwake = false;

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
            HandleFootstepAudio();
        }
        else
        {
            // Stop audio if movement is disabled
            if (footstepAudioSource.isPlaying)
                footstepAudioSource.Stop();
        }
        HandleGravity();
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
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundMask);
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

        // Track previous state
        bool wasMoving = isMoving;
        wasRunning = isRunning;

        // Update current state - removed isGrounded check
        isMoving = direction.magnitude >= 0.1f;
        isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);

        if (direction.magnitude >= 0.1f)
        {
            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            controller.Move(direction.normalized * currentSpeed * Time.deltaTime);
        }
    }

    private void HandleFootstepAudio()
    {
        // Only check if we're moving now, not if we're grounded
        if (!isMoving)
        {
            if (footstepAudioSource.isPlaying)
                footstepAudioSource.Stop();
            return;
        }

        // Change audio clip when running state changes
        if (isRunning != wasRunning || !footstepAudioSource.isPlaying)
        {
            // Get correct sound clip based on movement type
            AudioClip clipToPlay = isRunning ? runningSound : walkingSound;

            // Only change if necessary
            if (footstepAudioSource.clip != clipToPlay || !footstepAudioSource.isPlaying)
            {
                footstepAudioSource.clip = clipToPlay;

                // Start playing if not already playing
                if (!footstepAudioSource.isPlaying && clipToPlay != null)
                {
                    footstepAudioSource.Play();
                }
            }
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

    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
        if (!canMove)
        {
            velocity.x = 0;
            velocity.z = 0;

            // Stop audio if movement is disabled
            if (footstepAudioSource.isPlaying)
                footstepAudioSource.Stop();
        }
    }
}