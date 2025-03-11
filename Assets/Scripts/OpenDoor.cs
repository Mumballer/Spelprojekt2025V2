using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactionDistance = 2.0f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Components")]
    [SerializeField] private Animation doorAnimation;
    [SerializeField] private string openAnimationName = "DoorOpen";
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip doorSound;

    private Transform playerTransform;
    private bool isOpen = false;

    private void Start()
    {
        // Find the player
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Get Animation component if not assigned
        if (doorAnimation == null)
        {
            doorAnimation = GetComponent<Animation>();
        }

        // Get or add audio source if needed
        if (audioSource == null && doorSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    private void Update()
    {
        // Check if player is close enough
        if (playerTransform != null &&
            Vector3.Distance(transform.position, playerTransform.position) <= interactionDistance)
        {
            // Detect key press
            if (Input.GetKeyDown(interactionKey))
            {
                ToggleDoor();
            }
        }
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;

        // Play animation
        if (doorAnimation != null)
        {
            // If door is open, play animation forward, otherwise play in reverse
            if (isOpen)
            {
                doorAnimation[openAnimationName].speed = 1;
                doorAnimation.Play(openAnimationName);
            }
            else
            {
                doorAnimation[openAnimationName].speed = -1;
                doorAnimation[openAnimationName].time = doorAnimation[openAnimationName].length;
                doorAnimation.Play(openAnimationName);
            }

            // Play sound
            if (audioSource != null && doorSound != null)
            {
                audioSource.clip = doorSound;
                audioSource.Play();
            }
        }
    }

    // Optional: visualize the interaction range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
