using System;
using UnityEngine;

public class TableSpot : MonoBehaviour
{
    [Header("References")]
    public GameObject tableNametag;
    public GameObject highlightObject;
    public Material correctPlacementMaterial;
    public Material incorrectPlacementMaterial;
    public Material defaultMaterial;

    [Header("Settings")]
    public string assignedNametag; // The name of the nametag that should go here
    public bool isInteractable = true;
    public float feedbackDuration = 1.0f;
    [Tooltip("Maximum distance the player can be to interact with this spot")]
    public float interactionDistance = 2.0f; // Add this line - distance player can interact from

    [Header("Debug")]
    public bool showDebug = false;

    // Event that fires when a nametag is placed at this spot
    public event Action<TableSpot, string> OnNametagPlaced;

    // Reference to player for checking held nametags
    private GameObject player;
    private Transform playerTransform;
    private Nametag heldNametag;
    private Renderer spotRenderer;
    private Material originalMaterial;
    private bool playerInRange = false;

    private void Start()
    {
        // Find the player
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Get renderer if available
        spotRenderer = GetComponent<Renderer>();
        if (spotRenderer != null)
        {
            originalMaterial = spotRenderer.material;
        }

        // Initialize highlight state
        if (highlightObject != null)
        {
            highlightObject.SetActive(false);
        }

        // Check if there's already a nametag component on this spot
        if (tableNametag != null)
        {
            Nametag existingTag = tableNametag.GetComponent<Nametag>();
            if (existingTag != null)
            {
                Debug.Log($"Nametag component found on {tableNametag.name}. IsPickedUp: {existingTag.IsPickedUp()}");
                // Make sure the nametag is invisible at start
                existingTag.SetAlpha(0f);
            }
            else
            {
                Debug.LogError($"TableSpot has tableNametag reference but no Nametag component on {tableNametag.name}");
            }
        }
    }

    private void Update()
    {
        // Check if player is in range
        CheckPlayerDistance();

        // Handle interaction
        if (playerInRange && isInteractable && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    private void CheckPlayerDistance()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool inRange = distance <= interactionDistance;

        // Only update highlight if the range status changed
        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            ShowHighlight(playerInRange);
        }
    }

    public void ShowHighlight(bool show)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(show && isInteractable);
        }
    }

    public void Interact()
    {
        // If this spot already has a nametag, make it visible instead of just logging a message
        if (tableNametag != null)
        {
            Debug.Log($"This spot already has a nametag: {tableNametag.name}. Making it visible.");

            // Get the nametag component
            Nametag nametagComponent = tableNametag.GetComponent<Nametag>();
            if (nametagComponent != null)
            {
                // Make the existing nametag visible
                nametagComponent.PlaceNametag();

                // Fire the event with this spot and the nametag name
                if (OnNametagPlaced != null)
                {
                    OnNametagPlaced.Invoke(this, nametagComponent.name);
                    Debug.Log($"Fired OnNametagPlaced event for {nametagComponent.name}");
                }

                // Show positive feedback if you have a material for that
                if (correctPlacementMaterial != null)
                {
                    ShowFeedback(correctPlacementMaterial, feedbackDuration);
                }

                // Hide the highlight indicator
                if (highlightObject != null)
                {
                    highlightObject.SetActive(false);
                }

                // Prevent further interaction
                isInteractable = false;
            }
            else
            {
                Debug.LogError($"Found tableNametag reference but it has no Nametag component: {tableNametag.name}");
            }

            return;
        }

        // Try to get the player's held nametag
        if (player != null)
        {
            heldNametag = player.GetComponentInChildren<Nametag>();

            if (heldNametag != null)
            {
                // Check if this is the correct nametag for this spot
                bool isCorrectNametag = string.IsNullOrEmpty(assignedNametag) ||
                                        heldNametag.name == assignedNametag;

                // The player is carrying a nametag, place it at this spot
                Debug.Log($"Placing nametag {heldNametag.name} at spot {gameObject.name}");

                // Tell the nametag to place itself here
                heldNametag.PlaceAtSpot(this);

                // Show visual feedback
                if (isCorrectNametag)
                {
                    if (correctPlacementMaterial != null)
                    {
                        ShowFeedback(correctPlacementMaterial, feedbackDuration);
                    }

                    // Fire event
                    if (OnNametagPlaced != null)
                    {
                        OnNametagPlaced.Invoke(this, heldNametag.name);
                    }
                }
                else
                {
                    if (incorrectPlacementMaterial != null)
                    {
                        ShowFeedback(incorrectPlacementMaterial, feedbackDuration);
                    }
                }

                // Prevent further interaction if correct placement
                if (isCorrectNametag)
                {
                    isInteractable = false;
                }
            }
            else
            {
                Debug.Log("No nametag is being carried by the player.");
            }
        }
    }

    // Show visual feedback for nametag placement
    public void ShowFeedback(Material feedbackMaterial, float duration)
    {
        if (spotRenderer != null && feedbackMaterial != null)
        {
            spotRenderer.material = feedbackMaterial;
            Invoke("ResetMaterial", duration);
        }
    }

    // Reset material after feedback
    private void ResetMaterial()
    {
        if (spotRenderer != null)
        {
            spotRenderer.material = originalMaterial;
        }
    }

    // Set the nametag for this spot
    public void SetNametag(GameObject nametag, string nametagName)
    {
        tableNametag = nametag;
        assignedNametag = nametagName;

        if (showDebug)
        {
            Debug.Log($"Set nametag {nametagName} at spot {gameObject.name}");
        }
    }

    // Clear the nametag from this spot
    public void ClearSpot()
    {
        tableNametag = null;

        if (showDebug)
        {
            Debug.Log($"Cleared nametag from spot {gameObject.name}");
        }
    }

    private void OnDrawGizmos()
    {
        // Draw a visual indicator for this spot in the editor
        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f);
        Gizmos.DrawCube(transform.position, new Vector3(0.1f, 0.01f, 0.1f));

        // Draw interaction range
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.2f);
        Gizmos.DrawSphere(transform.position, interactionDistance);

        // Draw text for the assigned nametag
        if (!string.IsNullOrEmpty(assignedNametag))
        {
            Gizmos.color = Color.white;
            // You'd typically use handles for text, but that requires UnityEditor namespace
        }
    }
}