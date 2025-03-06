using UnityEngine;
using UnityEngine.UI; 

public class NameTagPlacingManager : MonoBehaviour
{
    public Text uiText; // Reference to the UI Text component
    private NameTag currentNametag; // Current nametag being held

    public void PickUpNametag(NameTag nametag)
    {
        if (currentNametag == null)
        {
            currentNametag = nametag;
            nametag.gameObject.SetActive(false); // Hide the picked-up nametag
            uiText.text = "Picked up: " + nametag.name;
        }
    }

    public void TryPlaceNametag(SeatTag seatTag)
    {
        if (currentNametag != null && currentNametag.name == seatTag.expectedName)
        {
            // Show the corresponding table nametag
            GameObject tableNametag = GameObject.Find(seatTag.expectedName);
            if (tableNametag != null)
            {
                tableNametag.SetActive(true);
                uiText.text = "Nametag placed correctly!";
            }
            currentNametag = null;
        }
        else
        {
            uiText.text = "Wrong position!";
        }
    }
}
