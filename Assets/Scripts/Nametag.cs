using UnityEngine;
using TMPro;

public class NameTag : MonoBehaviour
{
    [Header("Nametag Type")]
    [SerializeField] private bool isKitchenNametag = true; // true = pickable from kitchen, false = appears on table

    [Header("Nametag Information")]
    [SerializeField] private string guestName;
    [TextArea]
    public string guestDescription;

    [Header("Visual Elements")]
    public Renderer nametagRenderer;
    public TextMesh nameText;
    [SerializeField] private TextMeshPro tmpText; // Support for TextMeshPro

    [Header("Interaction")]
    public Collider nametagCollider;
    public float pickupDistance = 2.5f; // Increased for easier interaction
    public bool isPickedUp = false;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Physics")]
    public bool disablePhysicsWhenPlaced = true;
    public bool isPlaced = false; // Whether this nametag is placed

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
            // Update text if available
            if (nameText != null)
                nameText.text = value;
            if (tmpText != null)
                tmpText.text = value;
        }
    }

    // Expose type information
    public bool IsKitchenNametag => isKitchenNametag;
    public bool IsTableNametag => !isKitchenNametag;

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
        if (tmpText != null)
        {
            tmpText.text = guestName;
        }

        // Cache the rigidbody if we have one
        nametagRigidbody = GetComponent<Rigidbody>();
        if (nametagRigidbody != null)
        {
            wasKinematic = nametagRigidbody.isKinematic;
        }

        // If it's a table nametag, make sure it has a collider
        if (!isKitchenNametag && nametagCollider == null)
        {
            nametagCollider = GetComponent<Collider>();
        }
    }

    // Modified to accept optional Transform parameter
    public void PickUp(Transform holder = null)
    {
        // Only kitchen nametags can be picked up
        if (!isKitchenNametag) return;

        isPickedUp = true;
        isPlaced = false;

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
        // Only kitchen nametags can be dropped
        if (!isKitchenNametag) return;

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
        isPickedUp = false;
        isPlaced = false;
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
        // Only kitchen nametags can be placed
        if (!isKitchenNametag) return;

        transform.position = position;
        transform.rotation = rotation;
        transform.SetParent(null); // Detach from any holder
        isPickedUp = false;
        isPlaced = true;

        // Disable physics when placed if requested
        if (disablePhysicsWhenPlaced && nametagRigidbody != null)
        {
            nametagRigidbody.isKinematic = true;
            nametagRigidbody.linearVelocity = Vector3.zero;
            nametagRigidbody.angularVelocity = Vector3.zero;
        }

        // Notify that the nametag has been placed
        Debug.Log($"Fired OnNametagPlaced event for {guestName}");
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
        // Only kitchen nametags that aren't placed can be interacted with
        if (!isKitchenNametag || isPlaced) return false;

        return Vector3.Distance(transform.position, playerPosition) <= pickupDistance;
    }

    // Show/hide the nametag (for table nametags)
    public void SetVisibility(bool visible)
    {
        if (nametagRenderer != null)
        {
            nametagRenderer.enabled = visible;
        }

        if (nameText != null && nameText.GetComponent<Renderer>() != null)
        {
            nameText.GetComponent<Renderer>().enabled = visible;
        }

        if (tmpText != null)
        {
            tmpText.enabled = visible;
            // Also adjust alpha
            Color textColor = tmpText.color;
            textColor.a = visible ? 1f : 0f;
            tmpText.color = textColor;
        }

        if (nametagCollider != null)
        {
            nametagCollider.enabled = visible && isKitchenNametag;
        }
    }

    // For debugging in editor
    private void OnDrawGizmosSelected()
    {
        if (isKitchenNametag)
        {
            // Show pickup range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupDistance);
        }

        // Show status
        if (isPlaced)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.2f, new Vector3(0.1f, 0.1f, 0.1f));
        }
        else if (isPickedUp)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.2f, new Vector3(0.1f, 0.1f, 0.1f));
        }
    }
}