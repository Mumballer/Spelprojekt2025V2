using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(LayoutElement))] // Need LayoutElement for size control
public class DialogChoiceButton : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("Sizing & Padding")]
    [Tooltip("Total horizontal padding (left + right)")]
    [SerializeField] private float horizontalPadding = 30f; // Increased default slightly
    [Tooltip("Total vertical padding (top + bottom)")]
    [SerializeField] private float verticalPadding = 15f; // Increased default slightly
    [Tooltip("Minimum width the button can have.")]
    [SerializeField] private float minWidth = 150f;
    [Tooltip("Minimum height the button can have.")]
    [SerializeField] private float minHeight = 45f;
    [Tooltip("Maximum width the button can have. Text will wrap if wider.")]
    [SerializeField] private float maxWidth = 500f; // Adjust as needed

    private LayoutElement layoutElement;
    private Button button;
    private RectTransform textRectTransform; // Cache text rect transform

    private void Awake()
    {
        layoutElement = GetComponent<LayoutElement>();
        button = GetComponent<Button>();

        if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null) textRectTransform = buttonText.GetComponent<RectTransform>();

        if (layoutElement == null || button == null || buttonText == null || textRectTransform == null)
        {
            Debug.LogError($"DialogChoiceButton on {gameObject.name} is missing required components!", gameObject);
            enabled = false; // Disable script if setup fails
            return;
        }

        // Configure LayoutElement defaults - prevent flexible expansion
        layoutElement.minWidth = minWidth;
        layoutElement.minHeight = minHeight;
        layoutElement.flexibleWidth = 0;
        layoutElement.flexibleHeight = 0;

        // Configure Text defaults
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.enableWordWrapping = true; // Ensure wrapping is enabled
        buttonText.overflowMode = TextOverflowModes.Overflow; // Allow vertical overflow if needed (height calc should handle)
        // Ensure text rect fills the button area minus padding (handled in Setup now)
    }

    public void Setup(string text, System.Action onClickAction)
    {
        if (!enabled) return; // Don't run if Awake failed

        // --- 1. Set Text Content ---
        buttonText.text = text;

        // --- 2. Calculate Text Preferred Size ---
        // Define the maximum width the *text itself* can occupy
        float maxTextWidth = maxWidth - horizontalPadding;

        // Get preferred values based on the text and the max width constraint
        // This calculation accounts for word wrapping
        Vector2 preferredTextSize = buttonText.GetPreferredValues(text, maxTextWidth, Mathf.Infinity);

        // --- 3. Calculate Button Size ---
        // Add padding back to get the desired button size
        float desiredWidth = preferredTextSize.x + horizontalPadding;
        float desiredHeight = preferredTextSize.y + verticalPadding;

        // --- 4. Apply Constraints ---
        // Clamp width between min and max
        float finalWidth = Mathf.Clamp(desiredWidth, minWidth, maxWidth);
        // Ensure height meets minimum, use calculated height otherwise
        float finalHeight = Mathf.Max(desiredHeight, minHeight);

        // --- 5. Apply Size to LayoutElement ---
        layoutElement.preferredWidth = finalWidth;
        layoutElement.preferredHeight = finalHeight;
        // Re-apply min values just in case, though Awake should handle it
        layoutElement.minWidth = minWidth;
        layoutElement.minHeight = minHeight;

        // --- 6. Adjust Text RectTransform (Optional but good practice) ---
        // Make text rect fill the button area defined by padding
        textRectTransform.anchorMin = Vector2.zero; // Bottom-left
        textRectTransform.anchorMax = Vector2.one;  // Top-right
        textRectTransform.offsetMin = new Vector2(horizontalPadding / 2f, verticalPadding / 2f); // Bottom-left padding
        textRectTransform.offsetMax = new Vector2(-horizontalPadding / 2f, -verticalPadding / 2f); // Top-right padding


        // --- 7. Setup Button Action ---
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickAction?.Invoke());

        // Debug.Log($"Button '{text}': MaxTextW={maxTextWidth:F1}, PrefText={preferredTextSize.x:F1}x{preferredTextSize.y:F1}, Desired={desiredWidth:F1}x{desiredHeight:F1}, Final={finalWidth:F1}x{finalHeight:F1}");
    }
}