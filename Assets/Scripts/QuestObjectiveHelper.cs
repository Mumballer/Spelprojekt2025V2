using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestObjectiveHelper : MonoBehaviour
{
    [Header("Quest Settings")]
    [SerializeField] private Quest quest;
    [SerializeField] private int objectiveIndex;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private KeyCode interactionKey = KeyCode.F; // Use F instead of E
    [SerializeField] private bool completeOnInteract = true;
    [SerializeField] private bool completeAfterDialog = true;

    [Header("Visual Settings")]
    [SerializeField] private bool showPrompt = true;
    [SerializeField] private string promptText = "Press F to complete objective";
    [SerializeField] private float promptHeight = 2f;

    private bool hasBeenCompleted = false;
    private GameObject promptObject;
    private DialogTrigger dialogTrigger;

    private void Start()
    {
        dialogTrigger = GetComponent<DialogTrigger>();

        if (showPrompt)
        {
            CreatePrompt();
        }

        // Subscribe to DialogManager events
        if (completeAfterDialog && DialogManager.Instance != null)
        {
            DialogManager.Instance.OnHideDialog += CheckDialogEnded;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.OnHideDialog -= CheckDialogEnded;
        }
    }

    private void Update()
    {
        if (hasBeenCompleted) return;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool isInRange = distance <= interactionDistance;

        // Show prompt if in range
        if (promptObject != null)
        {
            promptObject.SetActive(isInRange && !hasBeenCompleted);

            // Make prompt face camera
            if (promptObject.activeSelf && Camera.main != null)
            {
                promptObject.transform.rotation = Camera.main.transform.rotation;
            }
        }

        // Check for keypress (only if not in dialog)
        if (completeOnInteract && isInRange && Input.GetKeyDown(interactionKey))
        {
            // Only allow F key to complete if we're not in dialog
            if (DialogManager.Instance == null || !DialogManager.Instance.IsDialogActive)
            {
                CompleteObjective();
            }
        }
    }

    private void CheckDialogEnded()
    {
        // Called when dialog ends (using existing OnHideDialog event)
        if (completeAfterDialog && !hasBeenCompleted)
        {
            CompleteObjective();
        }
    }

    private void CreatePrompt()
    {
        promptObject = new GameObject("QuestPrompt");
        promptObject.transform.SetParent(transform);
        promptObject.transform.localPosition = new Vector3(0, promptHeight, 0);

        // Add canvas
        Canvas canvas = promptObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // Add canvas scaler
        CanvasScaler scaler = promptObject.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;

        // Configure canvas transform
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(300, 100);
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        // Create background panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvas.transform);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.localPosition = Vector3.zero;
        panelRect.sizeDelta = Vector2.zero;

        // Create text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(panel.transform);

        TextMeshProUGUI promptTextComponent = textObj.AddComponent<TextMeshProUGUI>();
        promptTextComponent.text = promptText;
        promptTextComponent.fontSize = 36;
        promptTextComponent.alignment = TextAlignmentOptions.Center;
        promptTextComponent.color = Color.white;

        RectTransform textRect = promptTextComponent.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.localPosition = Vector3.zero;
        textRect.sizeDelta = Vector2.zero;

        // Initially hidden
        promptObject.SetActive(false);
    }

    public void CompleteObjective()
    {
        if (hasBeenCompleted) return;

        if (quest != null && QuestManager.Instance != null)
        {
            if (QuestManager.Instance.IsQuestActive(quest))
            {
                QuestManager.Instance.CompleteObjective(quest, objectiveIndex);
                Debug.Log($"Completed objective {objectiveIndex} for quest {quest.questName}");

                hasBeenCompleted = true;

                if (promptObject != null)
                {
                    promptObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning($"Quest {quest.questName} is not active yet!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}