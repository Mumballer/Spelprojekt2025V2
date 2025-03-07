using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Nametag : MonoBehaviour
{
    // Unique name of the nametag
    public string name;
    // Whether this nametag is on the kitchen counter (true) or the dinner table (false)
    public bool isKitchenNametag = true;
    // Reference to the TextMeshProUGUI element for UI feedback
    public TextMeshProUGUI nametagUIText;
    // Reference to the nametag's SpriteRenderer
    public SpriteRenderer nametagSprite;

    // Flag to track if this nametag is picked up
    private bool isPickedUp = false;
    // Flag to check if the prompt is active
    private bool isPromptActive = false;
    // Flag to check if player is in trigger zone
    private bool playerInTriggerZone = false;
    // Store the original color to restore it when needed
    private Color originalColor;

    private void Start()
    {
        // Store the original color
        originalColor = nametagSprite.color;

        // If this is a table nametag, start invisible
        if (!isKitchenNametag)
        {
            SetAlpha(0f);
        }

        // Make sure this object has a collider set as trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError("Nametag needs a Collider component!");
        }
    }

    void Update()
    {
        // Only allow interaction if player is in trigger zone and E is pressed
        if (playerInTriggerZone && Input.GetKeyDown(KeyCode.E))
        {
            if (!isPromptActive && isKitchenNametag && !isPickedUp)
            {
                Interact();
            }
            else if (isPromptActive)
            {
                ConfirmOrCancelPickup();
            }
        }
    }

    // Handle trigger enter for player detection
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTriggerZone = true;

            // Only show prompt for kitchen nametags that haven't been picked up
            if (isKitchenNametag && !isPickedUp)
            {
                nametagUIText.SetText("Press E to pick up " + name + " nametag");
            }
        }
    }

    // Handle trigger exit for player detection
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTriggerZone = false;

            // Clear prompt if it's showing the basic interaction message
            if (nametagUIText.text == "Press E to pick up " + name + " nametag")
            {
                nametagUIText.SetText("");
            }
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

    // Call this function to place the nametag on the assigned table spot
    public void PlaceNametag()
    {
        // Make the table nametag appear by increasing alpha
        SetAlpha(1f);
        Debug.Log("Nametag " + name + " has been placed on the table.");
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
}