using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.4f;

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float lookUpLimit = 80f;
    [SerializeField] private float lookDownLimit = 80f;

    [Header("Item Interaction")]
    [System.NonSerialized]
    [SerializeField] private LayerMask interactableLayers;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Transform groundCheck;
    private LayerMask groundMask;
    private float cameraPitch = 0f;
    private bool canMove = true;

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

        if (direction.magnitude >= 0.1f)
        {
            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
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

    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;

        if (!canMove)
        {
            velocity.x = 0;
            velocity.z = 0;
        }
    }

    public Transform GetCameraTransform()
    {
        return cameraTransform;
    }
}