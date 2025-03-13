using UnityEngine;
using System.Collections;

public class CollectibleItem : MonoBehaviour
{
    [Header("Quest Settings")]
    [SerializeField] private Quest associatedQuest;
    [SerializeField] private int objectiveIndex;
    [SerializeField] private bool findActiveQuestByName = true; // NEW - Find quest by name in active quests

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
    private float questCheckTimer = 0f;
    private const float QUEST_CHECK_INTERVAL = 1.0f;

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
        
        // Try to connect to the active quest if it exists
        if (associatedQuest != null && findActiveQuestByName)
        {
            StartCoroutine(TryReconnectToActiveQuest());
        }
    }

    private IEnumerator TryReconnectToActiveQuest()
    {
        // Wait a moment for the quest system to initialize
        yield return new WaitForSeconds(0.5f);
        
        if (QuestManager.Instance == null || associatedQuest == null)
            yield break;
            
        string questName = associatedQuest.questName;
        Debug.Log($"<color=cyan>Collectible trying to find active quest: {questName}</color>");
        
        // Look for an active quest with the same name
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            if (quest.questName == questName)
            {
                associatedQuest = quest;  // Use the active quest instance
                Debug.Log($"<color=cyan>Collectible connected to active quest: {questName}</color>");
                yield break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !isCollected)
        {
            isPlayerInRange = true;

            // Update quest reference before checking status
            if (findActiveQuestByName)
                FindMatchingActiveQuest();

            bool shouldShowUI = true;

            // Check if we should only show UI for active quest items
            if (showOnlyForActiveQuest && associatedQuest != null)
            {
                bool isQuestActive = CheckQuestIsActive();
                bool isObjectiveCompleted = false;
                
                if (objectiveIndex >= 0 && objectiveIndex < associatedQuest.objectives.Count)
                {
                    isObjectiveCompleted = associatedQuest.objectives[objectiveIndex].isCompleted;
                }
                
                shouldShowUI = isQuestActive && !isObjectiveCompleted;
            }

            if (shouldShowUI)
            {
                // Show interaction prompt
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

                // Show hover UI
                if (hoverUIElement != null)
                {
                    hoverUIElement.SetActive(true);

                    // If timer is set, start the countdown
                    if (hoverUIDisplayTime > 0)
                    {
                        hoverUITimer = hoverUIDisplayTime;
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
        // Periodically check for quest updates
        questCheckTimer += Time.deltaTime;
        if (questCheckTimer >= QUEST_CHECK_INTERVAL)
        {
            questCheckTimer = 0f;
            if (findActiveQuestByName && associatedQuest != null)
            {
                FindMatchingActiveQuest();
            }
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

    // Check if quest is active, looking in the active quests list
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

    public void CollectItem()
    {
        if (isCollected) return;

        // Make sure we have the latest quest reference
        if (findActiveQuestByName)
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