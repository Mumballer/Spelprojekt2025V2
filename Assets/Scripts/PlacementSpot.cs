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
        if (associatedQuest == null || QuestManager.Instance == null)
        {
            // Cannot check if quest asset or manager is missing
            return false;
        }

        // Directly check if the QuestManager's list contains this specific quest asset.
        // This is the most reliable way to know if it's active *right now*.
        bool isActive = QuestManager.Instance.IsQuestActive(associatedQuest); // Use QuestManager's check

        // Log the result from the QuestManager's perspective
        // Debug.Log($"PlacementSpot Check: Quest '{associatedQuest.questName}' IsActive according to QuestManager: {isActive}");

        return isActive;
    }

    public void TryPlaceItem()
    {
        if (isItemPlaced) return;

        // Use the reliable CheckQuestIsActive method
        bool isActive = CheckQuestIsActive();
        Debug.Log($"<color=orange>PlacementSpot - Quest '{associatedQuest?.questName ?? "NULL"}' active check: {isActive}</color>"); // Keep this log

        if (!isActive) // Use the result from our improved check
        {
            Debug.Log("Cannot place item - quest is not active according to QuestManager.");
            return; // Exit if not active
        }

        // Only proceed if the quest IS active
        if (associatedQuest != null && QuestManager.Instance != null)
        {
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
                Debug.Log($"Attempting to complete objective {objectiveIndex} via QuestManager for quest {associatedQuest.questName}");
                QuestManager.Instance.CompleteObjective(associatedQuest, objectiveIndex);

                // Mark as placed
                isItemPlaced = true;

                // Show the placed item visual
                if (placedItemPrefab != null)
                {
                    if (!placedItemPrefab.scene.IsValid())
                    {
                        Instantiate(placedItemPrefab, transform.position, transform.rotation);
                    }
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

                // Keep UI visible (existing logic)
                if (QuestUI.Instance != null && QuestManager.Instance != null)
                {
                     if (QuestManager.Instance.activeQuests.Contains(associatedQuest))
                     {
                         QuestUI.Instance.ShowQuestPanel();
                         Debug.Log("Making sure quest UI stays visible for active placement quest");
                     }
                 }
            }
            else
            {
                 Debug.LogWarning($"Invalid objective index ({objectiveIndex}) for quest '{associatedQuest.questName}'");
            }
        }
        else
        {
            Debug.LogError("Cannot place item - AssociatedQuest or QuestManager became null unexpectedly!");
        }
    }
}