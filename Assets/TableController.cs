using System.Collections.Generic;
using UnityEngine;

public class TableController : MonoBehaviour
{
    [Header("Quest Integration")]
    [SerializeField] private Quest relatedQuest;
    [SerializeField] private bool debugMode = true;

    [Header("Nametag Settings")]
    [SerializeField] private NameTagSpot[] nametagSpots;
    [SerializeField] private string[] expectedGuests;
    [SerializeField] private float checkingInterval = 0.5f;
    [SerializeField] private bool requireCorrectPositions = true;

    [Header("Interaction Settings")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private Transform promptPosition;

    [Header("Audio")]
    [SerializeField] private AudioClip correctPlacementSound;
    [SerializeField] private AudioClip allCorrectSound;
    [SerializeField] private AudioSource audioSource;

    // Tracking variables
    private int filledSpots = 0;
    private float timeSinceLastCheck = 0f;
    private bool questCompleted = false;
    private List<string> placedNameTags = new List<string>();
    private NameTag currentSelectedNameTag;

    private void Start()
    {
        // Make sure we have audio source if audio clips were provided
        if (audioSource == null && (correctPlacementSound != null || allCorrectSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (nametagSpots == null || nametagSpots.Length == 0)
        {
            // Try to find nametag spots if none were assigned
            nametagSpots = GetComponentsInChildren<NameTagSpot>();
            if (nametagSpots.Length == 0)
            {
                Debug.LogError("[TableController] No nametag spots found or assigned!");
            }
        }

        // Make sure we have the correct number of expected guests
        if (expectedGuests == null || expectedGuests.Length == 0)
        {
            if (debugMode) Debug.LogWarning("[TableController] No expected guests specified.");
            expectedGuests = new string[nametagSpots.Length];
            for (int i = 0; i < expectedGuests.Length; i++)
            {
                expectedGuests[i] = "Guest " + (i + 1);
            }
        }
        else if (expectedGuests.Length != nametagSpots.Length)
        {
            Debug.LogWarning($"[TableController] Mismatch between expected guests ({expectedGuests.Length}) and nametag spots ({nametagSpots.Length})");
        }

        // Initialize spots with their expected guests
        for (int i = 0; i < nametagSpots.Length && i < expectedGuests.Length; i++)
        {
            if (nametagSpots[i] != null)
            {
                nametagSpots[i].Initialize(expectedGuests[i], this);
                if (debugMode) Debug.Log($"[TableController] Spot {i} initialized for guest: {expectedGuests[i]}");
            }
        }

        // Hide interaction prompt at start
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        DebugLog("TableController initialized with " + nametagSpots.Length + " nametag spots");
    }

    private void Update()
    {
        // Periodically check if all nametags are placed correctly
        timeSinceLastCheck += Time.deltaTime;
        if (timeSinceLastCheck >= checkingInterval)
        {
            CheckAllNameTags();
            timeSinceLastCheck = 0f;
        }
    }

    private void CheckAllNameTags()
    {
        if (questCompleted) return;

        int correctlyPlaced = 0;
        filledSpots = 0;

        foreach (var spot in nametagSpots)
        {
            if (spot == null) continue;

            if (spot.HasNameTag)
            {
                filledSpots++;
                if (!requireCorrectPositions || spot.IsCorrectNameTag)
                {
                    correctlyPlaced++;
                }
            }
        }

        // Check if all spots are filled with correct nametags
        bool allPlaced = requireCorrectPositions ?
            (correctlyPlaced == nametagSpots.Length) :
            (filledSpots == nametagSpots.Length);

        if (allPlaced && !questCompleted)
        {
            DebugLog("All nametags placed! (" + filledSpots + "/" + nametagSpots.Length + ")");

            // Play sound effect
            if (audioSource != null && allCorrectSound != null)
            {
                audioSource.clip = allCorrectSound;
                audioSource.Play();
            }

            // Complete the quest
            CompleteNametagQuest();
        }
    }

    // Called when a nametag is placed on a spot
    public void OnNameTagPlaced(NameTagSpot spot, string guestName)
    {
        if (debugMode)
        {
            DebugLog($"Nametag placed: {guestName} at spot expecting {spot.ExpectedGuest}");
        }

        if (!placedNameTags.Contains(guestName))
        {
            placedNameTags.Add(guestName);
        }

        // Play correct placement sound if this is the right spot
        if (spot.IsCorrectNameTag && audioSource != null && correctPlacementSound != null)
        {
            audioSource.clip = correctPlacementSound;
            audioSource.Play();
        }

        // Check all nametags right away when a new one is placed
        CheckAllNameTags();
    }

    // Called when a nametag is removed from a spot
    public void OnNameTagRemoved(NameTagSpot spot, string guestName)
    {
        if (debugMode)
        {
            DebugLog($"Nametag removed: {guestName}");
        }

        placedNameTags.Remove(guestName);
    }

    // Completes the nametag quest
    private void CompleteNametagQuest()
    {
        if (questCompleted) return;

        questCompleted = true;

        if (relatedQuest == null)
        {
            Debug.LogWarning("[TableController] No quest assigned!");
            return;
        }

        DebugLog("Completing nametag quest...");

        // FIXED: Complete the quest using QuestManager
        if (QuestManager.Instance != null)
        {
            // Complete all objectives and the quest
            for (int i = 0; i < relatedQuest.Objectives.Count; i++)
            {
                QuestManager.Instance.CompleteObjective(relatedQuest, i);
            }

            // Ensure the quest is fully completed
            QuestManager.Instance.CompleteQuest(relatedQuest);

            DebugLog($"Completed quest '{relatedQuest.questName}' via QuestManager");
        }
        else
        {
            Debug.LogError("[TableController] QuestManager instance not found!");
        }
    }

    // Check if a nametag can be placed on the table
    public bool CanPlaceNameTag(NameTag nameTag = null)
    {
        // Find an empty spot
        foreach (var spot in nametagSpots)
        {
            if (spot != null && !spot.HasNameTag)
            {
                return true;
            }
        }
        return false;
    }

    // Place a nametag on the table
    public bool PlaceNameTag(NameTag nameTag, Vector3 playerPosition)
    {
        if (nameTag == null) return false;

        // Find the closest empty spot
        NameTagSpot closestSpot = null;
        float closestDistance = float.MaxValue;

        foreach (var spot in nametagSpots)
        {
            if (spot != null && !spot.HasNameTag)
            {
                float distance = Vector3.Distance(spot.transform.position, playerPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSpot = spot;
                }
            }
        }

        if (closestSpot != null && closestSpot.snapPoint != null)
        {
            // Place the nametag at the spot
            nameTag.PlaceOnTable(closestSpot.snapPoint.position, closestSpot.snapPoint.rotation);
            return true;
        }

        return false;
    }

    // Show/hide the interaction prompt
    public void ShowInteractionPrompt(bool show)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(show);

            // Position the prompt if it's being shown
            if (show && promptPosition != null)
            {
                interactionPrompt.transform.position = promptPosition.position;
            }
        }
    }

    // Debug helper
    private void DebugLog(string message)
    {
        if (debugMode)
        {
            Debug.Log("[TableController] " + message);
        }
    }

    // Public method to force quest completion (can be called from inspector for testing)
    public void ForceCompleteQuest()
    {
        if (!questCompleted)
        {
            DebugLog("Force completing nametag quest!");
            CompleteNametagQuest();
        }
    }
}