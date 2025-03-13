using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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

    private Dialog dialog;
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

    public void StartDialog(Dialog dialog)
    {
        if (dialog == null || dialog.Lines.Count == 0) return;
        
        this.dialog = dialog;
        currentLine = 0;
        remainingLines.Clear();
        
        // Queue up all lines in the dialog
        foreach (var line in dialog.Lines)
        {
            remainingLines.Enqueue(line);
        }
        
        // Lock camera control but allow camera effects
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
        
        // Prevent player movement
        playerController?.SetCanMove(false);
        
        // Show dialog panel
        dialogBox.SetActive(true);
        IsDialogActive = true;
        
        // Start showing the first line
        DisplayNextLine();
        
        OnShowDialog?.Invoke();
    }

    public void DisplayNextLine()
    {
        if (remainingLines.Count == 0)
        {
            EndDialog();
            return;
        }
        
        // Get next line to display
        DialogLine currentDialogLine = remainingLines.Dequeue();
        
        // Set the character information
        if (currentDialogLine.Character != null)
        {
            portraitContainer.SetActive(true);
            portraitImage.sprite = currentDialogLine.Character.portraitSprite;
            characterNameText.text = currentDialogLine.Character.characterName;

            // Set custom portrait size and position if specified
            float portraitSize = currentDialogLine.Character.portraitSize > 0 
                ? currentDialogLine.Character.portraitSize 
                : defaultPortraitSize;
                
            Vector2 portraitOffset = currentDialogLine.Character.portraitOffset != Vector2.zero
                ? currentDialogLine.Character.portraitOffset
                : defaultPortraitOffset;

            SetPortraitSizeAndPosition(portraitSize, portraitOffset);
            
            Debug.Log($"Setting portrait for {currentDialogLine.Character.characterName}: Size={portraitSize}, Offset={portraitOffset}");
        }
        else
        {
            portraitContainer.SetActive(false);
        }
        
        // Display the text
        typingCoroutine = StartCoroutine(TypeText(currentDialogLine.Text));
        
        // Setup choices if any
        if (currentDialogLine.HasChoices && currentDialogLine.Choices.Count > 0)
        {
            ShowChoices(currentDialogLine.Choices);
        }
        else if (currentDialogLine.NextDialog != null)
        {
            // Auto-proceed to next dialog functionality
        }
        
        currentLine++; // This should be an integer already
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogText.text = "";
        
        foreach (var letter in text.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        
        isTyping = false;

        if (currentLine < dialog.Lines.Count - 1)
        {
            DisplayNextLine();
        }
    }

    private void ShowChoices(List<DialogChoice> choices)
    {
        if (choices == null || choices.Count == 0) return;
        
        // Clear previous choices
        foreach (var btn in currentChoiceButtons)
        {
            if (btn != null) Destroy(btn);
        }
        currentChoiceButtons.Clear();
        
        // Show the choices container
        choicesContainer.SetActive(true);
        
        // Modify the layout group to prevent tiny buttons
        VerticalLayoutGroup layoutGroup = choicesContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.childControlWidth = false;  // Don't let layout control width
            layoutGroup.childControlHeight = false; // Don't let layout control height
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = 10f;
        }
        
        // Create buttons for each choice
        foreach (var choice in choices)
        {
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            buttonObj.SetActive(true);
            
            // Define textRect at this scope level so we can access it later
            RectTransform buttonRect = null;
            RectTransform textRect = null;
            
            // Ensure the button has a fixed size
            buttonRect = buttonObj.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                // Set a large explicit size for the button
                buttonRect.sizeDelta = new Vector2(300f, 60f);
            }
            
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = choice.Text;
                buttonText.fontSize = 18;  // Explicit font size
                
                // Fix text component size
                textRect = buttonText.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    // Make text fill most of the button
                    textRect.sizeDelta = new Vector2(280f, 50f);
                    textRect.anchorMin = new Vector2(0.5f, 0.5f);
                    textRect.anchorMax = new Vector2(0.5f, 0.5f);
                    textRect.pivot = new Vector2(0.5f, 0.5f);
                    textRect.anchoredPosition = Vector2.zero;
                }
                
                // Set text display properties
                buttonText.enableWordWrapping = true;
                buttonText.horizontalAlignment = HorizontalAlignmentOptions.Center;
                buttonText.verticalAlignment = VerticalAlignmentOptions.Middle;
                buttonText.textWrappingMode = TextWrappingModes.Normal;
            }
            
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                // Make sure button has colors that are clearly visible
                ColorBlock colors = button.colors;
                colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                colors.highlightedColor = new Color(1f, 1f, 0.8f, 1f);
                button.colors = colors;
                
                // Store the choice for this button
                DialogChoice choiceRef = choice;
                
                button.onClick.AddListener(() => {
                    // Start any quest associated with this choice
                    if (choiceRef.Quest != null && QuestManager.Instance != null)
                    {
                        QuestManager.Instance.AddQuest(choiceRef.Quest);
                    }
                    
                    // Go to next dialog if specified
                    if (choiceRef.NextDialog != null)
                    {
                        StartCoroutine(CleanupAndContinueDialog(choiceRef.NextDialog));
                    }
                    else
                    {
                        EndDialog();
                    }
                });
            }
            
            currentChoiceButtons.Add(buttonObj);

            // Log the sizes - now this will work properly
            Debug.Log($"Button size: {(buttonRect != null ? buttonRect.sizeDelta.ToString() : "null")}");
            Debug.Log($"Text size: {(textRect != null ? textRect.sizeDelta.ToString() : "null")}");
        }
        
        // Force layout rebuild
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(choicesContainer.GetComponent<RectTransform>());
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

    private IEnumerator CleanupAndContinueDialog(Dialog nextDialog)
    {
        yield return new WaitForEndOfFrame();

        foreach (var btn in new List<GameObject>(currentChoiceButtons))
        {
            if (btn != null)
            {
                Destroy(btn);
            }
        }
        currentChoiceButtons.Clear();

        if (choicesContainer != null)
        {
            choicesContainer.SetActive(false);
        }

        if (nextDialog != null)
        {
            Debug.Log("Starting next dialog sequence");
            StartDialog(nextDialog);
        }
        else
        {
            Debug.Log("No next dialog, ending conversation");
            EndDialog();
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

    private void EndDialog()
    {
        // Store reference to the current dialog before clearing it
        Dialog completedDialog = this.dialog;

        foreach (var button in currentChoiceButtons)
        {
            if (button != null)
            {
                Destroy(button);
            }
        }
        currentChoiceButtons.Clear();

        Debug.Log("EndDialog called - Forcing dialog box to close");
        if (dialogBox != null)
        {
            dialogBox.SetActive(false);
            choicesContainer.SetActive(false);
            portraitContainer.SetActive(false);
            Debug.Log($"Dialog box set to inactive. Active state: {dialogBox.activeSelf}");
        }

        IsDialogActive = false;
        currentLine = 0;

        // Make sure to re-enable camera if it was disabled
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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerController?.SetCanMove(true);

        StartCoroutine(DialogCooldown());

        // Fire events AFTER setting IsDialogActive to false
        OnHideDialog?.Invoke();

        if (completedDialog != null)
        {
            Debug.Log($"<color=yellow>DialogManager: Firing OnDialogComplete event for dialog {completedDialog.name}</color>");
            OnDialogComplete?.Invoke(completedDialog);
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
        if (Input.GetKeyDown(KeyCode.E) && !isTyping)
        {
            if (currentChoiceButtons.Count > 0)
            {
                return;
            }

            Debug.Log($"E pressed. Current line: {currentLine}, Total lines: {dialog.Lines.Count}");

            if (currentLine >= dialog.Lines.Count - 1)
            {
                Debug.Log("On last line, ending dialog");
                EndDialog();
                return;
            }

            currentLine++;
            Debug.Log($"Moving to line {currentLine}");
            typingCoroutine = StartCoroutine(TypeText(dialog.Lines[currentLine].Text));
        }
    }

    public IEnumerator ShowDialog(Dialog dialog)
    {
        StartDialog(dialog);
        yield break;
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