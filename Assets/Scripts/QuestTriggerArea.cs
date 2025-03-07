using UnityEngine;

public class QuestTriggerArea : MonoBehaviour
{
    [Header("Quest Settings")]
    [SerializeField] private Quest questToAdd;
    [SerializeField] private string playerTag = "Player"; // The tag used for your player

    [Header("Trigger Options")]
    [SerializeField] private bool showDebugMessage = true;
    [SerializeField] private string triggerMessage = "Player entered quest area. Adding quest.";
    [SerializeField] private float destroyDelay = 0.1f; // Small delay before destroying (can be 0)

    private void OnValidate()
    {
        // Make sure there's a collider and it's set to trigger
        Collider collider = GetComponent<Collider>();
        if (collider != null && !collider.isTrigger)
        {
            Debug.LogWarning("QuestTriggerArea: Collider should be set to isTrigger.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player
        if (other.CompareTag(playerTag))
        {
            AddQuestToPlayer();
        }
    }

    private void AddQuestToPlayer()
    {
        // Skip if we don't have a valid quest to add
        if (questToAdd == null)
        {
            Debug.LogWarning("QuestTriggerArea: No quest assigned to add!", this);
            return;
        }

        if (QuestManager.Instance != null)
        {
            // Check if the player already has this quest
            if (QuestManager.Instance.IsQuestActive(questToAdd))
            {
                if (showDebugMessage)
                {
                    Debug.Log($"Player already has quest: {questToAdd.questName}");
                }
                DestroyTrigger();
                return;
            }

            // Check if the player already completed this quest
            if (QuestManager.Instance.IsQuestCompleted(questToAdd))
            {
                if (showDebugMessage)
                {
                    Debug.Log($"Player already completed quest: {questToAdd.questName}");
                }
                DestroyTrigger();
                return;
            }

            // Add the quest
            if (showDebugMessage)
            {
                Debug.Log(triggerMessage);
            }

            QuestManager.Instance.AddQuest(questToAdd);
            DestroyTrigger();
        }
        else
        {
            Debug.LogWarning("QuestTriggerArea: QuestManager instance not found. Cannot add quest.", this);
        }
    }

    private void DestroyTrigger()
    {
        if (destroyDelay <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    // Visualize the trigger area in the editor
    private void OnDrawGizmos()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.center, boxCollider.size);

            // Draw wireframe
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
    }
}