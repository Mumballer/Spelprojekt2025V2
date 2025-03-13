using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("Quest Settings")]
    [SerializeField] private Quest associatedQuest;
    [SerializeField] private int objectiveIndex;

    [Header("Collection Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool requireButtonPress = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool destroyOnCollect = true;

    [Header("UI Settings")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private string promptText = "Press E to collect";

    [Header("Hover UI Settings")]
    [SerializeField] private GameObject hoverUIElement; // UI element to show when hovering
    [SerializeField] private float hoverUIDisplayTime = 0f; // 0 means display as long as hovering
    [SerializeField] private bool showOnlyForActiveQuest = true; // Only show for active quest items

    private bool isPlayerInRange = false;
    private bool isCollected = false;
    private float hoverUITimer = 0f;

    private void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        if (hoverUIElement != null)
        {
            hoverUIElement.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !isCollected)
        {
            isPlayerInRange = true;

            // Only show prompt if quest is active and objective not completed
            if (associatedQuest != null)
            {
                // Use our improved CheckQuestIsActive method instead of direct check
                bool isActive = CheckQuestIsActive();
                
                Debug.Log($"<color=magenta>CollectibleItem - OnTriggerEnter - Quest '{associatedQuest.questName}' active: {isActive}</color>");
                
                if (isActive && objectiveIndex < associatedQuest.objectives.Count &&
                    !associatedQuest.objectives[objectiveIndex].isCompleted)
                {
                    if (interactionPrompt != null)
                    {
                        interactionPrompt.SetActive(true);
                        Debug.Log($"<color=magenta>CollectibleItem - Showing interaction prompt</color>");

                        TMPro.TextMeshProUGUI promptTextComponent =
                            interactionPrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                        if (promptTextComponent != null)
                        {
                            promptTextComponent.text = promptText;
                            Debug.Log($"<color=magenta>CollectibleItem - Set prompt text to: {promptText}</color>");
                        }
                        else
                        {
                            Debug.LogWarning($"<color=red>CollectibleItem - No TextMeshProUGUI found in interaction prompt!</color>");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"<color=red>CollectibleItem - No interaction prompt assigned!</color>");
                    }

                    // Show hover UI if available
                    if (hoverUIElement != null)
                    {
                        hoverUIElement.SetActive(true);
                        hoverUITimer = hoverUIDisplayTime;
                        Debug.Log($"<color=magenta>CollectibleItem - Showing hover UI</color>");
                    }
                }
                else
                {
                    Debug.Log($"<color=orange>CollectibleItem - Cannot show UI - Quest active: {isActive}, Objective completed: {(objectiveIndex < associatedQuest.objectives.Count ? associatedQuest.objectives[objectiveIndex].isCompleted : false)}</color>");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;

            // Hide both UI elements when player leaves
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            if (hoverUIElement != null)
            {
                hoverUIElement.SetActive(false);
            }
        }
    }

    private void Update()
    {
        // Try to find the active quest reference periodically
        if (Time.frameCount % 30 == 0) // Check roughly every 30 frames
        {
            FindMatchingActiveQuest();
        }

        // Handle hover UI timer if it's set
        if (hoverUIDisplayTime > 0 && hoverUIElement != null && hoverUIElement.activeSelf)
        {
            hoverUITimer -= Time.deltaTime;
            if (hoverUITimer <= 0)
            {
                hoverUIElement.SetActive(false);
            }
        }

        if (requireButtonPress && isPlayerInRange && Input.GetKeyDown(interactKey) && !isCollected)
        {
            CollectItem();
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

    // Find active quest with the same name
    private bool FindMatchingActiveQuest()
    {
        if (associatedQuest == null || QuestManager.Instance == null)
            return false;
        
        string questName = associatedQuest.questName;
        
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            if (quest.questName == questName)
            {
                associatedQuest = quest; // Update to the active instance
                return true;
            }
        }
        
        return false;
    }

    public void CollectItem()
    {
        if (isCollected) return;

        // Try to find the right quest reference first
        FindMatchingActiveQuest();

        if (associatedQuest != null && QuestManager.Instance != null)
        {
            // Make sure the quest is active - check in active quests list
            bool isActive = CheckQuestIsActive();
            Debug.Log($"<color=orange>CollectItem - Quest '{associatedQuest.questName}' active check: {isActive}</color>");
            
            if (!isActive)
            {
                Debug.Log("<color=red>Cannot collect item - quest is not active</color>");
                return;
            }

            Debug.Log($"<color=green>Completing objective {objectiveIndex} for quest {associatedQuest.questName}</color>");

            // Complete this specific objective
            QuestManager.Instance.CompleteObjective(associatedQuest, objectiveIndex);

            // Mark as collected
            isCollected = true;

            // Debug logs for quest status
            Debug.Log("Quest objectives status after collection:");
            foreach (var objective in associatedQuest.objectives)
            {
                Debug.Log($"- {objective.description}: {(objective.isCompleted ? "Completed" : "Incomplete")}");
            }
            Debug.Log($"Quest completion status: {associatedQuest.IsCompleted}");

            Debug.Log($"Collected item for quest: {associatedQuest.questName}, objective: {objectiveIndex}");

            // Hide all UI elements
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            if (hoverUIElement != null)
            {
                hoverUIElement.SetActive(false);
            }

            // Destroy the item if configured to do so
            if (destroyOnCollect)
            {
                Destroy(gameObject);
            }
            else
            {
                // Just disable visuals
                MeshRenderer renderer = GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = false;
                }

                Collider itemCollider = GetComponent<Collider>();
                if (itemCollider != null)
                {
                    itemCollider.enabled = false;
                }
            }
        }
    }
}