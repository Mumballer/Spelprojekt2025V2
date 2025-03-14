using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(DialogTrigger))]
public class DialogSceneLoader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogManager dialogManager;
    [SerializeField] private GameObject dialogBox;

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
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
            Debug.Log("<color=cyan>DialogSceneLoader: Showing interaction prompt</color>");
        }
        
        if (dialogManager == null)
        {
            dialogManager = FindObjectOfType<DialogManager>();
            if (dialogManager != null)
            {
                Debug.Log("<color=green>DialogSceneLoader: Found DialogManager through FindObjectOfType</color>");
            }
        }
        
        if (dialogBox == null && dialogManager != null)
        {
            dialogBox = dialogManager.GetDialogBox();
            
            if (dialogBox == null)
            {
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
        // kollar efter knapptryck
        if (!hasStartedDialog && Input.GetKeyDown(interactKey))
        {
            Debug.Log("<color=cyan>DialogSceneLoader: Interaction key pressed</color>");
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
            
            if (dialogTrigger != null)
            {
                dialogTrigger.TriggerDialog();
                hasStartedDialog = true;
                Debug.Log("<color=cyan>DialogSceneLoader: Dialog triggered</color>");
            }
        }
        
        // kollar om dialog är slut
        if (dialogBox != null)
        {
            bool isDialogBoxActive = dialogBox.activeSelf;
            
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
        
        yield return new WaitForSeconds(delayAfterDialogComplete);
        
        if (fadeOutBeforeLoading)
        {
            yield return StartCoroutine(FadeToBlack());
        }
        
        // laddar nästa scen
        Debug.Log($"<color=cyan>DialogSceneLoader: Loading scene {sceneToLoad}</color>");
        SceneManager.LoadScene(sceneToLoad);
    }
    
    private IEnumerator FadeToBlack()
    {
        GameObject fadePanel = new GameObject("FadePanel");
        Canvas canvas = fadePanel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        
        RectTransform rectTransform = fadePanel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        
        UnityEngine.UI.Image fadeImage = fadePanel.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        
        // gör skärmen svart
        float startTime = Time.time;
        while (Time.time < startTime + fadeOutDuration)
        {
            float t = (Time.time - startTime) / fadeOutDuration;
            fadeImage.color = new Color(0, 0, 0, t);
            yield return null;
        }
        
        fadeImage.color = new Color(0, 0, 0, 1);
        yield return new WaitForSeconds(0.2f);
    }
} 