using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TableSpot : MonoBehaviour
{
    // Name of the nametag assigned to this spot
    public string assignedNametag;
    // Reference to the TextMeshProUGUI element for UI feedback
    public TextMeshProUGUI nametagUIText;
    // Reference to the nametag that should be placed here
    public Nametag tableNametagScript;

    // Reference to the kitchen nametag that corresponds to this table spot
    public Nametag kitchenNametagScript;

    // Flag to track if player is in trigger zone
    private bool playerInTriggerZone = false;
    // Flag to track if this nametag has been placed
    private bool isNametagPlaced = false;

    private void Start()
    {
        // Ensure table nametag starts invisible
        if (tableNametagScript != null)
        {
            tableNametagScript.SetAlpha(0f);
        }

        // Make sure this object has a collider set as trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError("TableSpot needs a Collider component!");
        }
    }

    void Update()
    {
        // Only allow interaction if player is in trigger zone and E is pressed
        if (playerInTriggerZone && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    // Handle trigger enter for player detection
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTriggerZone = true;
            // Show prompt if we have the right nametag and it's not already placed
            if (!isNametagPlaced && HasPickedUpCorrectNametag())
            {
                nametagUIText.SetText("Press E to place " + assignedNametag + " nametag here");
            }
        }
    }

    // Handle trigger exit for player detection
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTriggerZone = false;
            nametagUIText.SetText("");
        }
    }

    // Check if the player has picked up the correct nametag
    private bool HasPickedUpCorrectNametag()
    {
        // If the kitchen nametag is invisible, we assume it was picked up
        return kitchenNametagScript != null &&
               kitchenNametagScript.name == assignedNametag &&
               kitchenNametagScript.IsPickedUp();
    }

    // Handle the interaction logic
    public void Interact()
    {
        if (!isNametagPlaced && HasPickedUpCorrectNametag())
        {
            // Show the placement confirmation
            nametagUIText.SetText("Placing " + assignedNametag + " nametag...");

            // Place the nametag
            ConfirmPlacement();

            // Hide prompt after delay
            StartCoroutine(HidePromptAfterDelay());
        }
    }

    // Confirm the placement
    public void ConfirmPlacement()
    {
        if (tableNametagScript != null && tableNametagScript.name == assignedNametag)
        {
            // Make the table nametag appear
            tableNametagScript.PlaceNametag();
            isNametagPlaced = true;
            Debug.Log("Nametag " + assignedNametag + " has been placed at the table.");
        }
        else
        {
            Debug.LogError("Table nametag script is not set or does not match the expected nametag.");
        }
    }

    // Coroutine to hide the prompt after a delay
    private IEnumerator HidePromptAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        nametagUIText.SetText("");
    }
}