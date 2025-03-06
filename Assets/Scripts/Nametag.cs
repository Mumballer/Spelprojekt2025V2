using UnityEngine;
using System.Collections;

public class NameTag : MonoBehaviour
{
    [Header("Nametag Settings")]
    [SerializeField] private string guestName;
    [SerializeField] private Transform originalParent;
    [SerializeField] private Vector3 originalPosition;
    [SerializeField] private Quaternion originalRotation;
    [SerializeField] private Vector3 originalScale;

    [Header("Interaction Settings")]
    [SerializeField] private GameObject promptText;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip placeSound;

    private bool isPickedUp = false;
    private AudioSource audioSource;
    private Rigidbody rb;
    private Collider col;
    private bool isPlaced = false;

    public bool IsPickedUp => isPickedUp;
    public bool IsPlaced => isPlaced;
    public string GuestName => guestName;

    private void Awake()
    {
        originalParent = transform.parent;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        originalScale = transform.localScale;

        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null && (pickupSound != null || placeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
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
        isPlaced = false;

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        if (col != null)
        {
            col.enabled = false;
        }

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = originalScale;

        if (audioSource != null && pickupSound != null)
        {
            audioSource.clip = pickupSound;
            audioSource.Play();
        }

        ShowPrompt(false);

        if (NameTagManager.Instance != null)
        {
            NameTagManager.Instance.NotifyNameTagPickup(this);
        }
    }

    public void PlaceOnTable(Vector3 position, Quaternion rotation, Transform tableTransform)
    {
        if (!isPickedUp) return;

        isPickedUp = false;
        isPlaced = true;

        transform.SetParent(null);

        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = originalScale;

        transform.SetParent(tableTransform);

        if (col != null)
        {
            col.enabled = true;
        }

        if (audioSource != null && placeSound != null)
        {
            audioSource.clip = placeSound;
            audioSource.Play();
        }

        Debug.Log($"Placed {guestName}'s nametag at position {position}");

        if (NameTagManager.Instance != null)
        {
            NameTagManager.Instance.NotifyNameTagPlaced(this);
        }
    }

    public void Reset()
    {
        if (!isPickedUp) return;

        isPickedUp = false;
        isPlaced = false;

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        if (col != null)
        {
            col.enabled = true;
        }

        transform.SetParent(originalParent);
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        transform.localScale = originalScale;
    }
}