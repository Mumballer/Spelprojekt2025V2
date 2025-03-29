using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq; // Add this for Linq operations if needed later

public class DialogManager : MonoBehaviour
{
    [Header("Dialog UI")]
    [SerializeField] GameObject dialogBox;
    [SerializeField] TextMeshProUGUI dialogText;
    [SerializeField] int lettersPerSecond = 30;
    [SerializeField] GameObject choicesContainer;
    [SerializeField] GameObject choiceButtonPrefab;
    [SerializeField] private float cooldownDuration = 3f;
    [SerializeField] private float maxButtonWidth = 350f;

    [Header("Portrait System")]
    [SerializeField] private GameObject portraitContainer;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private RectTransform portraitFrame;
    [SerializeField] private float defaultPortraitSize = 100f;
    [SerializeField] private Vector2 defaultPortraitOffset = Vector2.zero;

    [Header("3D Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactableLayers;

    [Header("Camera and Input Settings")]
    [SerializeField] private bool lockCameraDuringDialog = true;
    [SerializeField] private bool allowCameraEffectsDuringDialog = true;
    [SerializeField] private bool showCursorDuringDialog = true;
    [SerializeField] private string[] cameraControlComponentNames = new string[] 
    { 
        "MouseLook", 
        "FirstPersonLook",
        "PlayerLook",
        "LookController" 
    };

    public event Action OnShowDialog;
    public event Action OnHideDialog;
    public event System.Action<Dialog> OnDialogComplete;
    public static DialogManager Instance { get; private set; }

    private Dialog currentDialog; // Renamed from 'dialog' for clarity
    private int currentLine = 0;
    private bool isTyping;
    private List<GameObject> currentChoiceButtons = new List<GameObject>();
    private Coroutine typingCoroutine;
    private PlayerController playerController;
    private bool isOnCooldown = false;
    private Camera mainCamera;

    public bool IsDialogActive { get; private set; }

    private Queue<DialogLine> remainingLines = new Queue<DialogLine>();

    private void Awake()
    {
        Instance = this;
        playerController = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        mainCamera = Camera.main;

        if (dialogBox != null)
        {
            dialogBox.SetActive(false);
            if (choicesContainer != null)
            {
                choicesContainer.SetActive(false);
            }
            if (portraitContainer != null)
            {
                portraitContainer.SetActive(false);
            }
        }

        if (choiceButtonPrefab != null)
        {
            choiceButtonPrefab.SetActive(false);
        }
    }

    private void Start()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.Log("Dialog UI is set up for 3D world space");
        }
        SetupChoicesContainer();
    }

    private void SetupChoicesContainer()
    {
        if (choicesContainer == null) return;
        VerticalLayoutGroup layoutGroup = choicesContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = choicesContainer.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.spacing = 10f;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
        }

        ContentSizeFitter sizeFitter = choicesContainer.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = choicesContainer.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    private void Update()
    {
        if (IsDialogActive)
        {
            HandleUpdate();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteractWithNPC();
        }
    }

