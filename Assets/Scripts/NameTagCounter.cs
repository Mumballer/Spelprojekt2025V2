using UnityEngine;
using UnityEngine.UI;

public class NametagCounter : MonoBehaviour
{
    [SerializeField] private Text counterText;
    [SerializeField] private int totalNametags = 6;
    [SerializeField] private string displayFormat = "Nametags: {0}/{1}";

    private int placedNametags = 0;

    private void Start()
    {
        // Make sure we have a text component
        if (counterText == null)
        {
            counterText = GetComponent<Text>();
        }

        UpdateDisplay();
    }

    public void IncrementNametagCount()
    {
        placedNametags++;
        placedNametags = Mathf.Min(placedNametags, totalNametags); // Don't exceed total
        UpdateDisplay();
    }

    public void SetNametagCount(int count)
    {
        placedNametags = Mathf.Clamp(count, 0, totalNametags);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (counterText != null)
        {
            counterText.text = string.Format(displayFormat, placedNametags, totalNametags);
        }
    }
}