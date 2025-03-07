using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class NametagQuestManager : MonoBehaviour
{
    [Header("Quest Settings")]
    [SerializeField] private NametagQuest nametagQuest;
    [SerializeField] private bool autoAddQuest = false; // New option to control auto-adding

    [Header("Placement Tracking")]
    [SerializeField] private TableSpot[] tableSpots;
    [SerializeField] private float updateDelay = 1.5f; // Delay in seconds before updating the counter
    [SerializeField] private float checkInterval = 0.5f; // How often to check for nametag placements
    [SerializeField] private float startTrackingDelay = 2.0f; // Delay before starting to track after quest add

    private HashSet<string> placedNametags = new HashSet<string>();
    private HashSet<string> pendingNametags = new HashSet<string>(); // Track nametags being processed
    private Dictionary<TableSpot, bool> processedSpots = new Dictionary<TableSpot, bool>();
    private bool questInitialized = false;
    private bool isTrackingActive = false;
    private Coroutine trackingCoroutine = null;

    // Singleton instance for direct access
    public static NametagQuestManager Instance { get; private set; }

    private void Awake()
    {
        // Setup singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Verify quest is assigned
        if (nametagQuest == null)
        {
            Debug.LogError("Nametag quest not assigned to the NametagQuestManager!");
            return;
        }

        // Find all TableSpots if not manually assigned
        if (tableSpots == null || tableSpots.Length == 0)
        {
            // Use the new FindObjectsByType method instead of FindObjectsOfType
            tableSpots = UnityEngine.Object.FindObjectsByType<TableSpot>(FindObjectsSortMode.None);
            Debug.Log($"Found {tableSpots.Length} table spots in the scene");
        }

        // Verify we have enough table spots for the quest
        if (tableSpots.Length < nametagQuest.nametagNames.Length)
        {
            Debug.LogWarning($"Not enough table spots ({tableSpots.Length}) for all nametags ({nametagQuest.nametagNames.Length})!");
        }

        // Initialize the processed spots dictionary
        InitializeProcessedSpots();

        // Subscribe to table spot events
        RegisterTableSpotEvents();

        // Make sure quest is reset to initial state
        ResetQuestState();

        // Only auto-add the quest if the option is enabled (off by default)
        if (autoAddQuest && QuestManager.Instance != null)
        {
            // Check if the quest is active in the quest manager
            bool isQuestActive = QuestManager.Instance.IsQuestActive(nametagQuest);
            if (!isQuestActive)
            {
                AddQuestToManager();
            }
        }
    }

    private void RegisterTableSpotEvents()
    {
        foreach (TableSpot spot in tableSpots)
        {
            if (spot != null)
            {
                try
                {
                    // Register for the placement event
                    spot.OnNametagPlaced += HandleNametagPlaced;
                    Debug.Log($"Successfully registered event handler for spot: {spot.name}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to register event for spot {spot.name}: {e.Message}");
                }
            }
        }
        Debug.Log("Registered for TableSpot placement events");
    }

    private void UnregisterTableSpotEvents()
    {
        foreach (TableSpot spot in tableSpots)
        {
            if (spot != null)
            {
                try
                {
                    // Unregister from the placement event
                    spot.OnNametagPlaced -= HandleNametagPlaced;
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }
        }
    }

    // Direct handler for nametag placement events
    public void HandleNametagPlaced(TableSpot spot, string nametagName)
    {
        Debug.Log($"NametagQuestManager received placement event: '{nametagName}' at {spot.name}");

        // Ignore if not tracking
        if (!isTrackingActive)
        {
            Debug.Log($"Ignoring placement of '{nametagName}' - tracking not active");
            return;
        }

        // Ignore if quest not active
        if (QuestManager.Instance == null || !QuestManager.Instance.IsQuestActive(nametagQuest))
        {
            Debug.Log($"Ignoring placement of '{nametagName}' - quest not active");
            return;
        }

        // Validate this is a nametag we care about
        if (!System.Array.Exists(nametagQuest.nametagNames, name => name == nametagName))
        {
            Debug.Log($"Ignoring placement of '{nametagName}' - not part of quest nametags");
            return;
        }

        // Check if we've already counted this nametag
        if (placedNametags.Contains(nametagName) || pendingNametags.Contains(nametagName))
        {
            Debug.Log($"Ignoring placement of '{nametagName}' - already counted");
            return;
        }

        // Mark spot as processed
        processedSpots[spot] = true;

        // Add to pending list
        pendingNametags.Add(nametagName);
        Debug.Log($"Added '{nametagName}' to pending nametags");

        // Start coroutine to update the counter after a delay
        StartCoroutine(UpdateCounterWithDelay(nametagName));
    }

    private void InitializeProcessedSpots()
    {
        processedSpots.Clear();
        foreach (TableSpot spot in tableSpots)
        {
            if (spot != null)
            {
                processedSpots[spot] = false;
            }
        }
    }

    // Reset quest to initial state
    private void ResetQuestState()
    {
        if (nametagQuest != null && nametagQuest.Objectives.Count > 0)
        {
            // Reset quest state
            nametagQuest.ResetQuest();

            // Set initial objective text
            nametagQuest.Objectives[0].description =
                $"Place nametags at the table (0/{nametagQuest.nametagNames.Length})";
            nametagQuest.Objectives[0].isCompleted = false;

            // Mark as initialized
            questInitialized = true;

            Debug.Log("Nametag quest reset to initial state");
        }
    }

    // Public method to add the quest - can be called from dialogue system
    public void AddQuestToManager()
    {
        if (QuestManager.Instance != null)
        {
            // Stop any existing tracking
            StopTracking();

            // Reset tracking data
            ResetTracking();

            // Make sure quest is properly initialized
            if (!questInitialized)
            {
                ResetQuestState();
            }

            // Check if the quest is already active
            bool isQuestActive = QuestManager.Instance.IsQuestActive(nametagQuest);
            if (!isQuestActive)
            {
                QuestManager.Instance.AddQuest(nametagQuest);
                Debug.Log($"Added nametag quest: {nametagQuest.questName}");

                // Start tracking with a delay to avoid counting pre-existing nametags
                StartTrackingWithDelay(startTrackingDelay);
            }
            else
            {
                Debug.LogWarning("Quest already active in QuestManager");
            }
        }
        else
        {
            Debug.LogError("QuestManager not found in scene");
        }
    }

    // Public method to get the quest for dialogue references
    public Quest GetNametagQuest()
    {
        return nametagQuest;
    }

    // Start tracking after a delay
    private void StartTrackingWithDelay(float delay)
    {
        // Start the coroutine and store a reference to it
        if (trackingCoroutine != null)
        {
            StopCoroutine(trackingCoroutine);
        }

        trackingCoroutine = StartCoroutine(StartTrackingDelayed(delay));
    }

    // Coroutine to start tracking after a delay
    private IEnumerator StartTrackingDelayed(float delay)
    {
        Debug.Log($"Will start tracking nametags after {delay} seconds");

        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Start tracking
        isTrackingActive = true;
        Debug.Log("Starting to track nametag placements");

        // Check for any already placed nametags that might have been missed
        CheckAllTableSpots();
    }

    // Stop tracking nametags
    private void StopTracking()
    {
        isTrackingActive = false;

        // Stop the coroutine if it's running
        if (trackingCoroutine != null)
        {
            StopCoroutine(trackingCoroutine);
            trackingCoroutine = null;
        }

        Debug.Log("Stopped tracking nametag placements");
    }

    // Check all table spots for correct nametag placements (once)
    private void CheckAllTableSpots()
    {
        Debug.Log("Checking all table spots for already placed nametags");

        foreach (TableSpot spot in tableSpots)
        {
            // Skip processed spots or null spots
            if (spot == null || (processedSpots.ContainsKey(spot) && processedSpots[spot]))
                continue;

            if (spot.tableNametag != null && !string.IsNullOrEmpty(spot.assignedNametag))
            {
                string nametagName = spot.assignedNametag;

                // Validate this is a nametag we care about
                if (System.Array.Exists(nametagQuest.nametagNames, name => name == nametagName))
                {
                    Debug.Log($"Found already placed nametag: '{nametagName}' at {spot.name}");

                    // Check if we've already counted this nametag
                    if (!placedNametags.Contains(nametagName) && !pendingNametags.Contains(nametagName))
                    {
                        // Mark this spot as processed
                        processedSpots[spot] = true;

                        // Add directly to placed nametags (no delay for pre-existing)
                        placedNametags.Add(nametagName);

                        Debug.Log($"Counted pre-existing nametag: '{nametagName}'");
                    }
                }
            }
        }

        // Update the counter with the current count
        int currentCount = placedNametags.Count;
        UpdateQuestObjective(currentCount);
    }

    // Update the counter with a delay
    private IEnumerator UpdateCounterWithDelay(string nametagName)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(updateDelay);

        // If tracking is no longer active, don't update
        if (!isTrackingActive)
        {
            pendingNametags.Remove(nametagName);
            yield break;
        }

        // Move from pending to placed
        pendingNametags.Remove(nametagName);
        placedNametags.Add(nametagName);

        // Get the new count
        int newCount = placedNametags.Count;

        // Update the counter in the quest objective
        UpdateQuestObjective(newCount);

        Debug.Log($"NametagQuestManager: Counter updated - {newCount}/{nametagQuest.nametagNames.Length} nametags placed");
    }

    // Update the quest objective text and completion state
    private void UpdateQuestObjective(int count)
    {
        if (nametagQuest != null && nametagQuest.Objectives.Count > 0 &&
            QuestManager.Instance != null && QuestManager.Instance.IsQuestActive(nametagQuest))
        {
            // Log all placed nametags for debugging
            string placedNames = string.Join(", ", placedNametags);
            Debug.Log($"Placed nametags: {placedNames}");

            // Use the method to update objective text AND trigger UI refresh
            string newText = $"Place nametags at the table ({count}/{nametagQuest.nametagNames.Length})";
            nametagQuest.UpdateObjectiveText(0, newText);

            // Check if all nametags are placed
            bool isComplete = count >= nametagQuest.nametagNames.Length;
            Debug.Log($"Quest completion check: {count}/{nametagQuest.nametagNames.Length} placed, isComplete: {isComplete}");

            // Update objective completion state if needed
            if (isComplete && !nametagQuest.Objectives[0].isCompleted)
            {
                // Mark objective as complete
                nametagQuest.CompleteObjective(0);
                Debug.Log("All nametags placed! Quest completed.");
            }
            else if (!isComplete && nametagQuest.Objectives[0].isCompleted)
            {
                // We need to mark it as incomplete
                nametagQuest.Objectives[0].isCompleted = false;

                // Trigger UI refresh
                nametagQuest.UpdateObjectiveText(0, newText);
            }
        }
    }

    // Reset tracking (useful for testing or when restarting the quest)
    public void ResetTracking()
    {
        placedNametags.Clear();
        pendingNametags.Clear();

        // Reset the processed spots dictionary
        InitializeProcessedSpots();

        // Reset the quest state
        ResetQuestState();

        Debug.Log("Nametag tracking reset");
    }

    // Force quest completion (can be called from inspector or for testing)
    public void ForceCompleteQuest()
    {
        if (nametagQuest != null && QuestManager.Instance != null &&
            QuestManager.Instance.IsQuestActive(nametagQuest))
        {
            // Complete the objective
            nametagQuest.CompleteObjective(0);
            Debug.Log("Quest completion forced");
        }
    }

    private void OnDestroy()
    {
        UnregisterTableSpotEvents();
        StopTracking();
        Debug.Log("NametagQuestManager destroyed");

        // Clear the singleton reference if this is the current instance
        if (Instance == this)
        {
            Instance = null;
        }
    }
}