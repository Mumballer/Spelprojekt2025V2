using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    [SerializeField] private Quest questToComplete;
    [SerializeField] private bool autoComplete = false;
    [SerializeField] private float triggerDistance = 3f;
    [SerializeField] public GameObject interactionPrompt;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

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
    }

    private void Update()
    {
        if (playerTransform == null || QuestManager.Instance == null) return;

        if (!QuestManager.Instance.IsQuestActive(questToComplete)) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= triggerDistance)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }

            if (autoComplete && !hasTriggered)
            {
                CompleteQuest();
                hasTriggered = true;
            }
            else if (Input.GetKeyDown(interactKey))
            {
                CompleteQuest();
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

    private void CompleteQuest()
    {
        if (questToComplete != null && QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest(questToComplete);

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}