using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogManager : MonoBehaviour
{
    [Header("Dialog UI")]
    // dialogruta i ui
    [SerializeField] GameObject dialogBox;
    // textkomponent för dialog
    [SerializeField] TextMeshProUGUI dialogText;
    // hastighet för textvisning
    [SerializeField] int lettersPerSecond = 30;
    // container för valalternativ
    [SerializeField] GameObject choicesContainer;
    // prefab för valalternativ
    [SerializeField] GameObject choiceButtonPrefab;
    // tid mellan dialoger
    [SerializeField] private float cooldownDuration = 5f;
    // maxbredd för knappar
    [SerializeField] private float maxButtonWidth = 350f;

    [Header("Portrait System")]
    // container för porträtt
    [SerializeField] private GameObject portraitContainer;
    // bildkomponent för porträtt
    [SerializeField] private Image portraitImage;
    // text för karaktärsnamn
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private RectTransform portraitFrame;
    [SerializeField] private float defaultPortraitSize = 100f;
    [SerializeField] private Vector2 defaultPortraitOffset = Vector2.zero;

    [Header("3D Settings")]
    // interaktionsavstånd för npc
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactableLayers;

    public event Action OnShowDialog;
    public event Action OnHideDialog;
    public event Action<Dialog> OnDialogComplete;
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

    private void Awake()
    {
        // sätt singletonen
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
        // skapar layout för val
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

        // kastar raycasting för interaktion
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

    public IEnumerator ShowDialog(Dialog dialog)
    {
        if (isOnCooldown || dialog == null || dialog.Lines == null || dialog.Lines.Count == 0)
        {
            Debug.Log(isOnCooldown ? "Dialog on cooldown" : "Invalid dialog data");
            yield break;
        }

        yield return new WaitForEndOfFrame();

        // aktiverar händelser
        OnShowDialog?.Invoke();
        IsDialogActive = true;

        playerController?.SetCanMove(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        this.dialog = dialog;
        currentLine = 0;
        dialogBox.SetActive(true);
        Debug.Log($"Starting dialog with {dialog.Lines.Count} lines");

        typingCoroutine = StartCoroutine(TypeDialog(dialog.Lines[0]));
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

            // avsluta dialog om sista raden
            if (currentLine >= dialog.Lines.Count - 1)
            {
                Debug.Log("On last line, ending dialog");
                EndDialog();
                return;
            }

            // gå till nästa rad
            currentLine++;
            Debug.Log($"Moving to line {currentLine}");
            typingCoroutine = StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
        }
    }

    private IEnumerator TypeDialog(DialogLine dialogLine)
    {
        if (dialogLine == null)
        {
            Debug.LogError("Dialog line is null");
            yield break;
        }

        isTyping = true;
        dialogText.text = "";

        if (dialogLine.Character != null)
        {
            // visa porträtt
            portraitContainer.SetActive(true);
            portraitImage.sprite = dialogLine.Character.portraitSprite;
            characterNameText.text = dialogLine.Character.characterName;

            // justera porträttbilden
            if (portraitFrame != null)
            {
                float size = dialogLine.Character.portraitSize > 0 ? dialogLine.Character.portraitSize : defaultPortraitSize;
                Vector2 offset = dialogLine.Character.portraitOffset != Vector2.zero ? dialogLine.Character.portraitOffset : defaultPortraitOffset;

                // storlek för porträtt
                portraitFrame.sizeDelta = new Vector2(size, size);
                portraitFrame.anchoredPosition = offset;

                // centrera bilden i ramen
                RectTransform imageRect = portraitImage.GetComponent<RectTransform>();
                if (imageRect != null && imageRect != portraitFrame)
                {
                    imageRect.anchorMin = new Vector2(0.5f, 0.5f);
                    imageRect.anchorMax = new Vector2(0.5f, 0.5f);
                    imageRect.pivot = new Vector2(0.5f, 0.5f);
                    imageRect.anchoredPosition = Vector2.zero;
                    imageRect.sizeDelta = new Vector2(size, size);
                }

                Debug.Log($"Setting portrait for {dialogLine.Character.characterName}: Size={size}, Offset={offset}");
            }
        }
        else
        {
            portraitContainer.SetActive(false);
        }

        // bokstav för bokstav animation
        foreach (var letter in dialogLine.Text.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        isTyping = false;

        if (dialogLine.HasChoices)
        {
            ShowChoices(dialogLine.Choices);
        }
    }

    private void ShowChoices(List<DialogChoice> choices)
    {
        if (choices == null || choices.Count == 0 || choiceButtonPrefab == null)
        {
            Debug.LogError("Missing required components for showing choices");
            return;
        }
        foreach (var btn in new List<GameObject>(currentChoiceButtons))
        {
            if (btn != null && btn != choiceButtonPrefab)
            {
                Destroy(btn);
            }
        }
        currentChoiceButtons.Clear();

        // visar valalternativ
        choicesContainer.SetActive(true);
        choiceButtonPrefab.SetActive(false);

        for (int i = 0; i < choices.Count; i++)
        {
            DialogChoice choice = choices[i];
            if (choice == null) continue;

            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            buttonObj.name = $"ChoiceButton_{i}";
            buttonObj.SetActive(true);


            DialogChoiceButton choiceButton = buttonObj.GetComponent<DialogChoiceButton>();
            if (choiceButton != null)
            {
                choiceButton.SetText(choice.Text);
            }
            else
            {
                // manuell konfiguration om knapp saknas
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = choice.Text;

                    buttonText.alignment = TextAlignmentOptions.Center;
                    OptimizeButtonText(buttonText, maxButtonWidth);
                    RectTransform textRectTransform = buttonText.GetComponent<RectTransform>();
                    if (textRectTransform != null)
                    {
                        textRectTransform.anchorMin = new Vector2(0, 0);
                        textRectTransform.anchorMax = new Vector2(1, 1);
                        textRectTransform.pivot = new Vector2(0.5f, 0.5f);
                        textRectTransform.offsetMin = new Vector2(10, 5);
                        textRectTransform.offsetMax = new Vector2(-10, -5);
                    }
                }

                LayoutElement layoutElement = buttonObj.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = buttonObj.AddComponent<LayoutElement>();
                    layoutElement.minWidth = 160f;
                    layoutElement.minHeight = 50f;
                }

                ContentSizeFitter buttonFitter = buttonObj.GetComponent<ContentSizeFitter>();
                if (buttonFitter == null)
                {
                    buttonFitter = buttonObj.AddComponent<ContentSizeFitter>();
                    buttonFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    buttonFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                }
            }

            // koppla knapp till val
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                DialogChoice currentChoice = choice;
                button.onClick.AddListener(() => {
                    if (currentChoice.Quest != null)
                    {
                        QuestManager.Instance?.AddQuest(currentChoice.Quest);
                    }
                    StartCoroutine(CleanupAndContinueDialog(currentChoice.NextDialog));
                });
            }

            currentChoiceButtons.Add(buttonObj);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(choicesContainer.GetComponent<RectTransform>());
    }

    private void OptimizeButtonText(TextMeshProUGUI textComponent, float maxWidth)
    {
        if (textComponent == null) return;

        SetWordWrapping(textComponent, false);

        textComponent.rectTransform.sizeDelta = new Vector2(0, textComponent.rectTransform.sizeDelta.y);
        textComponent.ForceMeshUpdate();
        float preferredWidth = textComponent.preferredWidth;

        // aktivera textbrytning om för bred
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
#pragma warning disable CS0618
            textComponent.enableWordWrapping = enableWrapping;
#pragma warning restore CS0618
        }
    }

    private IEnumerator CleanupAndContinueDialog(Dialog nextDialog)
    {
        yield return new WaitForEndOfFrame();

        // ta bort alla knappval
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
            StartCoroutine(ShowDialog(nextDialog));
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
        // sparar referens till dialog
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

        // återställ mus
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerController?.SetCanMove(true);

        StartCoroutine(DialogCooldown());

        // aktivera händelser
        OnHideDialog?.Invoke();

        if (completedDialog != null)
        {
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
}