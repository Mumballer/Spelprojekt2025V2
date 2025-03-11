using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public float interactionDistance = 2f;
    public UnityEvent onInteract;
    public string promptText = "Press E to interact";
    public string hoverHighlightColor = "#FFFF00"; // Yellow highlight color in hex

    private bool isPlayerInRange = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isHighlighted = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            onInteract?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // Show interaction prompt
            UIManager.Instance.ShowInteractPrompt(promptText);

            // Highlight object if it has a sprite renderer
            HighlightObject(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            // Hide interaction prompt
            UIManager.Instance.HideInteractPrompt();

            // Remove highlight
            HighlightObject(false);
        }
    }

    // Method to highlight object when player is nearby
    private void HighlightObject(bool highlight)
    {
        if (spriteRenderer != null && highlight != isHighlighted)
        {
            isHighlighted = highlight;

            if (highlight)
            {
                // Color.TryParseHexString is available in newer Unity versions
                if (ColorUtility.TryParseHtmlString(hoverHighlightColor, out Color highlightColor))
                {
                    // Keep the original alpha
                    highlightColor.a = originalColor.a;
                    spriteRenderer.color = highlightColor;
                }
            }
            else
            {
                // Restore original color
                spriteRenderer.color = originalColor;
            }
        }
    }

    // Call this to update the original color (useful for name tags whose alpha changes)
    public void UpdateOriginalColor(Color newColor)
    {
        originalColor = newColor;

        // If not highlighted, apply the new color immediately
        if (!isHighlighted && spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
}