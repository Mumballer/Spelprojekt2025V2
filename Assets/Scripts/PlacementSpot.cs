using UnityEngine;

public class PlacementSpot : MonoBehaviour
{
    [Header("Quest Settings")]
    [SerializeField] private Quest associatedQuest;
    [SerializeField] private int objectiveIndex;

    [Header("Placement Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool requireButtonPress = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject placedItemPrefab; // Visual representation of placed item

    [Header("UI Settings")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private string promptText = "Press E to place item";

    private bool isPlayerInRange = false;
    private bool isItemPlaced = false;

    private void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // If we have a placed item visual already in the scene, hide it until placement
        if (placedItemPrefab != null && placedItemPrefab.scene.IsValid())
        {
            placedItemPrefab.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // aktiverar prompt vid rätt tillfälle
        if (other.CompareTag(playerTag) && !isItemPlaced)
        {
            isPlayerInRange = true;

            // Only show prompt if quest is active and objective not completed
            if (associatedQuest != null)
            {
                // Use our custom method to check if the quest is active
                bool isActive = CheckQuestIsActive();
                
                if (isActive && objectiveIndex < associatedQuest.objectives.Count && 
                    !associatedQuest.objectives[objectiveIndex].isCompleted)
                {
                    if (interactionPrompt != null)
                    {
                        interactionPrompt.SetActive(true);

                        TMPro.TextMeshProUGUI promptTextComponent =
                            interactionPrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                        if (promptTextComponent != null)
                        {
                            promptTextComponent.text = promptText;
                        }
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (requireButtonPress && isPlayerInRange && Input.GetKeyDown(interactKey) && !isItemPlaced)
        {
            TryPlaceItem();
        }
    }

    private bool CheckQuestIsActive()
    {
        if (associatedQuest == null)
            return false;
        
        // First check the property directly
        if (associatedQuest.IsActive)
            return true;
        
        // Then check if it's in the active quests list
        if (QuestManager.Instance != null)
        {
            foreach (var quest in QuestManager.Instance.activeQuests)
            {
                if (quest.questName == associatedQuest.questName)
                    return true;
            }
        }
        
        return false;
    }

    public void TryPlaceItem()
    {
        // placerar objektet
        if (isItemPlaced) return;

        if (associatedQuest != null && QuestManager.Instance != null)
        {
            bool isActive = CheckQuestIsActive();
            Debug.Log($"<color=orange>PlacementSpot - Quest '{associatedQuest.questName}' active check: {isActive}</color>");
            
            if (!isActive)
            {
                Debug.Log("Cannot place item - quest is not active");
                return;
            }

            // Make sure the objective is not already completed
            if (objectiveIndex >= 0 && objectiveIndex < associatedQuest.objectives.Count)
            {
                QuestObjective objective = associatedQuest.objectives[objectiveIndex];

                if (objective.isCompleted)
                {
                    Debug.Log("This objective is already completed");
                    return;
                }

                // Complete this specific objective
                QuestManager.Instance.CompleteObjective(associatedQuest, objectiveIndex);

                // Mark as placed
                isItemPlaced = true;

                // Show the placed item visual
                if (placedItemPrefab != null)
                {
                    // If it's a prefab, instantiate it
                    if (!placedItemPrefab.scene.IsValid())
                    {
                        Instantiate(placedItemPrefab, transform.position, transform.rotation);
                    }
                    // If it's already in the scene, just activate it
                    else
                    {
                        placedItemPrefab.SetActive(true);
                    }
                }

                // Hide the prompt
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(false);
                }

                Debug.Log($"Item placed for objective {objectiveIndex} in quest {associatedQuest.questName}");

                // Make sure the quest UI stays visible for this quest
                if (QuestUI.Instance != null && QuestManager.Instance != null)
                {
                    // If this quest is still active, make sure the UI is showing
                    if (QuestManager.Instance.activeQuests.Contains(associatedQuest))
                    {
                        QuestUI.Instance.ShowQuestPanel();
                        Debug.Log("Making sure quest UI stays visible for active placement quest");
                    }
                }
            }
        }
    }
}