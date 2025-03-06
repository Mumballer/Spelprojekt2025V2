using UnityEngine;
using TMPro;

public class NametagCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private int totalNametags = 6;
    [SerializeField] private Color inProgressColor = Color.yellow;
    [SerializeField] private Color completedColor = Color.green;

    private int placedNametags = 0;

    private void Start()
    {
        UpdateCounterDisplay();

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
        if (NameTagManager.Instance != null)
        {
            NameTagManager.Instance.OnNameTagPlaced -= OnNametagPlaced;
        }
    }

    private void OnNametagPlaced(NameTag nameTag)
    {
        placedNametags++;
        UpdateCounterDisplay();
    }

    private void UpdateCounterDisplay()
    {
        if (counterText != null)
        {
            counterText.text = $"{placedNametags}/{totalNametags} nametags placed";
            counterText.color = (placedNametags >= totalNametags) ? completedColor : inProgressColor;
        }
    }
}