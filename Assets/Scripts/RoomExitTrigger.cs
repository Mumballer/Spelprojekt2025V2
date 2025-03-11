using UnityEngine;
using TMPro;

public class RoomExitTrigger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goToBedText;
    private bool hasTriggered = false;

    private void Start()
    {
        // Make sure the text is hidden initially
        goToBedText.gameObject.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if it's the player exiting the trigger
        if (other.CompareTag("Player") && !hasTriggered)
        {
            // Show the "GO TO BED" text
            goToBedText.gameObject.SetActive(true);

            // Set the flag so it only happens once
            hasTriggered = true;

            // Optional: You could add code here to hide the text after a few seconds
            // Uncomment this if you want the text to disappear after 3 seconds
            // Invoke("HideText", 3.0f);
        }
    }

    // Optional function if you want the text to disappear after a delay
    private void HideText()
    {
        goToBedText.gameObject.SetActive(false);
    }
}