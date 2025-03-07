using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Nametag : MonoBehaviour
{
    // Unique name of the nametag
    public string name;

    // Whether this nametag is on the kitchen counter (true) or the dinner table (false)
    public bool isKitchenNametag = true;

    // Reference to the TextMeshProUGUI element for UI feedback
    public TextMeshProUGUI nametagUIText;

    // Reference to the assigned table spot
    public TableSpot assignedTableSpot;

    // Flag to check if the prompt is active
    private bool isPromptActive = false;

    // Update is called once per frame
    void Update()
    {
        // Check for interaction input
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isPromptActive)
            {
                Interact();
            }
            else
            {
                // If prompt is active, check if it's time to confirm or cancel
                ConfirmOrCancelPickup();
            }
        }
    }

    // Handle the interaction logic
    public void Interact()
    {
        if (isKitchenNametag)
        {
            // Show the pickup prompt
            nametagUIText.SetText("Pick up " + name + " Nametag?");
            isPromptActive = true;

            // Wait for 1 second before automatically canceling
            Invoke(nameof(HidePrompt), 1f);
        }
        else
        {
            // Handle placement logic
            assignedTableSpot.Interact();
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
            CancelPickup();
        }
    }

    // Call this function to confirm pickup
    public void ConfirmPickup()
    {
        // Make the kitchen nametag disappear
        gameObject.SetActive(false);

        // Update the UI to indicate the nametag is being carried
        nametagUIText.SetText(name);

        // Reset the prompt state
        isPromptActive = false;
    }

    // Call this function to cancel pickup
    public void CancelPickup()
    {
        nametagUIText.SetText("");
        isPromptActive = false;
    }

    // Call this function to place the nametag on the assigned table spot
    public void PlaceNametag()
    {
        // Make the table nametag appear
        gameObject.SetActive(true);

        // Clear the UI text
        nametagUIText.SetText("");
    }

    // Optional: Add debug logs to track the flow
    void OnEnable()
    {
        Debug.Log("Nametag " + name + " is active.");
    }

    void OnDisable()
    {
        Debug.Log("Nametag " + name + " is inactive.");
    }
}