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

    private void Awake()
    {
        // skapar singleton-instans
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        mainCamera = Camera.main;

        if (dialogBox != null)
        {
            dialogBox.SetActive(false);
        }

        if (choicesContainer != null)
        {
            choicesContainer.SetActive(false);
        }
    }

    public bool CanStartDialog()
    {
        return !isOnCooldown && (dialogBox == null || !dialogBox.activeSelf);
    }

    public bool IsDialogActive => dialogBox != null && dialogBox.activeSelf;

    private void StartDialog(Dialog newDialog)
    {
        // startar ny dialog
        if (isOnCooldown)
        {
            Debug.Log("Dialog is on cooldown, cannot start new dialog");
            return;
        }

        if (dialogBox == null || newDialog == null || newDialog.Lines.Count == 0)
        {
            Debug.LogError("Dialog box or dialog is null, or dialog has no lines");
            return;
        }

        dialog = newDialog;
        currentLine = 0;
        dialogBox.SetActive(true);
        
        OnShowDialog?.Invoke();
        
        LockCamera();
        ShowPortrait(dialog.Lines[0].Character);
        
        typingCoroutine = StartCoroutine(TypeText(dialog.Lines[0].Text));
    }

    private void LockCamera()
    {
        // låser kameran under dialog
        if (lockCameraDuringDialog && mainCamera != null)
        {
            var cameraComponents = mainCamera.GetComponents<MonoBehaviour>();
            foreach (var comp in cameraComponents)
            {
                string componentName = comp.GetType().Name;
                
                bool shouldDisable = false;
                
                if (cameraControlComponentNames.Length > 0)
                {
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
                    shouldDisable = (componentName.Contains("Look") && componentName.Contains("Mouse")) ||
                                   (componentName.Contains("Look") && componentName.Contains("Player")) || 
                                   componentName.Contains("PlayerController");
                }
                
                if (shouldDisable && 
                    !(allowCameraEffectsDuringDialog && 
                      (componentName.Contains("Shake") || 
                       componentName.Contains("Effect") || 
                       componentName.Contains("PostProcessing"))))
                {
                    comp.enabled = false;
                }
            }
        }
        
        if (showCursorDuringDialog)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        playerController?.SetCanMove(false);
    }

    private void UnlockCamera()
    {
        // återaktiverar kameran
        if (lockCameraDuringDialog && mainCamera != null)
        {
            var cameraComponents = mainCamera.GetComponents<MonoBehaviour>();
            foreach (var comp in cameraComponents)
            {
                string componentName = comp.GetType().Name;
                
                bool shouldEnable = false;
                
                if (cameraControlComponentNames.Length > 0)
                {
                    foreach (var name in cameraControlComponentNames)
                    {
                        if (componentName.Contains(name))
                        {
                            shouldEnable = true;
                            break;
                        }
                    }
                }
                else
                {
                    shouldEnable = (componentName.Contains("Look") && componentName.Contains("Mouse")) ||
                                  (componentName.Contains("Look") && componentName.Contains("Player")) || 
                                  componentName.Contains("PlayerController");
                }
                
                if (shouldEnable && !comp.enabled)
                {
                    comp.enabled = true;
                }
            }
        }
        
        if (showCursorDuringDialog)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        playerController?.SetCanMove(true);
    }

    private void EndDialog()
    {
        // avslutar dialogen
        if (dialogBox != null)
        {
            dialogBox.SetActive(false);
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        ClearChoices();
        OnHideDialog?.Invoke();
        OnDialogComplete?.Invoke(dialog);
        UnlockCamera();

        StartCoroutine(StartCooldown());
    }

    private IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownDuration);
        isOnCooldown = false;
    }

    private IEnumerator TypeText(string text)
    {
        // skriver ut text gradvis
        isTyping = true;
        dialogText.text = "";

        foreach (char c in text.ToCharArray())
        {
            dialogText.text += c;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        isTyping = false;
        
        DialogLine currentDialogLine = dialog.Lines[currentLine];
        
        if (currentDialogLine.HasChoices)
        {
            ShowChoices(currentDialogLine.Choices);
        }
    }

    private void ShowPortrait(DialogCharacter character)
    {
        if (portraitContainer == null || portraitImage == null || characterNameText == null)
            return;

        if (character == null)
        {
            portraitContainer.SetActive(false);
            return;
        }

        portraitContainer.SetActive(true);
        portraitImage.sprite = character.portraitSprite;
        characterNameText.text = character.characterName;

        SetPortraitSizeAndPosition(
            character.portraitSize > 0 ? character.portraitSize : defaultPortraitSize,
            character.portraitOffset != Vector2.zero ? character.portraitOffset : defaultPortraitOffset
        );
    }

    private void ShowChoices(List<DialogChoice> choices)
    {
        // visar dialogval
        if (choicesContainer == null || choiceButtonPrefab == null)
            return;

        ClearChoices();
        
        choicesContainer.SetActive(true);

        foreach (var choice in choices)
        {
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            DialogChoiceButton button = buttonObj.GetComponent<DialogChoiceButton>();
            
            if (button != null)
            {
                button.SetText(choice.Text);
            }
            
            Button uiButton = buttonObj.GetComponent<Button>();
            if (uiButton != null)
            {
                DialogChoice capturedChoice = choice;
                uiButton.onClick.AddListener(() => HandleChoiceSelected(capturedChoice));
            }
            
            currentChoiceButtons.Add(buttonObj);
        }
    }

    private void ClearChoices()
    {
        if (choicesContainer != null)
        {
            choicesContainer.SetActive(false);
        }

        foreach (var button in currentChoiceButtons)
        {
            Destroy(button);
        }
        
        currentChoiceButtons.Clear();
    }

    private void HandleChoiceSelected(DialogChoice choice)
    {
        // hanterar val i dialog
        ClearChoices();
        
        if (choice.Quest != null && QuestManager.Instance != null)
        {
            Debug.Log($"Selected choice with quest: {choice.Quest.questName}");
            QuestManager.Instance.AddQuest(choice.Quest);
        }
        
        if (choice.NextDialog != null)
        {
            dialog = choice.NextDialog;
            currentLine = 0;
            ShowPortrait(dialog.Lines[currentLine].Character);
            typingCoroutine = StartCoroutine(TypeText(dialog.Lines[currentLine].Text));
        }
        else
        {
            DialogLine currentDialogLine = dialog.Lines[currentLine];
            if (currentDialogLine.NextDialog != null)
            {
                dialog = currentDialogLine.NextDialog;
                currentLine = 0;
                ShowPortrait(dialog.Lines[currentLine].Character);
                typingCoroutine = StartCoroutine(TypeText(dialog.Lines[currentLine].Text));
            }
            else if (currentLine < dialog.Lines.Count - 1)
            {
                currentLine++;
                ShowPortrait(dialog.Lines[currentLine].Character);
                typingCoroutine = StartCoroutine(TypeText(dialog.Lines[currentLine].Text));
            }
            else
            {
                EndDialog();
            }
        }
    }

    private void Update()
    {
        if (!IsDialogActive) return;

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
        if (portraitFrame != null)
        {
            portraitFrame.sizeDelta = new Vector2(size, size);
            portraitFrame.anchoredPosition = offset;

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