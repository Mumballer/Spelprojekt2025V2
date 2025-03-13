using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(DialogTrigger))]
public class DialogSceneLoader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogManager dialogManager; // Direct reference
    [SerializeField] private GameObject dialogBox; // Reference to the dialog UI box

    [Header("Scene Loading Settings")]
    [SerializeField] private string sceneToLoad;
    [SerializeField] private float delayAfterDialogComplete = 0.5f;
    [SerializeField] private bool fadeOutBeforeLoading = true;
    [SerializeField] private float fadeOutDuration = 1f;
    
    [Header("Interaction Settings")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    [Header("Debug Options")]
    [SerializeField] private bool debugMode = true;
    
    private DialogTrigger dialogTrigger;
    private bool hasStartedDialog = false;
    private bool isLoadingScene = false;
    private bool wasDialogBoxActive = false;
    
    private void Start()
    {
        dialogTrigger = GetComponent<DialogTrigger>();
        
        Debug.Log($"<color=cyan>DialogSceneLoader ready on {gameObject.name}, scene to load: {sceneToLoad}</color>");
        
        // Show prompt immediately
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
            Debug.Log("<color=cyan>DialogSceneLoader: Showing interaction prompt</color>");
        }
        
        // Try to find DialogManager if not assigned
        if (dialogManager == null)
        {
            dialogManager = FindObjectOfType<DialogManager>();
            if (dialogManager != null)
            {
                Debug.Log("<color=green>DialogSceneLoader: Found DialogManager through FindObjectOfType</color>");
            }
        }
        
        // Try to find dialog box if not assigned
        if (dialogBox == null && dialogManager != null)
        {
            // Try to get it from the DialogManager
            dialogBox = dialogManager.GetDialogBox();
            
            if (dialogBox == null)
            {
                // Fallback: search by common names
                dialogBox = GameObject.Find("DialogBox");
                if (dialogBox == null) dialogBox = GameObject.Find("DialogPanel");
                if (dialogBox == null) dialogBox = GameObject.Find("DialogUI");
            }
            
            if (dialogBox != null)
            {
                Debug.Log("<color=green>DialogSceneLoader: Found dialog box UI element</color>");
            }
            else
            {
                Debug.LogWarning("<color=yellow>DialogSceneLoader: Could not find dialog box UI element</color>");
            }
        }
    }
    
    private void Update()
    {
        // APPROACH 1: Check for interaction key to start dialog
        if (!hasStartedDialog && Input.GetKeyDown(interactKey))
        {
            Debug.Log("<color=cyan>DialogSceneLoader: Interaction key pressed</color>");
            
            // Hide the prompt
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
            
            // Start dialog
            if (dialogTrigger != null)
            {
                dialogTrigger.TriggerDialog();
                hasStartedDialog = true;
                Debug.Log("<color=cyan>DialogSceneLoader: Dialog triggered</color>");
            }
        }
        
        // APPROACH 2: Monitor dialog box active state to detect when dialog completes
        if (dialogBox != null)
        {
            bool isDialogBoxActive = dialogBox.activeSelf;
            
            // Detect when dialog box was active but is now inactive (dialog ended)
            if (wasDialogBoxActive && !isDialogBoxActive && hasStartedDialog && !isLoadingScene)
            {
                Debug.Log("<color=green>DialogSceneLoader: Dialog box became inactive - dialog completed</color>");
                StartCoroutine(LoadSceneAfterDelay());
            }
            
            wasDialogBoxActive = isDialogBoxActive;
        }
    }
    
    private IEnumerator LoadSceneAfterDelay()
    {
        if (isLoadingScene) yield break;
        isLoadingScene = true;
        
        Debug.Log($"<color=green>DialogSceneLoader: Preparing to load scene: {sceneToLoad}</color>");
        
        // Wait a short delay after dialog completes
        yield return new WaitForSeconds(delayAfterDialogComplete);
        
        // Do fade out if requested
        if (fadeOutBeforeLoading)
        {
            yield return StartCoroutine(FadeToBlack());
        }
        
        // Load the scene
        Debug.Log($"<color=cyan>DialogSceneLoader: Loading scene {sceneToLoad}</color>");
        SceneManager.LoadScene(sceneToLoad);
    }
    
    private IEnumerator FadeToBlack()
    {
        // Create temporary fade panel
        GameObject fadePanel = new GameObject("FadePanel");
        Canvas canvas = fadePanel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Make sure it's on top
        
        RectTransform rectTransform = fadePanel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        
        UnityEngine.UI.Image fadeImage = fadePanel.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        
        // Fade over time
        float startTime = Time.time;
        while (Time.time < startTime + fadeOutDuration)
        {
            float t = (Time.time - startTime) / fadeOutDuration;
            fadeImage.color = new Color(0, 0, 0, t);
            yield return null;
        }
        
        fadeImage.color = new Color(0, 0, 0, 1);
        yield return new WaitForSeconds(0.2f); // Short pause at full black
    }
} 