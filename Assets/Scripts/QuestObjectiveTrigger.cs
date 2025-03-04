using UnityEngine;

public class QuestObjectiveTrigger : MonoBehaviour
{
    [SerializeField] private Quest linkedQuest;
    [SerializeField] private int objectiveIndex;
    [SerializeField] private bool autoComplete = false;
    [SerializeField] private float triggerDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool destroyAfterCompletion = true;

    private Transform playerTransform;
    private bool hasTriggered = false;

    private void Start()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }


    }

    private void Update()
    {
        if (playerTransform == null || QuestManager.Instance == null) return;

        if (!QuestManager.Instance.IsQuestActive(linkedQuest)) return;
        if (objectiveIndex >= 0 && objectiveIndex < linkedQuest.Objectives.Count &&
            linkedQuest.Objectives[objectiveIndex].isCompleted)
        {

            return;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= triggerDistance)
        {


            if (autoComplete && !hasTriggered)
            {
                CompleteObjective();
                hasTriggered = true;
            }
            else if (Input.GetKeyDown(interactKey))
            {
                CompleteObjective();
            }
        }
        else
        {


            if (distance > triggerDistance * 1.5f)
            {
                hasTriggered = false;
            }
        }
    }

    private void CompleteObjective()
    {
        if (linkedQuest != null && QuestManager.Instance != null &&
            objectiveIndex >= 0 && objectiveIndex < linkedQuest.Objectives.Count)
        {
            QuestManager.Instance.CompleteObjective(linkedQuest, objectiveIndex);


            if (destroyAfterCompletion)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}