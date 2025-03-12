using UnityEngine;
using System.Collections;

public class DialogTrigger : MonoBehaviour
{
    [SerializeField] public Dialog dialog;

    [SerializeField] private bool autoTrigger = false;
    [SerializeField] public float triggerDistance = 3f;
    [SerializeField] public GameObject interactionPrompt;

    [Header("Quest Integration")]
    [SerializeField] private Quest questToComplete;
    [SerializeField] private int objectiveIndex;
    [SerializeField] private bool completeQuestAfterDialog = false;

    private Transform playerTransform;
    private bool hasTriggered = false;

    private void Start()
    {
        PlayerController player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // Subscribe to dialog events to handle prompt visibility
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.OnShowDialog += HidePrompt;
            DialogManager.Instance.OnHideDialog += CheckShowPrompt;
            DialogManager.Instance.OnDialogComplete += OnDialogCompleted;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.OnShowDialog -= HidePrompt;
            DialogManager.Instance.OnHideDialog -= CheckShowPrompt;
            DialogManager.Instance.OnDialogComplete -= OnDialogCompleted;
        }
    }

    private void Update()
    {
        if (playerTransform == null || DialogManager.Instance == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= triggerDistance)
        {
            // Only show prompt if no dialog is active
            if (interactionPrompt != null && !DialogManager.Instance.IsDialogActive)
            {
                interactionPrompt.SetActive(true);
            }

            if (autoTrigger && !hasTriggered && DialogManager.Instance.CanStartDialog())
            {
                TriggerDialog();
                hasTriggered = true;
            }
        }
        else
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            if (distance > triggerDistance * 1.5f)
            {
                hasTriggered = false;
            }
        }
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
            interactionPrompt.SetActive(true);
        }
    }

    public void TriggerDialog()
    {
        if (dialog == null)
        {
            Debug.LogError("Dialog is null!");
            return;
        }
        
        if (dialog.Lines == null)
        {
            Debug.LogError("Dialog Lines collection is null!");
            return;
        }
        
        if (dialog.Lines.Count == 0)
        {
            Debug.LogError("Dialog has 0 lines!");
            return;
        }
        
        if (DialogManager.Instance != null && DialogManager.Instance.CanStartDialog())
        {
            Debug.Log($"Triggering dialog from {gameObject.name}");
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog));
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    // Handle quest completion after dialog
    private void OnDialogCompleted(Dialog completedDialog)
    {
        if (completedDialog == dialog && completeQuestAfterDialog &&
            questToComplete != null && QuestManager.Instance != null)
        {
            if (QuestManager.Instance.IsQuestActive(questToComplete))
            {
                QuestManager.Instance.CompleteObjective(questToComplete, objectiveIndex);
                Debug.Log($"Dialog completed quest {questToComplete.questName}, objective {objectiveIndex}");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}