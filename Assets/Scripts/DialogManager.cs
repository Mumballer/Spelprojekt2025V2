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
    [SerializeField] int lettersPerSecond = 30; // hur snabbt texten skrivs ut
    [SerializeField] GameObject choicesContainer; // där valknappar hamnar
    [SerializeField] GameObject choiceButtonPrefab; // mall för valknappar
    [SerializeField] private float cooldownDuration = 3f; // väntetid efter prat
    [SerializeField] private float maxButtonWidth = 350f; // inte för breda knappar

    [Header("Portrait System")]
    [SerializeField] private GameObject portraitContainer; // bildens ram
    [SerializeField] private Image portraitImage; // karaktärens bildruta
    [SerializeField] private TextMeshProUGUI characterNameText; // karaktärens namn
    [SerializeField] private RectTransform portraitFrame; // själva bildramen
    [SerializeField] private float defaultPortraitSize = 100f; // normal bildstorlek
    [SerializeField] private Vector2 defaultPortraitOffset = Vector2.zero; // normal bildposition

    [Header("3D Settings")]
    [SerializeField] private float interactionDistance = 3f; // hur nära för att prata
    [SerializeField] private LayerMask interactableLayers; // vilka saker går att prata med

    [Header("Camera and Input Settings")]
    [SerializeField] private bool lockCameraDuringDialog = true; // lås kameran vid prat
    [SerializeField] private bool allowCameraEffectsDuringDialog = true; // tillåt kamera skak osv
    [SerializeField] private bool showCursorDuringDialog = true; // visa muspekaren vid prat
    [SerializeField] private string[] cameraControlComponentNames = new string[] // namn på kamerakontroller att stänga av
    {
        "MouseLook",
        "FirstPersonLook",
        "PlayerLook",
        "LookController"
    };

    // signaler när dialog startar/slutar
    public event Action OnShowDialog;
    public event Action OnHideDialog;
    public event System.Action<Dialog> OnDialogComplete;
    public static DialogManager Instance { get; private set; } // enkel åtkomst till chefen

    private Dialog currentDialog; // dialogen som visas nu
    private int currentLine = 0; // inte använd längre, kö istället
    private bool isTyping; // skriver texten ut sig nu?
    private List<GameObject> currentChoiceButtons = new List<GameObject>(); // listan med valknappar
    private Coroutine typingCoroutine; // processen som skriver ut text
    private PlayerController playerController; // spelarens kontrollscript
    private bool isOnCooldown = false; // är chefen i paus?
    private Camera mainCamera; // huvudkameran

    public bool IsDialogActive { get; private set; } // är en dialog igång?

    private Queue<DialogLine> remainingLines = new Queue<DialogLine>(); // kö med repliker

    private void Awake()
    {
        Instance = this; // sätt chefen
        playerController = FindFirstObjectByType<PlayerController>(); // hitta spelaren
        mainCamera = Camera.main; // hitta kameran

        // göm alla ui-delar från start
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
            choiceButtonPrefab.SetActive(false); // göm knappmallen också
        }
    }

    private void Start()
    {
        // kolla om canvas är i 3d-läge
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.Log("dialog ui i 3d-läge");
        }
        SetupChoicesContainer(); // fixa layout för knappar
    }

    // fixa layouten för knappbehållaren
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
            // dessa rader styr knapparnas storlek
            layoutGroup.childControlWidth = true; // tas över av knapparna sen
            layoutGroup.childControlHeight = true; // tas över av knapparna sen
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
        }

        // fixa så storleken anpassas
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
            HandleUpdate(); // hantera input under dialog
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteractWithNPC(); // försök prata med nån
        }
    }

    // försök starta prat med npc
    private void TryInteractWithNPC()
    {
        if (!CanStartDialog() || mainCamera == null) return; // kolla om vi får prata

        // skicka en stråle från mitten
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayers))
        {
            DialogTrigger trigger = hit.collider.GetComponent<DialogTrigger>(); // kolla om det är en prat-trigger
            if (trigger != null)
            {
                Debug.Log($"hittade prat-trigger på {hit.collider.gameObject.name}");
                trigger.TriggerDialog(); // säg åt triggern att starta
            }
            else
            {
                Debug.Log($"ingen prat-trigger på {hit.collider.gameObject.name}");
            }
        }
        else
        {
            Debug.Log("strålen träffade inget pratbart");
        }
    }

    // starta en hel dialogsekvens
    public void StartDialog(Dialog dialogToShow)
    {
        if (dialogToShow == null || dialogToShow.Lines == null || dialogToShow.Lines.Count == 0)
        {
            Debug.LogError("kan inte starta tom dialog");
            return;
        }
        if (IsDialogActive)
        {
            Debug.LogWarning("försökte starta ny dialog för tidigt");
            return;
        }

        Debug.Log($"<color=yellow>startar dialog: {dialogToShow.name}</color>");
        currentDialog = dialogToShow;
        remainingLines.Clear(); // töm gamla repliker

        // lägg alla repliker i kön
        foreach (var line in currentDialog.Lines)
        {
            remainingLines.Enqueue(line);
        }

        // lås kamera etc
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

                // stäng bara av kontroller, inte effekter
                if (shouldDisable && !(allowCameraEffectsDuringDialog && (componentName.Contains("Shake") || componentName.Contains("Effect") || componentName.Contains("PostProcessing"))))
                {
                    comp.enabled = false;
                    Debug.Log($"<color=cyan>stängde av kamerakontroll: {componentName}</color>");
                }
            }
        }

        if (showCursorDuringDialog)
        {
            Cursor.lockState = CursorLockMode.None; // visa muspekare
            Cursor.visible = true;
        }

        playerController?.SetCanMove(false); // stoppa spelaren

        // visa ui och sätt status
        dialogBox.SetActive(true);
        IsDialogActive = true; // nu är dialogen aktiv!

        // städa gamla knappar/bilder
        ClearChoiceButtons();
        choicesContainer?.SetActive(false);
        portraitContainer?.SetActive(false);

        DisplayNextLine(); // visa första repliken

        OnShowDialog?.Invoke(); // signalera att dialog startat
    }

    // visa nästa replik från kön
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

    // skriv ut texten bokstav för bokstav
    private IEnumerator TypeText(string text, List<DialogChoice> choices)
    {
        isTyping = true;
        dialogText.text = ""; // börja tomt

        try
        {
            foreach (var letter in text.ToCharArray())
            {
                dialogText.text += letter;
                yield return new WaitForSeconds(1f / lettersPerSecond); // vänta lite
            }
        }
        finally // körs alltid, även om avbruten
        {
            // visa hela texten direkt
            dialogText.text = text;
            isTyping = false;
            typingCoroutine = null; // nollställ skrivprocessen

            // visa valknappar om det finns
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

    // ta bort gamla valknappar
    private void ClearChoiceButtons()
    {
        foreach (var btn in currentChoiceButtons)
        {
            if (btn != null) Destroy(btn); // förstör knappen
        }
        currentChoiceButtons.Clear(); // töm listan
    }

    // skapa och visa valknapparna
    private void ShowChoices(List<DialogChoice> choices)
    {
        if (choicesContainer == null || choiceButtonPrefab == null) { /* Error Log */ return; }
        if (choices == null || choices.Count == 0) { /* Warning Log */ choicesContainer?.SetActive(false); return; }

        choicesContainer.SetActive(true); // visa knappområdet
        ClearChoiceButtons(); // ta bort gamla först

        // fixa layouten så knapparna ser bra ut
        var layoutGroup = choicesContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.enabled = true;
            // låt knapparna bestämma sin egen storlek
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter; // centrera knapparna
        }

        // skapa en knapp för varje val
        foreach (var choice in choices)
        {
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer.transform); // skapa från mallen
            buttonObj.SetActive(true); // se till att den syns

            DialogChoiceButton choiceButtonScript = buttonObj.GetComponent<DialogChoiceButton>();
            if (choiceButtonScript != null)
            {
                DialogChoice choiceRef = choice; // kom ihåg rätt val för klicket

                // säg åt knappen att ställa in sig
                choiceButtonScript.Setup(choiceRef.Text, () => {
                    // detta händer när man klickar
                    Debug.Log($"Choice selected: '{choiceRef.Text}'");
                    Dialog nextDialogToStart = choiceRef.NextDialog;
                    Quest questToGive = choiceRef.Quest;

                    // göm och ta bort knappar
                    choicesContainer?.SetActive(false);
                    ClearChoiceButtons();

                    // ge quest om det finns
                    if (questToGive != null && QuestManager.Instance != null)
                    {
                        Debug.Log($"Adding quest '{questToGive.questName}' from choice.");
                        QuestManager.Instance.AddQuest(questToGive);
                    }

                    // starta nästa dialog eller avsluta
                    if (nextDialogToStart != null)
                    {
                        Debug.Log($"val leder till ny dialog: {nextDialogToStart.name}");
                        EndDialog(fireCompletionEvent: false); // avsluta nuvarande tyst
                        StartDialog(nextDialogToStart); // starta nästa
                    }
                    else
                    {
                        Debug.Log("val avslutar samtalet.");
                        EndDialog(fireCompletionEvent: true); // avsluta på riktigt
                    }
                });
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

            currentChoiceButtons.Add(buttonObj); // lägg till i listan
        }

        // tvinga layouten att uppdateras direkt
        LayoutRebuilder.ForceRebuildLayoutImmediate(choicesContainer.GetComponent<RectTransform>());

        Debug.Log($"Displayed {currentChoiceButtons.Count} choices.");
    }

    // onödig nu? beror på button script
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

    // onödig nu? beror på button script
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

    // stäng dialogen utifrån
    public void ForceCloseDialog()
    {
        if (IsDialogActive)
        {
            Debug.Log("stänger dialog (spelaren gick iväg?)");
            EndDialog(); // avsluta normalt
        }
    }

    // avsluta hela dialogsekvensen
    private void EndDialog(bool fireCompletionEvent = true)
    {
        // stoppa skrivandet om det pågår
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            isTyping = false;
        }

        Dialog completedDialog = this.currentDialog; // spara vilken som avslutades
        this.currentDialog = null; // glöm nuvarande dialog

        // städa upp ui
        ClearChoiceButtons();
        if (choicesContainer != null) choicesContainer.SetActive(false);
        if (portraitContainer != null) portraitContainer.SetActive(false);
        if (dialogBox != null) dialogBox.SetActive(false);

        Debug.Log("<color=yellow>EndDialog called - Dialog UI hidden.</color>");

        IsDialogActive = false; // dialogen är inte aktiv längre
        remainingLines.Clear(); // töm replikkön

        // återställ kamera/kontroller
        if (lockCameraDuringDialog && mainCamera != null)
        {
            var cameraComponents = mainCamera.GetComponents<MonoBehaviour>();
            foreach (var comp in cameraComponents)
            {
                string componentName = comp.GetType().Name;
                bool shouldReEnable = false;
                if (cameraControlComponentNames.Length > 0)
                {
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

        // återställ mus och spelare
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerController?.SetCanMove(true);

        StartCoroutine(DialogCooldown()); // starta kort paus

        // signalera att dialogen är gömd
        OnHideDialog?.Invoke();

        // signalera att dialogen är KLAR (om det var slutet)
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

    // kort paus efter dialogen slutat
    private IEnumerator DialogCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownDuration);
        isOnCooldown = false;
    }

    // får vi starta en ny dialog?
    public bool CanStartDialog()
    {
        return !isOnCooldown && !IsDialogActive; // inte paus och inte redan igång
    }

    // hantera input medan dialog är aktiv
    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.E)) // om man trycker E
        {
            // gör inget om text skrivs ut
            // gör inget om val visas
            if (!isTyping && currentChoiceButtons.Count == 0)
            {
                Debug.Log("DialogManager: E pressed, advancing to next line.");
                DisplayNextLine(); // visa nästa
            }
        }
    }

    // ställ in storlek och position på porträttbilden
    private void SetPortraitSizeAndPosition(float size, Vector2 offset)
    {
        if (portraitFrame != null)
        {
            portraitFrame.sizeDelta = new Vector2(size, size);
            portraitFrame.anchoredPosition = offset;

            // se till att bilden fyller ramen
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

    // ge andra script tillgång till boxen
    public GameObject GetDialogBox()
    {
        return dialogBox;
    }
}