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
            QuestManager.Instance.CompleteObjective(associatedQuest, objectiveIndex);
            
            Debug.Log($"Collected item for quest: {associatedQuest.questName}, objective: {objectiveIndex + 1}");
            
            // Handle object disposal
            if (destroyOnCollect)
            {
                Destroy(gameObject);
            }
            else
            {
                // Just hide it
                GetComponent<Renderer>()?.enabled = false;
                GetComponent<Collider>()?.enabled = false;
            }
        }
    }
} 