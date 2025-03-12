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
            if (associatedQuest != null &&
                associatedQuest.IsActive &&
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

    public void TryPlaceItem()
    {
        if (isItemPlaced) return;

        if (associatedQuest != null && QuestManager.Instance != null)
        {
            // Make sure the quest is active
            if (!associatedQuest.IsActive)
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
            }
        }
    }
}