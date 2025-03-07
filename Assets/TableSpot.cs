using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TableSpot : MonoBehaviour
{
    // Name of the nametag assigned to this spot
    public string assignedNametag;

    // Reference to the TextMeshProUGUI element for UI feedback
    public TextMeshProUGUI nametagUIText;

    // Reference to the nametag that should be placed here
    public Nametag assignedNametagScript;

    // Update is called once per frame
    void Update()
    {
        // Check for interaction input
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    // Handle the interaction logic
    public void Interact()
    {
        // Get the nearby nametag
        Nametag nearbyNametag = GetNearbyNametag();

        if (nearbyNametag != null && nearbyNametag.name == assignedNametag)
        {
            // Show the placement prompt
            nametagUIText.SetText("Place " + nearbyNametag.name + " Nametag here?");

            // Wait for the player to confirm or cancel
            Invoke(nameof(HidePrompt), 1f);
        }
        else
        {
            // Clear the UI text
            nametagUIText.SetText("");
        }
    }

    // Confirm the placement
    public void ConfirmPlacement(Nametag nametag)
    {
        if (nametag.name == assignedNametag)
        {
            // Make the table nametag appear
            assignedNametagScript.gameObject.SetActive(true);

            // Clear the UI text
            nametagUIText.SetText("");

            // Update the UI to indicate the nametag is placed
            nametag.nametagUIText.SetText("");
        }
    }

    // Cancel the placement
    public void CancelPlacement()
    {
        nametagUIText.SetText("");
    }

    // Helper function to get the nearby nametag
    private Nametag GetNearbyNametag()
    {
        // Raycast from the camera to detect nametags
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 2f))
        {
            if (hit.transform.CompareTag("Nametag"))
            {
                return hit.transform.GetComponent<Nametag>();
            }
        }
        return null;
    }

    // Hide the prompt
    private void HidePrompt()
    {
        nametagUIText.SetText("");
    }
}