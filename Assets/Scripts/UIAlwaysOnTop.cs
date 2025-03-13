using UnityEngine;

/// <summary>
/// Place this script on any UI element to make it appear on top of all other UI elements.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIAlwaysOnTop : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int sortingOrder = 999;
    [SerializeField] private bool applyOnAwake = true;
    [SerializeField] private bool applyOnEnable = true;

    private Canvas canvas;
    private Canvas parentCanvas;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (applyOnAwake)
        {
            MakeTopMost();
        }
    }

    private void OnEnable()
    {
        if (applyOnEnable)
        {
            MakeTopMost();
        }
    }

    /// <summary>
    /// Makes this UI element appear on top of all other UI elements.
    /// </summary>
    public void MakeTopMost()
    {
        // Try to get or add a Canvas component
        canvas = GetComponent<Canvas>();

        if (canvas == null)
        {
            // Check if we need to add a Canvas
            if (transform.parent != null)
            {
                // Look for a parent Canvas
                parentCanvas = GetComponentInParent<Canvas>();

                if (parentCanvas != null)
                {
                    // Use Canvas Group instead of adding a new Canvas
                    canvasGroup = GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = gameObject.AddComponent<CanvasGroup>();
                    }

                    // Ensure this canvas group doesn't block raycasts for elements beneath it
                    canvasGroup.blocksRaycasts = true;

                    // Add a high Z position to move the UI element in front visually
                    RectTransform rectTransform = GetComponent<RectTransform>();
                    Vector3 position = rectTransform.localPosition;
                    position.z = -100; // Negative Z brings it forward in UI space
                    rectTransform.localPosition = position;

                    Debug.Log($"<color=cyan>UIAlwaysOnTop: {gameObject.name} is now visually on top using positioning</color>");
                }
                else
                {
                    // No parent canvas found, add our own
                    canvas = gameObject.AddComponent<Canvas>();
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = sortingOrder;

                    // Add a raycaster so we can interact with this UI
                    if (GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                    {
                        gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                    }

                    Debug.Log($"<color=cyan>UIAlwaysOnTop: {gameObject.name} is now on top using a new Canvas</color>");
                }
            }
            else
            {
                // Root object, add a Canvas
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = sortingOrder;

                // Add a raycaster so we can interact with this UI
                if (GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                {
                    gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }

                Debug.Log($"<color=cyan>UIAlwaysOnTop: {gameObject.name} is now on top as a root Canvas</color>");
            }
        }
        else
        {
            // We already have a Canvas, just set its sorting order
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
            Debug.Log($"<color=cyan>UIAlwaysOnTop: {gameObject.name} is now on top with sorting order {sortingOrder}</color>");
        }
    }

    /// <summary>
    /// Increases the sorting order to ensure this UI element is above others.
    /// </summary>
    public void BringToFront()
    {
        sortingOrder += 10;
        MakeTopMost();
    }
}