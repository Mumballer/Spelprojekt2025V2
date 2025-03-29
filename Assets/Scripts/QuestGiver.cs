using UnityEngine;
using System.Collections;

public class QuestGiver : MonoBehaviour
{
    [Header("Quest Settings")]
    [SerializeField] private Quest questToGive;
    [SerializeField] private bool startQuestOnTriggerEnter = false;

    [Header("Dialog Integration")]
    [SerializeField] private Dialog initialDialog;          // Dialog before quest is offered/known.
    [SerializeField] private Dialog questOfferDialog;       // Dialog specifically offering the quest (optional, can use initialDialog).
    [SerializeField] private Dialog postAcceptanceDialog;   // Dialog shown ONCE immediately after accepting.
    [SerializeField] private Dialog activeQuestDialog;      // Dialog shown while the quest is active but not complete.
    [SerializeField] private Dialog completedQuestDialog;   // Dialog shown after the quest is completed.
    [SerializeField] private bool useDialogForQuest = true; // Use dialog choices to offer quest

    [Header("Interaction Settings")]
    [SerializeField] private bool requireButtonPress = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private GameObject interactionPrompt;

    private bool playerInRange = false;
    private bool questOfferedOrGiven = false; // Renamed from questGiven for clarity
    private bool shownPostAcceptanceDialog = false; // Flag to track if the post-acceptance dialog was shown

    private DialogTrigger dialogTrigger;

