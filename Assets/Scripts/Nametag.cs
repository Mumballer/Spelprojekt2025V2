using UnityEngine;

public class NameTag : MonoBehaviour
{
    [Header("Nametag Settings")]
    [SerializeField] private string guestName;
    [SerializeField] private string tagID; // This should match the chair's tagID
    [SerializeField] private bool isPickedUp = false;
    [SerializeField] private GameObject visualModel;
    [SerializeField] private GameObject interactionPrompt;

    [Header("Effects")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip placeSound;
    [SerializeField] private GameObject pickupEffect;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform playerHoldPoint;
    private AudioSource audioSource;

    public string TagID => tagID;
    public string GuestName => guestName;
    public bool IsPickedUp => isPickedUp;

    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        if (audioSource == null && (pickupSound != null || placeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f; // 3D sound
            audioSource.volume = 0.7f;
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        if (pickupEffect != null)
        {
            pickupEffect.SetActive(false);
        }
    }

    private void Start()
    {
        // Find the player's hold point
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            // Try to find an existing hold point
            playerHoldPoint = player.transform.Find("ItemHoldPoint");

            // If no hold point exists, create one
            if (playerHoldPoint == null)
            {
                GameObject holdPointObj = new GameObject("ItemHoldPoint");
                playerHoldPoint = holdPointObj.transform;
                playerHoldPoint.SetParent(player.transform);

                // Position it in front of the player camera
                Camera playerCamera = Camera.main;
                if (playerCamera != null)
                {
                    playerHoldPoint.position = playerCamera.transform.position +
                                              playerCamera.transform.forward * 0.5f;
                    playerHoldPoint.rotation = playerCamera.transform.rotation;
                }
                else
                {
                    playerHoldPoint.localPosition = new Vector3(0, 1.5f, 0.5f);
                }

                Debug.Log("Created ItemHoldPoint for player");
            }
        }
        else
        {
            Debug.LogWarning("No PlayerController found in scene");
        }
    }

    private void Update()
    {
        if (isPickedUp && playerHoldPoint != null)
        {
            // Keep the nametag at the player's hold point
            transform.position = playerHoldPoint.position;
            transform.rotation = playerHoldPoint.rotation;
        }
    }

    public void ShowInteractionPrompt(bool show)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(show);
        }
    }

    public void PickUp()
    {
        if (isPickedUp) return;

        isPickedUp = true;

        // Disable collider while held
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Play sound
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }

        // Show effect
        if (pickupEffect != null)
        {
            pickupEffect.SetActive(true);
            Invoke(nameof(HidePickupEffect), 1.5f);
        }

        // Notify the nametag manager
        NameTagManager.Instance?.OnNameTagPickedUp(this);

        Debug.Log($"Picked up {guestName}'s nametag");
    }

    private void HidePickupEffect()
    {
        if (pickupEffect != null)
        {
            pickupEffect.SetActive(false);
        }
    }

    public void Place(Transform placeTransform)
    {
        if (!isPickedUp) return;

        isPickedUp = false;

        // Position at the placement point
        transform.position = placeTransform.position;
        transform.rotation = placeTransform.rotation;

        // Re-enable collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        // Play sound
        if (audioSource != null && placeSound != null)
        {
            audioSource.PlayOneShot(placeSound);
        }

        Debug.Log($"Placed {guestName}'s nametag");
    }

    public void ReturnToOriginalPosition()
    {
        isPickedUp = false;
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // Re-enable collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        Debug.Log($"Returned {guestName}'s nametag to original position");
    }
}