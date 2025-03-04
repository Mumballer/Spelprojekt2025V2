using UnityEngine;
using System.Collections;

public class DialogTrigger : MonoBehaviour
{
    // Change from private to public - this solves the accessibility issue
    [SerializeField] public Dialog dialog;

    [SerializeField] private bool autoTrigger = false;
    [SerializeField] private float triggerDistance = 3f;
    [SerializeField] private GameObject interactionPrompt;

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

        // Subscribe to dialog completion if we need to complete a quest
        if (completeQuestAfterDialog && questToComplete != null && DialogManager.Instance != null)
        {
            DialogManager.Instance.OnDialogComplete += OnDialogCompleted;
        }
    }

    private void OnDestroy()
    {
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.OnDialogComplete -= OnDialogCompleted;
        }
    }

    private void Update()
    {
        if (playerTransform == null || DialogManager.Instance == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= triggerDistance)
        {
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

    public void TriggerDialog()
    {
        if (dialog != null && DialogManager.Instance != null && DialogManager.Instance.CanStartDialog())
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog));

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    // Handle quest completion after dialog
    public void OnDialogCompleted(Dialog completedDialog)
    {
        // Now we can directly access dialog since it's public
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