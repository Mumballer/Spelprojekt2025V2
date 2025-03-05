using UnityEngine;
using System.Collections;

public class NameTag : MonoBehaviour
{
    [Header("Nametag Settings")]
    [SerializeField] private string guestName;
    [SerializeField] private Transform originalParent;
    [SerializeField] private Vector3 originalPosition;
    [SerializeField] private Quaternion originalRotation;

    [Header("Interaction Settings")]
    [SerializeField] private GameObject promptText;
    [SerializeField] private float interactionDistance = 3f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip placeSound;

    private bool isPickedUp = false;
    private AudioSource audioSource;
    private Rigidbody rb;
    private Collider col;

    public bool IsPickedUp => isPickedUp;
    public string GuestName => guestName;

    private void Awake()
    {
        // Store original position and parent
        originalParent = transform.parent;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

        // Get components
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null && (pickupSound != null || placeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }
    }

    private void Start()
    {
        if (promptText != null)
        {
            promptText.SetActive(false);
        }
    }

    public void ShowPrompt(bool show)
    {
        if (promptText != null)
        {
            promptText.SetActive(show);
        }
    }

    public void PickUp(Transform holdPoint)
    {
        if (isPickedUp) return;

        isPickedUp = true;

        // Disable physics
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Disable collider to prevent clipping
        if (col != null)
        {
            col.enabled = false;
        }

        // Parent to the hold point
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Play sound
        if (audioSource != null && pickupSound != null)
        {
            audioSource.clip = pickupSound;
            audioSource.Play();
        }

        // Hide prompt
        ShowPrompt(false);

        // Notify the manager - use the NotifyNameTagPickup method instead
        if (NameTagManager.Instance != null)
        {
            NameTagManager.Instance.NotifyNameTagPickup(this);
        }
    }

    public void Place(Transform spot)
    {
        if (!isPickedUp) return;

        isPickedUp = false;

        // Parent to the placement spot
        transform.SetParent(spot);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Enable collider again
        if (col != null)
        {
            col.enabled = true;
        }

        // Keep rigidbody kinematic to prevent it from falling

        // Play sound
        if (audioSource != null && placeSound != null)
        {
            audioSource.clip = placeSound;
            audioSource.Play();
        }
    }

    public void Reset()
    {
        if (!isPickedUp) return;

        isPickedUp = false;

        // Re-enable physics
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        // Re-enable collider
        if (col != null)
        {
            col.enabled = true;
        }

        // Return to original position
        transform.SetParent(originalParent);
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
    }
}