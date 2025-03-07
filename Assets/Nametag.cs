using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Nametag : MonoBehaviour
{
    [Header("Nametag Settings")]
    // Unique name of the nametag
    public string name;
    // Whether this nametag is on the kitchen counter (true) or the dinner table (false)
    public bool isKitchenNametag = true;

    [Header("References")]
    // Reference to the TextMeshProUGUI element for UI feedback
    public TextMeshProUGUI nametagUIText;
    // Reference to the nametag's SpriteRenderer
    public SpriteRenderer nametagSprite;

    [Header("Interaction Settings")]
    // The radius around this nametag where the player can interact (only used for kitchen nametags)
    [SerializeField] private float interactionRadius = 1.5f;
    // Optional - tag of the player object (default: "Player")
    [SerializeField] private string playerTag = "Player";

    [Header("Table Placement")]
    // Reference to the table spot this nametag is placed at (if any)
    [SerializeField] private TableSpot currentTableSpot;

    [Header("Advanced Options")]
    // Whether to draw debug gizmos in the editor
    [SerializeField] private bool showDebugGizmos = true;
    // Color of the interaction radius gizmo
    [SerializeField] private Color gizmoColor = new Color(0.9f, 0.2f, 0.3f, 0.3f);

    // Flag to track if this nametag is picked up
    private bool isPickedUp = false;
    // Flag to check if the prompt is active
    private bool isPromptActive = false;
    // Flag to check if player is in interaction range
    private bool playerInRange = false;
    // Store the original color to restore it when needed
    private Color originalColor;
    // Reference to the player transform when in range
    private Transform playerTransform;

    private void Start()
    {
        // Store the original color
        if (nametagSprite != null)
        {
            originalColor = nametagSprite.color;
        }
        else
        {
            Debug.LogError($"Nametag is missing SpriteRenderer reference: {gameObject.name}");
        }

        // If this is a table nametag, start invisible
        if (!isKitchenNametag)
        {
            SetAlpha(0f);
        }
    }

    void Update()
    {
        // Only check for interaction if this is a kitchen nametag
        if (isKitchenNametag)
        {
            // Check if player is in range
            CheckPlayerDistance();

            // Process interaction when E is pressed
            if (playerInRange && Input.GetKeyDown(KeyCode.E))
            {
                if (!isPromptActive && !isPickedUp)
                {
                    Interact();
                }
                else if (isPromptActive)
                {
                    ConfirmOrCancelPickup();
                }
            }
        }
    }

    // Check if player is within interaction range
    private void CheckPlayerDistance()
    {
        // Find the player if we haven't already
        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
        }

        // If we have a player reference, check distance
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);

            // Check if player has entered range
            if (distance <= interactionRadius && !playerInRange)
            {
                playerInRange = true;
                OnPlayerEnterRange();
            }
            // Check if player has left range
            else if (distance > interactionRadius && playerInRange)
            {
                playerInRange = false;
                OnPlayerExitRange();
            }
        }
    }

    // Called when player enters the interaction range
    private void OnPlayerEnterRange()
    {
        // Only show prompt for kitchen nametags that haven't been picked up
        if (isKitchenNametag && !isPickedUp)
        {
            nametagUIText.SetText("Press E to pick up " + name + " nametag");
        }
    }

    // Called when player exits the interaction range
    private void OnPlayerExitRange()
    {
        // Clear prompt if it's showing the basic interaction message
        if (nametagUIText.text == "Press E to pick up " + name + " nametag")
        {
            nametagUIText.SetText("");
        }
    }

    // Handle the interaction logic
    public void Interact()
    {
        if (isKitchenNametag && !isPickedUp)
        {
            // Show the pickup prompt
            nametagUIText.SetText("Pick up " + name + " Nametag? Press E to confirm");
            isPromptActive = true;
            // Use coroutine instead of Invoke for better control
            StartCoroutine(HidePromptAfterDelay());
        }
    }

    // Confirm or cancel the pickup based on the second E press
    private void ConfirmOrCancelPickup()
    {
        if (isPromptActive)
        {
            // Confirm pickup
            ConfirmPickup();
        }
    }

    // Hide the prompt
    private void HidePrompt()
    {
        if (isPromptActive)
        {
            nametagUIText.SetText("");
            isPromptActive = false;
        }
    }

    // Coroutine to hide the prompt after a delay
    private IEnumerator HidePromptAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        HidePrompt();
    }

    // Call this function to confirm pickup
    public void ConfirmPickup()
    {
        // Make the kitchen nametag disappear by reducing alpha
        SetAlpha(0f);
        isPickedUp = true;
        Debug.Log("Nametag " + name + " has been picked up.");

        // Update the UI to indicate the nametag is being carried
        nametagUIText.SetText("Carrying " + name + " nametag");

        // Clear the prompt after a moment
        StartCoroutine(ClearCarryingTextAfterDelay());

        // Reset the prompt state
        isPromptActive = false;
    }

    // Coroutine to clear the "Carrying" text after a delay
    private IEnumerator ClearCarryingTextAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        nametagUIText.SetText("");
    }

    public void PlaceNametag()
    {
        // Make the table nametag appear by setting to white and full opacity
        if (nametagSprite != null)
        {
            // Set to pure white with full opacity
            Color whiteColor = Color.white;
            nametagSprite.color = whiteColor;
        }

        Debug.Log("Nametag " + name + " has been placed on the table.");
    }



    // Method to place nametag at a specific table spot
    public void PlaceAtSpot(TableSpot spot)
    {
        // Make the nametag visible
        SetAlpha(1f);

        // Clear previous spot if any
        if (currentTableSpot != null && currentTableSpot != spot)
        {
            currentTableSpot.ClearSpot();
        }

        // Store reference to new spot
        currentTableSpot = spot;

        // Tell the table spot about this nametag
        spot.SetNametag(gameObject, name);

        // Log placement
        Debug.Log("Nametag " + name + " has been placed on the table at " + spot.name);
    }

    // Helper function to set the alpha of the nametag
    public void SetAlpha(float alpha)
    {
        if (nametagSprite != null)
        {
            Color newColor = originalColor;
            newColor.a = alpha;
            nametagSprite.color = newColor;
        }
    }

    // Public method to check if this nametag has been picked up
    public bool IsPickedUp()
    {
        return isPickedUp;
    }

    // Draw gizmos to visualize the interaction range in the editor (only for kitchen nametags)
    private void OnDrawGizmos()
    {
        if (showDebugGizmos && isKitchenNametag)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, interactionRadius);
        }
    }
}