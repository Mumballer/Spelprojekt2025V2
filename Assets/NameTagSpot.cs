using UnityEngine;

public class NameTagSpot : MonoBehaviour
{
    [Header("Spot Settings")]
    [SerializeField] private string expectedGuest;
    [SerializeField] public Transform snapPoint;
    [SerializeField] private float snapDistance = 0.5f; // Increased for easier placement

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 3.0f; // Increased range
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private bool showPromptWhenInRange = true;

    [Header("Guest Info Display")]
    [SerializeField] private GameObject guestInfoLabel; // Pre-positioned text object

    [Header("Visualization")]
    [SerializeField] private bool showDebugVisuals = true;
    [SerializeField] private Color availableColor = Color.green;
    [SerializeField] private Color occupiedColor = Color.red;
    [SerializeField] private float gizmoSphereSize = 0.1f;

    private bool hasNameTag = false;
    private string currentGuestName = "";
    private TableController tableController;
    private Transform playerTransform;
    private bool playerInRange = false;

    // Properties
    public bool HasNameTag => hasNameTag;
    public bool IsCorrectNameTag => hasNameTag && currentGuestName == expectedGuest;
    public string ExpectedGuest => expectedGuest;

    void Start()
    {
        // Make sure the guest info label is hidden at start
        if (guestInfoLabel != null)
        {
            guestInfoLabel.SetActive(false);
        }

        // If we don't have a snap point, create one at our position
        if (snapPoint == null)
        {
            GameObject snapObj = new GameObject("SnapPoint");
            snapPoint = snapObj.transform;
            snapPoint.SetParent(transform);
            snapPoint.localPosition = Vector3.zero;
            snapPoint.localRotation = Quaternion.identity;
        }

        // Hide interaction prompt initially
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // Find player reference
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        // Check for player in range
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            bool newInRange = distanceToPlayer <= interactionDistance;

            // Only update if changed
            if (newInRange != playerInRange)
            {
                playerInRange = newInRange;

                // Show/hide prompt
                if (interactionPrompt != null && showPromptWhenInRange)
                {
                    interactionPrompt.SetActive(playerInRange);
                }
            }

            // Handle interaction when in range
            if (playerInRange && Input.GetKeyDown(interactKey))
            {
                Interact();
            }
        }
    }

    // Called when player interacts with this spot
    public void Interact()
    {
        // If we already have a nametag, just make it visible
        if (hasNameTag)
        {
            Debug.Log($"This spot already has a nametag: {currentGuestName}. Making it visible.");

            // Find and show the table nametag
            NameTag[] nametagsAtSpot = GetComponentsInChildren<NameTag>(true);
            foreach (var nametag in nametagsAtSpot)
            {
                if (!nametag.IsKitchenNametag) // If it's a table nametag
                {
                    nametag.SetVisibility(true);
                }
            }

            // Notify tableController if we have one
            if (tableController != null)
            {
                tableController.OnNameTagPlaced(this, currentGuestName);
                Debug.Log($"Fired OnNametagPlaced event for {currentGuestName}");
            }
        }
        else
        {
            // Check if player is holding a nametag
            // You'd need to add code to detect what nametag the player is holding
            Debug.Log("No nametag at this spot and auto-placement not implemented.");
        }
    }

    // Method to show/hide the guest info label
    public void ShowGuestInfoLabel(bool show)
    {
        if (guestInfoLabel != null)
        {
            guestInfoLabel.SetActive(show);
        }
    }

    // Initialize this spot with necessary information
    public void Initialize(string guestName, TableController controller)
    {
        expectedGuest = guestName;
        tableController = controller;
    }

    private void OnTriggerEnter(Collider other)
    {
        NameTag nameTag = other.GetComponent<NameTag>();
        if (nameTag != null && !hasNameTag && nameTag.IsKitchenNametag)
        {
            // Check if we should auto-snap
            if (Vector3.Distance(nameTag.transform.position, transform.position) <= snapDistance)
            {
                // Snap nametag to position if we have a snap point
                if (snapPoint != null)
                {
                    nameTag.transform.position = snapPoint.position;
                    nameTag.transform.rotation = snapPoint.rotation;
                }

                hasNameTag = true;
                currentGuestName = nameTag.GuestName; // Using GuestName property

                // Mark the nametag as placed
                nameTag.isPlaced = true;

                // Notify table controller
                if (tableController != null)
                {
                    tableController.OnNameTagPlaced(this, currentGuestName);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        NameTag nameTag = other.GetComponent<NameTag>();
        if (nameTag != null && hasNameTag && nameTag.IsKitchenNametag)
        {
            hasNameTag = false;
            string previousGuest = currentGuestName;
            currentGuestName = "";

            // Mark the nametag as not placed
            nameTag.isPlaced = false;

            // Notify table controller
            if (tableController != null)
            {
                tableController.OnNameTagRemoved(this, previousGuest);
            }
        }
    }

    // Check if a position is within range of this spot
    public bool IsNameTagInRange(Vector3 position)
    {
        return Vector3.Distance(transform.position, position) <= snapDistance;
    }

    // For debugging in the editor
    private void OnDrawGizmos()
    {
        if (!showDebugVisuals) return;

        // Draw a sphere at the spot position
        Gizmos.color = hasNameTag ? occupiedColor : availableColor;
        Gizmos.DrawSphere(transform.position, gizmoSphereSize);

        // Draw interaction range
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        // Draw snap range
        Gizmos.color = new Color(0.2f, 0.2f, 0.8f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, snapDistance);

        // Draw a line to the snap point if available
        if (snapPoint != null)
        {
            Gizmos.DrawLine(transform.position, snapPoint.position);
            Gizmos.DrawWireSphere(snapPoint.position, gizmoSphereSize * 0.8f);
        }

        // Show the expected guest name
        if (!string.IsNullOrEmpty(expectedGuest))
        {
            // This requires the Unity Editor to display properly
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.2f, expectedGuest);
#endif
        }
    }
}