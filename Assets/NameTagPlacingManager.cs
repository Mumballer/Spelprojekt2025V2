using UnityEngine;
using UnityEngine.UI;

public class NameTagPlacingManager : MonoBehaviour
{
    public static NameTagPlacingManager Instance { get; private set; } // Singleton instance
    public Text uiText; // UI text to display nametag information
    public Transform counterGroup; // Container for counter nametags
    public Transform tableGroup; // Container for table nametags

    private NameTags currentTag; // Currently held nametag
    private bool isSwapping = false; // Is the player trying to swap a nametag?

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PickUpNametag(NameTags tag)
    {
        if (currentTag == null)
        {
            currentTag = tag;
            tag.gameObject.SetActive(false); // Hide the counter nametag
            uiText.text = $"Picked up: {tag.name}";
        }
    }

    public void TryPlaceNametag(SeatTag seat)
    {
        if (currentTag != null && currentTag.name == seat.expectedName)
        {
            // Show the table nametag at the correct position
            Transform tableNametagTransform = seat.transform.Find(currentTag.name);
            if (tableNametagTransform != null)
            {
                tableNametagTransform.gameObject.SetActive(true);
                uiText.text = "Nametag placed correctly!";
            }
            currentTag = null;
        }
        else
        {
            uiText.text = "Wrong position!";
        }
    }

    public void HoverSeat(SeatTag seat)
    {
        uiText.text = $"Place: {seat.expectedName}";
    }

    public void StopHovering()
    {
        uiText.text = "";
    }
}