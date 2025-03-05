using UnityEngine;
using TMPro;

public class NametagCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText; // This will replace the list of names
    [SerializeField] private int totalNametags = 6;
    [SerializeField] private Color inProgressColor = Color.yellow;
    [SerializeField] private Color completedColor = Color.green;

    private int placedNametags = 0;

    private void Start()
    {
        // Initialize counter
        UpdateCounterDisplay();

        // Subscribe to NameTagManager events
        if (NameTagManager.Instance != null)
        {
            NameTagManager.Instance.OnNameTagPlaced += OnNametagPlaced;
        }
        else
        {
            Debug.LogWarning("NameTagManager instance not found. Counter won't update automatically.");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (NameTagManager.Instance != null)
        {
            NameTagManager.Instance.OnNameTagPlaced -= OnNametagPlaced;
        }
    }

    private void OnNametagPlaced(ChairNameTagSpot spot, NameTag nameTag, bool isCorrect)
    {
        // Only increment counter if placed correctly
        if (isCorrect)
        {
            placedNametags++;
            UpdateCounterDisplay();
        }
    }

    private void UpdateCounterDisplay()
    {
        if (counterText != null)
        {
            // Just show the counter: "0/6 nametags placed"
            counterText.text = $"{placedNametags}/{totalNametags} nametags placed";

            // Change color when complete
            counterText.color = (placedNametags >= totalNametags) ? completedColor : inProgressColor;
        }
    }
}