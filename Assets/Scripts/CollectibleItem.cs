using UnityEngine;
using System.Collections;

public class CollectibleItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private string itemID;
    [SerializeField] private string itemName;
    [SerializeField] private string itemDescription;
    
    [Header("Interaction Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool requireButtonPress = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 2f;
    
    [Header("Quest Integration")]
    [SerializeField] private Quest associatedQuest;
    [SerializeField] private int objectiveIndex;
    
    [Header("UI Settings")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private string promptText = "Press E to collect";
    
    [Header("Hover UI Settings")]
    [SerializeField] private GameObject hoverUIElement; // UI element to show when hovering
    [SerializeField] private float hoverUIDisplayTime = 0f; // 0 means display as long as hovering
    [SerializeField] private bool showOnlyForActiveQuest = true; // Only show for active quest items
    
    private bool playerInRange = false;
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
            playerInRange = true;
            
            bool shouldShowUI = true;
            
            // Check if we should only show UI for active quest items
            if (showOnlyForActiveQuest && associatedQuest != null)
            {
                shouldShowUI = associatedQuest.IsActive && 
                               !associatedQuest.objectives[objectiveIndex].isCompleted;
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
            playerInRange = false;
            
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
        // Handle hover UI timer if it's set
        if (hoverUIDisplayTime > 0 && hoverUIElement != null && hoverUIElement.activeSelf)
        {
            hoverUITimer -= Time.deltaTime;
            if (hoverUITimer <= 0)
            {
                hoverUIElement.SetActive(false);
            }
        }
        
        if (requireButtonPress && playerInRange && Input.GetKeyDown(interactKey) && !isCollected)
        {
            CollectItem();
        }
    }
    
    public void CollectItem()
    {
        if (isCollected) return;
        
        if (associatedQuest != null && QuestManager.Instance != null)
        {
            // Make sure the quest is active
            if (!associatedQuest.IsActive)
            {
                Debug.Log("Cannot collect item - quest is not active");
                return;
            }
            
            Debug.Log($"Completing objective {objectiveIndex} for quest {associatedQuest.questName}");
            
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
            Destroy(gameObject);
        }
    }
    
    // Add this method for automatic collection without button press
    public void AutoCollect()
    {
        CollectItem();
    }
    
    // Draw gizmo for interaction range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
} 