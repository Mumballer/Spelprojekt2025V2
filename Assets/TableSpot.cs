using System;
using UnityEngine;

public class TableSpot : MonoBehaviour
{
    [Header("References")]
    public GameObject tableNametag;
    public GameObject highlightObject;
    public Material correctPlacementMaterial;
    public Material incorrectPlacementMaterial;
    public Material defaultMaterial;

    [Header("Settings")]
    public string assignedNametag; // The name of the nametag that should go here
    public bool isInteractable = true;
    public float feedbackDuration = 1.0f;
    [Tooltip("Maximum distance the player can be to interact with this spot")]
    public float interactionDistance = 2.0f;

    [Header("Nametag Counting")]
    [SerializeField] private bool countTowardsTotal = true; // Whether this spot counts towards the total
    [SerializeField] private static int targetNametagCount = 6; // When this many nametags are placed, complete the quest
    [SerializeField] private bool autoCompleteQuest = true; // Whether to auto-complete the quest when target is reached

    [Header("Debug")]
    public bool showDebug = false;

    // Event that fires when a nametag is placed at this spot
    public event Action<TableSpot, string> OnNametagPlaced;

    // Static event for when the nametag count changes
    public static event Action<int, int> OnNametagCountChanged; // (current, total)

    // Static event for when all nametags are placed
    public static event Action OnAllNametagsPlaced;

    // Static counter for placed nametags
    private static int placedNametagCount = 0;
    private static int totalNametagSpots = 0;
    private static bool questCompleted = false;

    // Flag to track if this spot already has a placed nametag that was counted
    private bool hasBeenCounted = false;

    // Reference to player for checking held nametags
    private GameObject player;
    private Transform playerTransform;
    private Nametag heldNametag;
    private Renderer spotRenderer;
    private Material originalMaterial;
    private bool playerInRange = false;

    // Static constructor to reset count when domain reloads
    static TableSpot()
    {
        placedNametagCount = 0;
        totalNametagSpots = 0;
        questCompleted = false;
    }

    private void Awake()
    {
        // Register this spot in the total count if it should be counted
        if (countTowardsTotal)
        {
            totalNametagSpots++;
        }
    }

    private void Start()
    {
        // Find the player
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Get renderer if available
        spotRenderer = GetComponent<Renderer>();
        if (spotRenderer != null)
        {
            originalMaterial = spotRenderer.material;
        }

        // Initialize highlight state
        if (highlightObject != null)
        {
            highlightObject.SetActive(false);
        }

        // Check if there's already a nametag component on this spot
        if (tableNametag != null)
        {
            Nametag existingTag = tableNametag.GetComponent<Nametag>();
            if (existingTag != null)
            {
                Debug.Log($"Nametag component found on {tableNametag.name}. IsPickedUp: {existingTag.IsPickedUp()}");
                // Make sure the nametag is invisible at start
                existingTag.SetAlpha(0f);
            }
            else
            {
                Debug.LogError($"TableSpot has tableNametag reference but no Nametag component on {tableNametag.name}");
            }
        }
    }

    private void OnDestroy()
    {
        // Reduce the total count when this spot is destroyed
        if (countTowardsTotal)
        {
            totalNametagSpots--;
        }

        // Reduce the placed count if this spot was counted
        if (hasBeenCounted)
        {
            placedNametagCount--;
            NotifyCountChanged();
        }
    }

    private void Update()
    {
        // Check if player is in range
        CheckPlayerDistance();

        // Handle interaction
        if (playerInRange && isInteractable && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    private void CheckPlayerDistance()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool inRange = distance <= interactionDistance;

        // Only update highlight if the range status changed
        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            ShowHighlight(playerInRange);
        }
    }

