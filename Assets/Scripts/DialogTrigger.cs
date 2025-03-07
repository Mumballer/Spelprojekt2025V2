using UnityEngine;
using System.Collections;

public class DialogTrigger : MonoBehaviour
{
    // dialogdata att visa
    [SerializeField] public Dialog dialog;

    // automatisk aktivering
    [SerializeField] private bool autoTrigger = false;
    // avstånd för aktivering
    [SerializeField] public float triggerDistance = 3f;
    // visuell uppmaning
    [SerializeField] public GameObject interactionPrompt;

    [Header("Quest Integration")]
    // uppdrag att avsluta
    [SerializeField] private Quest questToComplete;
    // index för uppdragsmål
    [SerializeField] private int objectiveIndex;
    // avsluta efter dialog
    [SerializeField] private bool completeQuestAfterDialog = false;

    private Transform playerTransform;
    private bool hasTriggered = false;

    private void Start()
    {
        // hitta spelaren
        PlayerController player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // ansluter till händelser
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.OnShowDialog += HidePrompt;
            DialogManager.Instance.OnHideDialog += CheckShowPrompt;
            DialogManager.Instance.OnDialogComplete += OnDialogCompleted;
        }
    }

    private void OnDestroy()
    {
        // rensar upp händelser
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
            // visa prompt om ingen dialog visas
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

    // dölj uppmaning vid dialog
    private void HidePrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    // kolla om uppmaning ska visas
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
        if (dialog != null && DialogManager.Instance != null && DialogManager.Instance.CanStartDialog())
        {
            Debug.Log($"Triggering dialog from {gameObject.name}");
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog));

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning($"Failed to trigger dialog from {gameObject.name}. Dialog: {(dialog == null ? "null" : "valid")}, DialogManager: {(DialogManager.Instance == null ? "null" : "valid")}, CanStartDialog: {(DialogManager.Instance != null ? DialogManager.Instance.CanStartDialog().ToString() : "N/A")}");
        }
    }

    // hantera uppdrag efter dialog
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