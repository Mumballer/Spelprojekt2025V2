using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("Quest Settings")]
    [SerializeField] private Quest associatedQuest;
    [SerializeField] private int objectiveIndex;
    [SerializeField] private bool destroyOnCollect = true;

    [Header("Collection Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool requireButtonPress = false;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool isPlayerInRange = false;
    private bool isCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            
            if (!requireButtonPress && !isCollected)
            {
                CollectItem();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
        }
    }

    private void Update()
    {
        if (requireButtonPress && isPlayerInRange && Input.GetKeyDown(interactKey) && !isCollected)
        {
            CollectItem();
        }
    }

    public void CollectItem()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        if (associatedQuest != null && QuestManager.Instance != null)
        {
            // Make sure the quest is active
            if (!associatedQuest.IsActive)
            {
                QuestManager.Instance.AddQuest(associatedQuest);
            }
            
            // Complete this specific objective
            Debug.Log($"Completing objective {objectiveIndex} for quest {associatedQuest.questName}");
            QuestManager.Instance.CompleteObjective(associatedQuest, objectiveIndex);
            
            // Log the quest state after completing the objective
            string objectivesStatus = "";
            foreach (var objective in associatedQuest.objectives)
            {
                objectivesStatus += $"\n- {objective.description}: {(objective.isCompleted ? "Completed" : "Incomplete")}";
            }
            Debug.Log($"Quest objectives status after collection:{objectivesStatus}");
            Debug.Log($"Quest completion status: {associatedQuest.IsCompleted}");
            
            Debug.Log($"Collected item for quest: {associatedQuest.questName}, objective: {objectiveIndex + 1}");
            
            // Handle object disposal
            if (destroyOnCollect)
            {
                Destroy(gameObject);
            }
            else
            {
                // Fix for the nullable reference operator error - use a normal if check
                Renderer renderer = GetComponent<Renderer>();
                if (renderer != null)
                    renderer.enabled = false;
                
                Collider collider = GetComponent<Collider>();
                if (collider != null)
                    collider.enabled = false;
            }
        }
    }
} 