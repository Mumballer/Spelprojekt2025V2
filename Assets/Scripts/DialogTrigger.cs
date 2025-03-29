using UnityEngine;
using System.Collections;

public class DialogTrigger : MonoBehaviour
{
    [Header("Dialog Settings")]
    [Tooltip("The main dialog to play the first time the player interacts.")]
    [SerializeField] public Dialog dialog;
    [Tooltip("Optional: The dialog to play on subsequent interactions after the first one.")]
    [SerializeField] public Dialog repeatDialog; // New field for subsequent dialogs

    [Header("Trigger Settings")]
    [SerializeField] private bool autoTrigger = false; // Auto trigger on enter (usually for cutscenes)
    [SerializeField] public float triggerDistance = 3f;
    [SerializeField] public GameObject interactionPrompt;

    [Header("State Tracking")]
    [Tooltip("If true, the trigger resets after the dialog ends, allowing the initial dialog to play again.")]
    [SerializeField] private bool resetOnDialogEnd = false;
    private bool hasBeenTriggeredBefore = false; // Tracks if the initial dialog was triggered

    // --- Quest Integration (Consider removing if not used with this trigger type) ---
    [Header("Quest Integration (Optional)")]
    [Tooltip("Optional: Quest to complete objective for after this dialog finishes.")]
    [SerializeField] private Quest questToComplete;
    [SerializeField] private int objectiveIndex;
    [SerializeField] private bool completeQuestAfterDialog = false;
    // Note: Combining quest completion with repeatDialog might need careful design.
    // Ensure the quest completion logic makes sense if triggered by either dialog.
    // --- End Quest Integration ---


    private Transform playerTransform;
    // Removed 'hasTriggered' flag as 'hasBeenTriggeredBefore' serves a similar purpose for dialog selection

    private void Start()
    {
        Debug.Log($"--- DialogTrigger Start on {gameObject.name} ---");
        Debug.Log($"Interaction Prompt assigned: {interactionPrompt != null}");
        Debug.Log($"Dialog assigned: {(dialog != null ? dialog.name : "NULL")}");
        Debug.Log($"Repeat Dialog assigned: {(repeatDialog != null ? repeatDialog.name : "NULL")}");

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning($"DialogTrigger on {gameObject.name}: PlayerController not found!");
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // Subscribe to dialog events to handle prompt visibility and potential reset
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.OnShowDialog += HidePrompt;
            DialogManager.Instance.OnHideDialog += CheckShowPrompt; // Renamed from OnDialogCompleted for clarity
            // Subscribe to OnDialogComplete ONLY if needed for quest completion or reset
            if (completeQuestAfterDialog || resetOnDialogEnd)
            {
                 DialogManager.Instance.OnDialogComplete += HandleDialogCompleted;
            }
        }
        else
        {
             Debug.LogError($"DialogTrigger on {gameObject.name}: DialogManager instance not found!");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.OnShowDialog -= HidePrompt;
            DialogManager.Instance.OnHideDialog -= CheckShowPrompt;
             if (completeQuestAfterDialog || resetOnDialogEnd)
             {
                DialogManager.Instance.OnDialogComplete -= HandleDialogCompleted;
             }
        }
    }

    private void Update()
    {
        if (playerTransform == null || DialogManager.Instance == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool canInteract = distance <= triggerDistance;

        // Handle Interaction Prompt Visibility
        if (interactionPrompt != null)
        {
            // Show prompt only if in range AND no dialog is currently active globally
            interactionPrompt.SetActive(canInteract && !DialogManager.Instance.IsDialogActive);
        }

        // Handle Auto Trigger (usually for first interaction only)
        if (canInteract && autoTrigger && !hasBeenTriggeredBefore && DialogManager.Instance.CanStartDialog())
        {
            TriggerDialog();
            // autoTrigger usually only happens once, so we don't reset hasTriggered here
        }

        // Handle Manual Trigger (E key press is handled by DialogManager's TryInteractWithNPC now)
        // The DialogManager will call TriggerDialog on the correct trigger instance.
    }

    // Hide prompt when any dialog starts
    private void HidePrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    // Check if we should show prompt when dialog ends
    private void CheckShowPrompt()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance <= triggerDistance && interactionPrompt != null)
        {
            // Check again if another dialog started immediately
             if (!DialogManager.Instance.IsDialogActive)
             {
                interactionPrompt.SetActive(true);
             }
        }
    }

    public void TriggerDialog()
    {
        // --- Determine which dialog to use ---
        Dialog dialogToUse = null;
        if (hasBeenTriggeredBefore && repeatDialog != null)
        {
            dialogToUse = repeatDialog;
            Debug.Log($"DialogTrigger on {gameObject.name}: Using repeat dialog '{repeatDialog.name}'.");
        }
        else
        {
            dialogToUse = dialog; // Use the primary dialog
             if (hasBeenTriggeredBefore && repeatDialog == null)
             {
                 Debug.Log($"DialogTrigger on {gameObject.name}: Using primary dialog '{dialog?.name}' again (no repeat dialog assigned).");
             }
             else
             {
                 Debug.Log($"DialogTrigger on {gameObject.name}: Using primary dialog '{dialog?.name}'.");
             }
        }
        // --- End Determine Dialog ---


        // Validate the selected dialog
        if (dialogToUse == null)
        {
            Debug.LogError($"DialogTrigger on {gameObject.name}: No valid dialog asset assigned (primary or repeat)!");
            return;
        }
        if (dialogToUse.Lines == null || dialogToUse.Lines.Count == 0)
        {
            Debug.LogError($"DialogTrigger on {gameObject.name}: Selected dialog '{dialogToUse.name}' has no lines!");
            return;
        }


        // Attempt to start the dialog
        if (DialogManager.Instance != null && DialogManager.Instance.CanStartDialog())
        {
            Debug.Log($"Triggering dialog '{dialogToUse.name}' from {gameObject.name}");
            DialogManager.Instance.StartDialog(dialogToUse);

            // Mark that this trigger has been activated at least once
            hasBeenTriggeredBefore = true;

            // Hide prompt immediately
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
        else if (DialogManager.Instance == null)
        {
             Debug.LogError($"DialogTrigger on {gameObject.name}: DialogManager instance not found!");
        }
        else if (!DialogManager.Instance.CanStartDialog())
        {
             Debug.Log($"DialogTrigger on {gameObject.name}: Cannot start dialog right now (cooldown or already active).");
        }
    }

    // Handle events after a dialog completes
    private void HandleDialogCompleted(Dialog completedDialog)
    {
         // Check if the completed dialog is one associated with this trigger
         bool isRelevantDialog = (completedDialog == dialog || completedDialog == repeatDialog);

         if (!isRelevantDialog) return; // Ignore if it's from another trigger

        // Optional: Reset the trigger state if configured
        if (resetOnDialogEnd)
        {
            hasBeenTriggeredBefore = false;
            Debug.Log($"DialogTrigger on {gameObject.name}: Resetting trigger state after dialog '{completedDialog.name}'.");
        }

        // Optional: Handle quest completion
        if (completeQuestAfterDialog && questToComplete != null && QuestManager.Instance != null)
        {
            // Ensure we only complete if the *correct* dialog finished (primary or repeat)
            if (QuestManager.Instance.IsQuestActive(questToComplete))
            {
                QuestManager.Instance.CompleteObjective(questToComplete, objectiveIndex);
                Debug.Log($"DialogTrigger on {gameObject.name}: Completed quest '{questToComplete.questName}', objective {objectiveIndex} after dialog '{completedDialog.name}'.");
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}