    private void TryInteractWithNPC()
    {
        if (!CanStartDialog() || mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayers))
        {
            DialogTrigger trigger = hit.collider.GetComponent<DialogTrigger>();
            if (trigger != null)
            {
                Debug.Log($"Found dialog trigger on {hit.collider.gameObject.name}");
                trigger.TriggerDialog();
            }
            else
            {
                Debug.Log($"No dialog trigger found on {hit.collider.gameObject.name}");
            }
        }
        else
        {
            Debug.Log("No interactable object hit by raycast");
        }
    }

    public void StartDialog(Dialog dialogToShow)
    {
        if (dialogToShow == null || dialogToShow.Lines == null || dialogToShow.Lines.Count == 0)
        {
            Debug.LogError("DialogManager: Cannot start null or empty dialog.");
            return;
        }
        if (IsDialogActive)
        {
            Debug.LogWarning("DialogManager: Tried to start a new dialog while one is already active.");
            return; // Don't start a new one if already active
        }

        Debug.Log($"<color=yellow>Starting Dialog: {dialogToShow.name}</color>");
        currentDialog = dialogToShow;
        remainingLines.Clear();

        // Queue up all lines
        foreach (var line in currentDialog.Lines)
        {
            remainingLines.Enqueue(line);
        }

        // Lock camera, show cursor, disable player movement (existing logic)
        if (lockCameraDuringDialog && mainCamera != null)
        {
            var cameraComponents = mainCamera.GetComponents<MonoBehaviour>();
            foreach (var comp in cameraComponents)
            {
                string componentName = comp.GetType().Name;
                
                // Disable only specific camera control components
                bool shouldDisable = false;
                
                if (cameraControlComponentNames.Length > 0)
                {
                    // Use the explicit list of components to disable
                    foreach (var name in cameraControlComponentNames)
                    {
                        if (componentName.Contains(name))
                        {
                            shouldDisable = true;
                            break;
                        }
                    }
                }
                else
                {
                    // Fallback to generic naming detection for camera control
                    shouldDisable = (componentName.Contains("Look") && componentName.Contains("Mouse")) ||
                                   (componentName.Contains("Look") && componentName.Contains("Player")) || 
                                   componentName.Contains("PlayerController");
                }
                
                // Only disable the component if it's a control component
                // and not a camera effect (like shake)
                if (shouldDisable && 
                    !(allowCameraEffectsDuringDialog && 
                      (componentName.Contains("Shake") || 
                       componentName.Contains("Effect") || 
                       componentName.Contains("PostProcessing"))))
                {
                    comp.enabled = false;
                    Debug.Log($"<color=cyan>DialogManager: Disabled camera component: {componentName}</color>");
                }
            }
        }
        
        if (showCursorDuringDialog)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        playerController?.SetCanMove(false);
        
        // Show dialog panel and set active state
        dialogBox.SetActive(true);
        IsDialogActive = true; // Set active *before* showing first line
        
        // Clear any leftover choices UI just in case
        ClearChoiceButtons();
        choicesContainer?.SetActive(false);
        portraitContainer?.SetActive(false); // Hide portrait initially
        
        // Start showing the first line
        DisplayNextLine();
        
        OnShowDialog?.Invoke();
    }

    public void DisplayNextLine()
    {
        // If typing, finish immediately (optional, but common UX)
        if (isTyping && typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            // Assuming TypeText sets the full text at the end
            // If not, you'd need to set the full text here:
            // dialogText.text = [full text of the line being typed];
            isTyping = false; // Ensure typing flag is cleared

            // Now check if the line that *was* typing had choices
            // This requires storing the current line temporarily
            // For simplicity now, we'll just let the next 'E' press handle choices/next line
            // OR, we can show choices immediately after skipping:
            // Find the line that was just skipped (might need to peek at queue or store it)
            // and call ShowChoices if it had any.
            // Let's stick to the simpler approach for now: skip typing, next E shows choices or next line.
            return; // Wait for next input to proceed after skipping
        }

        // If choices are displayed, don't advance the line
        if (currentChoiceButtons.Count > 0)
        {
            Debug.Log("DialogManager: Choices are visible, waiting for selection.");
            return;
        }

        if (remainingLines.Count == 0)
        {
            Debug.Log("DialogManager: No more lines remaining.");
            EndDialog();
            return;
        }

        // Get next line to display
        DialogLine currentDialogLine = remainingLines.Dequeue();
        Debug.Log($"<color=cyan>Displaying Line: '{currentDialogLine.Text}'</color>");

        // Set the character information
        if (currentDialogLine.Character != null)
        {
            portraitContainer.SetActive(true);
            portraitImage.sprite = currentDialogLine.Character.portraitSprite;
            characterNameText.text = currentDialogLine.Character.characterName;

            float portraitSize = currentDialogLine.Character.portraitSize > 0
                ? currentDialogLine.Character.portraitSize
                : defaultPortraitSize;

            Vector2 portraitOffset = currentDialogLine.Character.portraitOffset != Vector2.zero
                ? currentDialogLine.Character.portraitOffset
                : defaultPortraitOffset;

            SetPortraitSizeAndPosition(portraitSize, portraitOffset);
        }
        else
        {
            portraitContainer.SetActive(false);
        }

        // Clear previous choices *before* starting to type the new line
        ClearChoiceButtons();
        choicesContainer?.SetActive(false);

        // Start typing the text and pass choices to the coroutine
        typingCoroutine = StartCoroutine(TypeText(currentDialogLine.Text, currentDialogLine.Choices));

        // DO NOT show choices here anymore
        // DO NOT increment currentLine here (we use the queue)
    }

    private IEnumerator TypeText(string text, List<DialogChoice> choices)
    {
        isTyping = true;
        dialogText.text = ""; // Clear text first

        try
        {
            foreach (var letter in text.ToCharArray())
            {
                dialogText.text += letter;
                yield return new WaitForSeconds(1f / lettersPerSecond);
            }
        }
        finally // This block executes whether the coroutine finishes normally or is stopped
        {
            // Ensure full text is displayed if stopped early or finished normally
            dialogText.text = text;
            isTyping = false;
            typingCoroutine = null; // Clear the reference

            // Show choices if they exist, regardless of how the coroutine ended
            if (choices != null && choices.Count > 0)
            {
                Debug.Log($"<color=lime>Typing finished or skipped, showing choices.</color>");
                ShowChoices(choices);
            }
            else
            {
                 Debug.Log($"<color=gray>Typing finished or skipped, no choices for this line.</color>");
            }
        }
    }

    private void ClearChoiceButtons()
    {
        foreach (var btn in currentChoiceButtons)
        {
            if (btn != null) Destroy(btn);
        }
        currentChoiceButtons.Clear();
    }

    private void ShowChoices(List<DialogChoice> choices)
    {
        if (choicesContainer == null || choiceButtonPrefab == null) { /* Error Log */ return; }
        if (choices == null || choices.Count == 0) { /* Warning Log */ choicesContainer?.SetActive(false); return; }

        choicesContainer.SetActive(true);
        ClearChoiceButtons();

        // --- Configure Layout Group ---
        // Ensure the layout group doesn't force expansion if we want LayoutElement to control size
        var layoutGroup = choicesContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.enabled = true;
            // *** IMPORTANT: Uncheck these if you want buttons to truly size themselves ***
            layoutGroup.childControlWidth = false;  // Let LayoutElement control width
            layoutGroup.childControlHeight = false; // Let LayoutElement control height
            layoutGroup.childForceExpandWidth = false; // Don't force expand width
            layoutGroup.childForceExpandHeight = false; // Don't force expand height
            // Set alignment (e.g., MiddleCenter to center the potentially different-sized buttons)
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            // Adjust spacing/padding for the container itself if needed
            // layoutGroup.spacing = 10f;
            // layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        }
        // --- End Layout Group Config ---


        // Create buttons for each choice
        foreach (var choice in choices)
        {
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            buttonObj.SetActive(true);

            DialogChoiceButton choiceButtonScript = buttonObj.GetComponent<DialogChoiceButton>();
            if (choiceButtonScript != null)
            {
                DialogChoice choiceRef = choice; // Capture loop variable

                // --- Use the revised Setup method ---
                choiceButtonScript.Setup(choiceRef.Text, () => {
                    // --- Existing Click Logic ---
                    Debug.Log($"Choice selected: '{choiceRef.Text}'");
                    Dialog nextDialogToStart = choiceRef.NextDialog;
                    Quest questToGive = choiceRef.Quest;
                    choicesContainer?.SetActive(false);
                    ClearChoiceButtons();
                    if (questToGive != null && QuestManager.Instance != null) { /* Add Quest */ }
                    if (nextDialogToStart != null)
                    {
                        EndDialog(fireCompletionEvent: false);
                        StartDialog(nextDialogToStart);
                    }
                    else
                    {
                        EndDialog(fireCompletionEvent: true);
                    }
                    // --- End Click Logic ---
                });
                // --- End Setup call ---
            }
            else
            {
                 Debug.LogError("Choice Button Prefab is missing the DialogChoiceButton script!", buttonObj);
                 // Fallback (less ideal now)
                 TextMeshProUGUI tmp = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                 if (tmp != null) tmp.text = choice.Text;
                 Button btn = buttonObj.GetComponent<Button>();
                 if(btn != null) btn.onClick.AddListener(() => Debug.LogError("Button click failed - missing script"));
            }

            currentChoiceButtons.Add(buttonObj);
        }

        // Force the layout group to update its arrangement *after* all buttons are added and configured
        LayoutRebuilder.ForceRebuildLayoutImmediate(choicesContainer.GetComponent<RectTransform>());

        Debug.Log($"Displayed {currentChoiceButtons.Count} choices.");
    }

    private void OptimizeButtonText(TextMeshProUGUI textComponent, float maxWidth)
    {
        if (textComponent == null) return;

        SetWordWrapping(textComponent, false);

        textComponent.rectTransform.sizeDelta = new Vector2(0, textComponent.rectTransform.sizeDelta.y);
        textComponent.ForceMeshUpdate();
        float preferredWidth = textComponent.preferredWidth;

        if (preferredWidth > maxWidth)
        {
            SetWordWrapping(textComponent, true);
            textComponent.rectTransform.sizeDelta = new Vector2(maxWidth, textComponent.rectTransform.sizeDelta.y);

            RectTransform buttonRect = textComponent.transform.parent.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                buttonRect.sizeDelta = new Vector2(
                    maxWidth + 20f,
                    textComponent.preferredHeight + 20f
                );
            }
        }
    }
    private void SetWordWrapping(TMP_Text textComponent, bool enableWrapping)
    {
        var property = typeof(TMP_Text).GetProperty("textWrappingMode");

        if (property != null)
        {
            property.SetValue(textComponent, enableWrapping ? 1 : 0);
        }
        else
        {
            // Fall back to the older enableWordWrapping property with a warning suppression
#pragma warning disable CS0618 // Disable obsolete warning
            textComponent.enableWordWrapping = enableWrapping;
#pragma warning restore CS0618
        }
    }

    public void ForceCloseDialog()
    {
        if (IsDialogActive)
        {
            Debug.Log("Force closing dialog due to player walking away");
            EndDialog();
        }
    }

    private void EndDialog(bool fireCompletionEvent = true)
    {
        // Stop typing if it's happening
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            isTyping = false;
        }

        // Store reference before clearing
        Dialog completedDialog = this.currentDialog;
        this.currentDialog = null; // Clear current dialog reference

        // Clear buttons and hide containers
        ClearChoiceButtons();
        if (choicesContainer != null) choicesContainer.SetActive(false);
        if (portraitContainer != null) portraitContainer.SetActive(false);
        if (dialogBox != null) dialogBox.SetActive(false);

        Debug.Log("<color=yellow>EndDialog called - Dialog UI hidden.</color>");

        IsDialogActive = false; // Set inactive *before* enabling controls/firing events
        remainingLines.Clear(); // Clear any remaining lines

        // Re-enable camera/controls (existing logic)
        if (lockCameraDuringDialog && mainCamera != null)
        {
            var cameraComponents = mainCamera.GetComponents<MonoBehaviour>();
            foreach (var comp in cameraComponents)
            {
                string componentName = comp.GetType().Name;
                
                // Re-enable only specific camera control components
                bool shouldReEnable = false;
                
                if (cameraControlComponentNames.Length > 0)
                {
                    // Use the explicit list of components to re-enable
                    foreach (var name in cameraControlComponentNames)
                    {
                        if (componentName.Contains(name))
                        {
                            shouldReEnable = true;
                            break;
                        }
                    }
                }
                else
                {
                    // Fallback to generic naming detection for camera control
                    shouldReEnable = (componentName.Contains("Look") && componentName.Contains("Mouse")) ||
                                    (componentName.Contains("Look") && componentName.Contains("Player")) || 
                                    componentName.Contains("PlayerController");
                }
                
                if (shouldReEnable && !comp.enabled)
                {
                    comp.enabled = true;
                    Debug.Log($"<color=cyan>DialogManager: Re-enabled camera component: {componentName}</color>");
                }
            }
        }

        // Restore cursor and player movement
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerController?.SetCanMove(true);

        // Start cooldown
        StartCoroutine(DialogCooldown());

        // Fire events AFTER setting IsDialogActive to false and cleaning up
        OnHideDialog?.Invoke();

        // Only fire completion event if requested
        if (fireCompletionEvent && completedDialog != null)
        {
            Debug.Log($"<color=yellow>DialogManager: Firing OnDialogComplete event for dialog {completedDialog.name}</color>");
            OnDialogComplete?.Invoke(completedDialog);
        }
        else if (fireCompletionEvent && completedDialog == null)
        {
             Debug.Log("<color=orange>DialogManager: OnDialogComplete not fired, completedDialog was null.</color>");
        }
        else if (!fireCompletionEvent)
        {
            Debug.Log($"<color=grey>DialogManager: OnDialogComplete event skipped for intermediate dialog: {completedDialog?.name ?? "N/A"}</color>");
        }
    }

    private IEnumerator DialogCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownDuration);
        isOnCooldown = false;
    }

    public bool CanStartDialog()
    {
        return !isOnCooldown && !IsDialogActive;
    }

    public void HandleUpdate()
    {
        // Handle advancing the dialog
        if (Input.GetKeyDown(KeyCode.E)) // Or your interaction key
        {
            // Only advance if NOT typing and NO choices are shown
            if (!isTyping && currentChoiceButtons.Count == 0)
            {
                Debug.Log("DialogManager: E pressed, advancing to next line.");
                DisplayNextLine(); // Display the next line from the queue
            }
            // If choices are showing, pressing E does nothing here (handled by button clicks)
            // If typing is in progress, pressing E also does nothing now.
        }
    }

    private void SetPortraitSizeAndPosition(float size, Vector2 offset)
    {
        // Only adjust the portrait frame/image, not the container
        if (portraitFrame != null)
        {
            // Set the portrait image size
            portraitFrame.sizeDelta = new Vector2(size, size);
            portraitFrame.anchoredPosition = offset;

            // Ensure the portrait image is centered within its frame
            RectTransform imageRect = portraitImage.GetComponent<RectTransform>();
            if (imageRect != null && imageRect != portraitFrame)
            {
                imageRect.anchorMin = new Vector2(0.5f, 0.5f);
                imageRect.anchorMax = new Vector2(0.5f, 0.5f);
                imageRect.pivot = new Vector2(0.5f, 0.5f);
                imageRect.anchoredPosition = Vector2.zero;
                imageRect.sizeDelta = new Vector2(size, size);
            }
        }
    }

    public GameObject GetDialogBox()
    {
        return dialogBox;
    }
}