    public void ShowHighlight(bool show)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(show && isInteractable);
        }
    }

    public void Interact()
    {
        // If this spot already has a nametag, make it visible instead of just logging a message
        if (tableNametag != null)
        {
            Debug.Log($"This spot already has a nametag: {tableNametag.name}. Making it visible.");

            // Get the nametag component
            Nametag nametagComponent = tableNametag.GetComponent<Nametag>();
            if (nametagComponent != null)
            {
                // Make the existing nametag visible
                nametagComponent.PlaceNametag();

                // Count this nametag if it hasn't been counted yet
                if (countTowardsTotal && !hasBeenCounted)
                {
                    IncrementNametagCount();
                }

                // Fire the event with this spot and the nametag name
                if (OnNametagPlaced != null)
                {
                    OnNametagPlaced.Invoke(this, nametagComponent.name);
                    Debug.Log($"Fired OnNametagPlaced event for {nametagComponent.name}");
                }

                // Show positive feedback if you have a material for that
                if (correctPlacementMaterial != null)
                {
                    ShowFeedback(correctPlacementMaterial, feedbackDuration);
                }

                // Hide the highlight indicator
                if (highlightObject != null)
                {
                    highlightObject.SetActive(false);
                }

                // Prevent further interaction
                isInteractable = false;
            }
            else
            {
                Debug.LogError($"Found tableNametag reference but it has no Nametag component: {tableNametag.name}");
            }

            return;
        }

        // Try to get the player's held nametag
        if (player != null)
        {
            heldNametag = player.GetComponentInChildren<Nametag>();

            if (heldNametag != null)
            {
                // Check if this is the correct nametag for this spot
                bool isCorrectNametag = string.IsNullOrEmpty(assignedNametag) ||
                                        heldNametag.name == assignedNametag;

                // The player is carrying a nametag, place it at this spot
                Debug.Log($"Placing nametag {heldNametag.name} at spot {gameObject.name}");

                // Tell the nametag to place itself here
                heldNametag.PlaceAtSpot(this);

                // Show visual feedback
                if (isCorrectNametag)
                {
                    if (correctPlacementMaterial != null)
                    {
                        ShowFeedback(correctPlacementMaterial, feedbackDuration);
                    }

                    // Count this nametag if it should be counted
                    if (countTowardsTotal && !hasBeenCounted)
                    {
                        IncrementNametagCount();
                    }

                    // Fire event
                    if (OnNametagPlaced != null)
                    {
                        OnNametagPlaced.Invoke(this, heldNametag.name);
                    }
                }
                else
                {
                    if (incorrectPlacementMaterial != null)
                    {
                        ShowFeedback(incorrectPlacementMaterial, feedbackDuration);
                    }
                }

                // Prevent further interaction if correct placement
                if (isCorrectNametag)
                {
                    isInteractable = false;
                }
            }
            else
            {
                Debug.Log("No nametag is being carried by the player.");
            }
        }
    }

    // Method to increment the nametag count
    private void IncrementNametagCount()
    {
        placedNametagCount++;
        hasBeenCounted = true;
        Debug.Log($"Nametag count increased to {placedNametagCount}/{targetNametagCount}");

        NotifyCountChanged();

        // Check if we've reached the target count
        if (placedNametagCount >= targetNametagCount && !questCompleted && autoCompleteQuest)
        {
            CompleteNametagQuest();
        }
    }

    // NEW: Method to complete the nametag quest
    private void CompleteNametagQuest()
    {
        if (questCompleted) return; // Prevent multiple completions

        Debug.Log($"All {targetNametagCount} nametags placed! Completing quest...");
        questCompleted = true;

        // Trigger the all nametags placed event
        if (OnAllNametagsPlaced != null)
        {
            OnAllNametagsPlaced.Invoke();
        }

        // Find and complete the nametag quest
        CompleteQuestInManager();
    }

    // Find and complete the nametag quest in the quest manager
    private void CompleteQuestInManager()
    {
        // Try to find the NametagQuestManager first
        NametagQuestManager nametagManager = FindObjectOfType<NametagQuestManager>();
        if (nametagManager != null)
        {
            // Try to complete via ForceCompleteQuest method if it exists
            System.Reflection.MethodInfo forceComplete = nametagManager.GetType().GetMethod("ForceCompleteQuest");
            if (forceComplete != null)
            {
                forceComplete.Invoke(nametagManager, null);
                Debug.Log("Completed nametag quest via NametagQuestManager.ForceCompleteQuest");
                return;
            }

            // Try to get the quest reference and complete it via QuestManager
            Quest nametagQuest = nametagManager.GetNametagQuest();
            if (nametagQuest != null && QuestManager.Instance != null)
            {
                // Check if the quest is active
                if (QuestManager.Instance.IsQuestActive(nametagQuest))
                {
                    // Complete the first objective (assuming this is the nametag placement objective)
                    nametagQuest.CompleteObjective(0);
                    Debug.Log("Completed nametag quest via objective completion");
                    return;
                }
            }
        }

        // Fallback: try to find any NametagQuest in active quests
        if (QuestManager.Instance != null)
        {
            foreach (var quest in QuestManager.Instance.GetActiveQuests())
            {
                if (quest is NametagQuest ||
                    (quest != null && quest.questName.Contains("Nametag") || quest.questName.Contains("Dinner")))
                {
                    // Complete the first objective (assuming this is the nametag placement objective)
                    quest.CompleteObjective(0);
                    Debug.Log($"Completed quest: {quest.questName}");
                    return;
                }
            }
        }

        Debug.LogWarning("Could not find nametag quest to complete!");
    }

    // Notify listeners that the count changed
    private void NotifyCountChanged()
    {
        if (OnNametagCountChanged != null)
        {
            OnNametagCountChanged.Invoke(placedNametagCount, targetNametagCount);
        }
    }

    // Static method to get current count
    public static int GetPlacedNametagCount()
    {
        return placedNametagCount;
    }

    // Static method to get target count
    public static int GetTargetNametagCount()
    {
        return targetNametagCount;
    }

    // Static method to set the target count
    public static void SetTargetNametagCount(int count)
    {
        targetNametagCount = Mathf.Max(1, count);
    }

    // Static method to reset the count (for testing or scene changes)
    public static void ResetNametagCount()
    {
        placedNametagCount = 0;
        questCompleted = false;
    }

    // Show visual feedback for nametag placement
    public void ShowFeedback(Material feedbackMaterial, float duration)
    {
        if (spotRenderer != null && feedbackMaterial != null)
        {
            spotRenderer.material = feedbackMaterial;
            Invoke("ResetMaterial", duration);
        }
    }

    // Reset material after feedback
    private void ResetMaterial()
    {
        if (spotRenderer != null)
        {
            spotRenderer.material = originalMaterial;
        }
    }

    // Set the nametag for this spot
    public void SetNametag(GameObject nametag, string nametagName)
    {
        tableNametag = nametag;
        assignedNametag = nametagName;

        if (showDebug)
        {
            Debug.Log($"Set nametag {nametagName} at spot {gameObject.name}");
        }
    }

    // Clear the nametag from this spot
    public void ClearSpot()
    {
        // Decrement count if this spot was counted
        if (hasBeenCounted && countTowardsTotal)
        {
            placedNametagCount--;
            hasBeenCounted = false;
            NotifyCountChanged();
        }

        tableNametag = null;

        if (showDebug)
        {
            Debug.Log($"Cleared nametag from spot {gameObject.name}");
        }
    }

    private void OnDrawGizmos()
    {
        // Draw a visual indicator for this spot in the editor
        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f);
        Gizmos.DrawCube(transform.position, new Vector3(0.1f, 0.01f, 0.1f));

        // Draw interaction range
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.2f);
        Gizmos.DrawSphere(transform.position, interactionDistance);

        // Draw text for the assigned nametag
        if (!string.IsNullOrEmpty(assignedNametag))
        {
            Gizmos.color = Color.white;
            // You'd typically use handles for text, but that requires UnityEditor namespace
        }
    }
}