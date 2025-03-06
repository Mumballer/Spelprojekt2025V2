using UnityEngine;

public class NameTagSpot : MonoBehaviour
{
    [Header("Spot Settings")]
    [SerializeField] private string expectedGuest;
    [SerializeField] public Transform snapPoint;
    [SerializeField] private float snapDistance = 0.2f;

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
        if (nameTag != null && !hasNameTag)
        {
            // Snap nametag to position if we have a snap point
            if (snapPoint != null)
            {
                nameTag.transform.position = snapPoint.position;
                nameTag.transform.rotation = snapPoint.rotation;
            }

            hasNameTag = true;
            currentGuestName = nameTag.GuestName; // Using GuestName property

            // Notify table controller
            if (tableController != null)
            {
                tableController.OnNameTagPlaced(this, currentGuestName);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        NameTag nameTag = other.GetComponent<NameTag>();
        if (nameTag != null && hasNameTag)
        {
            hasNameTag = false;
            string previousGuest = currentGuestName;
            currentGuestName = "";

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