    private void Start()
    {
        // If we're using dialog for quest, set up a DialogTrigger component
        if (useDialogForQuest) // Check if dialog system should be used
        {
            dialogTrigger = GetComponent<DialogTrigger>();
            if (dialogTrigger == null)
            {
                dialogTrigger = gameObject.AddComponent<DialogTrigger>();
            }

            // Set initial dialog based on current state
            dialogTrigger.dialog = GetAppropriateDialog();
            dialogTrigger.triggerDistance = interactionDistance;
            dialogTrigger.interactionPrompt = interactionPrompt;

            // Subscribe to dialog completion events ONLY if using dialog system
            if (DialogManager.Instance != null)
            {
                DialogManager.Instance.OnDialogComplete += HandleDialogComplete;
            }
        }
        else if (initialDialog != null) // Handle non-quest-related initial dialog even if not using dialog for quest giving
        {
             dialogTrigger = GetComponent<DialogTrigger>();
            if (dialogTrigger == null)
            {
                dialogTrigger = gameObject.AddComponent<DialogTrigger>();
            }
             dialogTrigger.dialog = initialDialog; // Just use the initial one
             dialogTrigger.triggerDistance = interactionDistance;
             dialogTrigger.interactionPrompt = interactionPrompt;
             // No need to subscribe to OnDialogComplete if not handling quest acceptance via dialog
        }


        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // Check initial quest state to set flags correctly on load/start
        if (questToGive != null && QuestManager.Instance != null)
        {
            if (QuestManager.Instance.IsQuestActive(questToGive) || QuestManager.Instance.IsQuestCompleted(questToGive))
            {
                questOfferedOrGiven = true;
                // If it's active or completed, we assume the post-acceptance phase is passed
                shownPostAcceptanceDialog = true;
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events ONLY if using dialog system for quest
        if (useDialogForQuest && DialogManager.Instance != null)
        {
            DialogManager.Instance.OnDialogComplete -= HandleDialogComplete;
        }
    }

    // Handle dialog completion events (primarily to know when offer/acceptance dialogs finish)
    private void HandleDialogComplete(Dialog completedDialog)
    {
        // If the dialog that just finished was the one offering the quest,
        // OR the one shown immediately after acceptance, update the state.
        if (completedDialog == questOfferDialog || completedDialog == postAcceptanceDialog)
        {
             // If the quest is now active, ensure the post-acceptance flag is set.
             if (questToGive != null && QuestManager.Instance != null && QuestManager.Instance.IsQuestActive(questToGive))
             {
                 questOfferedOrGiven = true; // Mark as given
                 shownPostAcceptanceDialog = true; // Mark post-acceptance dialog as conceptually "shown"
                 Debug.Log($"QuestGiver: Dialog '{completedDialog.name}' completed. Quest '{questToGive.questName}' is active. Post-acceptance phase passed.");
             }
        }
        // Potentially add logic here if other dialogs trigger state changes
    }

    // Get the appropriate dialog based on quest state
    private Dialog GetAppropriateDialog()
    {
        if (questToGive == null || QuestManager.Instance == null)
        {
            Debug.Log("QuestGiver: No quest assigned or QuestManager not found, returning initial dialog.");
            return initialDialog; // Return initial if no quest logic applies
        }

        // 1. Check if quest is completed
        if (QuestManager.Instance.IsQuestCompleted(questToGive))
        {
            Debug.Log($"QuestGiver: Quest '{questToGive.questName}' is completed. Returning completed dialog.");
            return completedQuestDialog ?? initialDialog; // Use completed or fallback to initial
        }

        // 2. Check if quest is active
        if (QuestManager.Instance.IsQuestActive(questToGive))
        {
            // 2a. Check if we need to show the post-acceptance dialog (only once)
            if (questOfferedOrGiven && !shownPostAcceptanceDialog && postAcceptanceDialog != null)
            {
                 Debug.Log($"QuestGiver: Quest '{questToGive.questName}' was just accepted. Returning post-acceptance dialog.");
                 // We don't set shownPostAcceptanceDialog = true here yet.
                 // It gets set when this specific dialog completes (in HandleDialogComplete)
                 // or when the player interacts again after seeing it once.
                 return postAcceptanceDialog;
            }
            // 2b. Otherwise, show the standard active quest dialog
            else
            {
                Debug.Log($"QuestGiver: Quest '{questToGive.questName}' is active. Returning active dialog.");
                // If we got here, the post-acceptance phase is definitely over.
                shownPostAcceptanceDialog = true;
                return activeQuestDialog ?? initialDialog; // Use active or fallback to initial
            }
        }

        // 3. If not completed and not active, check if it has been offered/given before
        //    (This state usually means the player declined or hasn't accepted yet)
        if (questOfferedOrGiven)
        {
             // If it was offered but is not active/completed, maybe show the offer again or a reminder?
             // For simplicity, let's fall back to the offer dialog or initial.
             Debug.Log($"QuestGiver: Quest '{questToGive.questName}' was offered but isn't active/completed. Returning offer/initial dialog.");
             return questOfferDialog ?? initialDialog;
        }

        // 4. If none of the above, it means the quest hasn't been offered yet.
        Debug.Log($"QuestGiver: Quest '{questToGive.questName}' not offered yet. Returning offer/initial dialog.");
        return questOfferDialog ?? initialDialog; // Use offer dialog or fallback to initial
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;

            // Update the dialog trigger ONLY if using dialog system
            if (useDialogForQuest && dialogTrigger != null)
            {
                 dialogTrigger.dialog = GetAppropriateDialog(); // Ensure dialog is up-to-date
            }

            if (interactionPrompt != null && !DialogManager.Instance.IsDialogActive)
            {
                interactionPrompt.SetActive(true);
            }

            if (startQuestOnTriggerEnter && !questOfferedOrGiven && !useDialogForQuest)
            {
                GiveQuest(); // Legacy direct giving
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (playerInRange && requireButtonPress && Input.GetKeyDown(interactKey))
        {
            if (useDialogForQuest)
            {
                // Update dialog based on current quest state right before triggering
                if (dialogTrigger != null)
                {
                    // If the current dialog IS the post-acceptance one, mark it as shown now
                    // so the *next* interaction uses the active dialog.
                    if (dialogTrigger.dialog == postAcceptanceDialog)
                    {
                        shownPostAcceptanceDialog = true;
                    }

                    dialogTrigger.dialog = GetAppropriateDialog(); // Get the potentially updated dialog
                    dialogTrigger.TriggerDialog();
                }
                else {
                     Debug.LogError($"QuestGiver on {gameObject.name}: useDialogForQuest is true, but DialogTrigger is missing!");
                }
            }
            else
            {
                // Legacy direct quest giving without dialog
                GiveQuest();
            }
        }
    }

    // Original method for directly giving quests (no dialog integration)
    public void GiveQuest()
    {
        if (questToGive != null && !questOfferedOrGiven && QuestManager.Instance != null)
        {
            // Check if already active/completed just in case
            if (QuestManager.Instance.IsQuestActive(questToGive) || QuestManager.Instance.IsQuestCompleted(questToGive))
            {
                Debug.Log($"Quest {questToGive.questName} already active or completed");
                questOfferedOrGiven = true; // Ensure flag is set
                shownPostAcceptanceDialog = true; // Skip post-acceptance if already active/done
                return;
            }

            // Add the quest to the player's active quests
            QuestManager.Instance.AddQuest(questToGive);
            questOfferedOrGiven = true; // Mark as given
            shownPostAcceptanceDialog = false; // Allow post-acceptance dialog to show next

            Debug.Log($"Gave quest directly: {questToGive.questName}");

            // Trigger post-acceptance dialog if available and NOT using dialog choice system
            if (!useDialogForQuest && postAcceptanceDialog != null && dialogTrigger != null)
            {
                dialogTrigger.dialog = postAcceptanceDialog;
                dialogTrigger.TriggerDialog();
                // Note: HandleDialogComplete won't fire if not subscribed,
                // so the shownPostAcceptanceDialog flag relies on the Update logic.
            }
            // Fallback to acceptedDialog if postAcceptanceDialog is null but acceptedDialog exists (for backward compatibility maybe?)
            // else if (!useDialogForQuest && acceptedDialog != null && dialogTrigger != null) { ... }
        }
    }

    // Called by UI button or other components to force give the quest
    public void ForceGiveQuest()
    {
        useDialogForQuest = false; // Ensure direct giving logic is used
        GiveQuest();
    }

    // Called by DialogManager when a choice accepts a quest
    public void HandleQuestAccepted(Quest quest)
    {
        if (quest == questToGive)
        {
            questOfferedOrGiven = true;
            shownPostAcceptanceDialog = false; // Reset this flag so the post-acceptance dialog can show
            Debug.Log($"Quest accepted via dialog choice: {quest.questName}. Ready for post-acceptance dialog.");

            // Optional: Immediately update the dialog trigger if the player is still in range
            if (playerInRange && dialogTrigger != null)
            {
                 dialogTrigger.dialog = GetAppropriateDialog();
            }
        }
    }
} 