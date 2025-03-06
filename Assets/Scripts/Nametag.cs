using UnityEngine;

public class NameTag : MonoBehaviour
{
    [Header("Nametag Information")]
    [SerializeField] private string guestName;
    [TextArea]
    public string guestDescription;

    [Header("Visual Elements")]
    public Renderer nametagRenderer;
    public TextMesh nameText;

    [Header("Interaction")]
    public Collider nametagCollider;
    public float pickupDistance = 2f;
    public bool isPickedUp = false;

    [Header("Physics")]
    public bool disablePhysicsWhenPlaced = true;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Rigidbody nametagRigidbody;
    private bool wasKinematic;

    // Property for guest name
    public string GuestName
    {
        get { return guestName; }
        set
        {
            guestName = value;
            // Update text mesh if available
            if (nameText != null)
                nameText.text = value;
        }
    }

    void Start()
    {
        // Store the original position and rotation
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Setup the nametag text if available
        if (nameText != null)
        {
            nameText.text = guestName;
        }

        // Cache the rigidbody if we have one
        nametagRigidbody = GetComponent<Rigidbody>();
        if (nametagRigidbody != null)
        {
            wasKinematic = nametagRigidbody.isKinematic;
        }
    }

    // Modified to accept optional Transform parameter
    public void PickUp(Transform holder = null)
    {
        isPickedUp = true;

        // Parent to holder if provided
        if (holder != null)
        {
            transform.SetParent(holder);
            transform.localPosition = Vector3.zero;
        }

        // Disable physics if we have a rigidbody
        if (nametagRigidbody != null)
        {
            nametagRigidbody.isKinematic = true;
            nametagRigidbody.linearVelocity = Vector3.zero;
            nametagRigidbody.angularVelocity = Vector3.zero;
        }
    }

    public void Drop()
    {
        isPickedUp = false;

        // Remove parent if we have one
        transform.SetParent(null);

        // Re-enable physics if we have a rigidbody
        if (nametagRigidbody != null)
        {
            nametagRigidbody.isKinematic = wasKinematic;
        }
    }

    public void ResetToOriginalPosition()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.SetParent(null); // Ensure no parent
        Drop();
    }

    // For backward compatibility
    public string GetGuestName()
    {
        return guestName;
    }

    // When the nametag is placed on the table
    public void PlaceOnTable(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        transform.SetParent(null); // Detach from any holder
        isPickedUp = false;

        // Disable physics when placed if requested
        if (disablePhysicsWhenPlaced && nametagRigidbody != null)
        {
            nametagRigidbody.isKinematic = true;
            nametagRigidbody.linearVelocity = Vector3.zero;
            nametagRigidbody.angularVelocity = Vector3.zero;
        }
    }

    // Can be used to highlight the nametag when selectable
    public void SetHighlighted(bool highlighted)
    {
        if (nametagRenderer != null)
        {
            // You can implement highlighting logic here
            // For example, changing material color or emissive properties
            Material mat = nametagRenderer.material;
            if (mat != null)
            {
                if (highlighted)
                {
                    // Example: Make it slightly brighter
                    mat.color = new Color(
                        mat.color.r * 1.2f,
                        mat.color.g * 1.2f,
                        mat.color.b * 1.2f
                    );
                }
                else
                {
                    // Reset to original color
                    mat.color = Color.white;
                }
            }
        }
    }

    // Helper method to determine if this nametag is interactable by the player
    public bool IsInteractable(Vector3 playerPosition)
    {
        return Vector3.Distance(transform.position, playerPosition) <= pickupDistance;
    }
}