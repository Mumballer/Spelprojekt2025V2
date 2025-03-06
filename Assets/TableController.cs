using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableController : MonoBehaviour
{
    [Header("Nametag Settings")]
    [SerializeField] private List<Transform> nametagPlacementSpots = new List<Transform>();
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private AudioClip placementSound;
    [SerializeField] private bool hideNametagsOnStart = true;

    [Header("Quest Settings")]
    [SerializeField] private Quest nametagQuest;
    [SerializeField] private int startingObjectiveIndex = 0;
    [SerializeField] private bool incrementObjectiveIndex = true;
    [SerializeField] private bool requireCorrectNametagOrder = false;

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;

    private List<NameTag> placedNametags = new List<NameTag>();
    private AudioSource audioSource;
    private int totalNametagsPlaced = 0;
    private int objectiveIndex;

    void Awake()
    {
        // Initialize audio source if sound effects are used
        if (placementSound != null && audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Make sure table is on correct layer for detection
        if (gameObject.layer != LayerMask.NameToLayer("Table"))
        {
            DebugLog($"Setting layer of {gameObject.name} to 'Table'");
            gameObject.layer = LayerMask.NameToLayer("Table");
        }

        // Initially hide the interaction prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // Set initial objective index
        objectiveIndex = startingObjectiveIndex;

        ValidateSetup();
    }

    void Start()
    {
        // Perform additional checks after all objects are initialized
        if (hideNametagsOnStart)
        {
            // This can be useful if you have pre-placed nametags in the scene that should be hidden until placed
            foreach (Transform spot in nametagPlacementSpots)
            {
                // Check if there are any child nametags that should be hidden initially
                NameTag existingNameTag = spot.GetComponentInChildren<NameTag>(true);
                if (existingNameTag != null)
                {
                    existingNameTag.gameObject.SetActive(false);
                    DebugLog($"Hiding pre-placed nametag: {existingNameTag.name}");
                }
            }
        }
    }

    // Validate component setup
    private void ValidateSetup()
    {
        if (nametagPlacementSpots.Count == 0)
        {
            Debug.LogWarning($"TableController on {gameObject.name} has no nametag placement spots defined!");
        }

        if (nametagQuest != null && nametagQuest.Objectives.Count <= startingObjectiveIndex)
        {
            Debug.LogWarning($"Starting objective index ({startingObjectiveIndex}) is greater than the number of objectives in the quest ({nametagQuest.Objectives.Count})!");
        }
    }

    // Called by NametagManager when player is looking at the table
    public void ShowInteractionPrompt(bool show)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(show);
        }
    }

    // Check if a nametag can be placed on this table
    public bool CanPlaceNameTag()
    {
        // If all spots are full, return false
        return totalNametagsPlaced < nametagPlacementSpots.Count;
    }

    // Places a nametag on the next available spot
    public void PlaceNameTag(NameTag nametag)
    {
        if (nametag == null)
        {
            Debug.LogWarning("TableController: Attempted to place null nametag!");
            return;
        }

        if (!CanPlaceNameTag())
        {
            DebugLog($"TableController: Cannot place nametag - table is full ({totalNametagsPlaced}/{nametagPlacementSpots.Count})");
            return;
        }

        DebugLog($"TableController: Placing nametag '{nametag.GuestName}' at spot {totalNametagsPlaced}");

        // Find next available spot
        if (totalNametagsPlaced < nametagPlacementSpots.Count)
        {
            Transform spot = nametagPlacementSpots[totalNametagsPlaced];

            // Position the nametag
            nametag.transform.position = spot.position;
            nametag.transform.rotation = spot.rotation;

            // Add to our tracking list
            placedNametags.Add(nametag);
            totalNametagsPlaced++;

            // Play sound effect if available
            if (audioSource != null && placementSound != null)
            {
                audioSource.clip = placementSound;
                audioSource.Play();
            }

            // Update the quest objective for this nametag
            if (nametagQuest != null)
            {
                // Only update if we have a valid objective index
                if (objectiveIndex < nametagQuest.Objectives.Count)
                {
                    // Complete the specific objective for this nametag
                    DebugLog($"NAMETAG QUEST: Completing objective {objectiveIndex} of {nametagQuest.Objectives.Count} for quest '{nametagQuest.questName}'");
                    nametagQuest.CompleteObjective(objectiveIndex);

                    // Increment to the next objective if configured to do so
                    if (incrementObjectiveIndex)
                    {
                        objectiveIndex++;
                        DebugLog($"Incremented to next objective index: {objectiveIndex}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Objective index {objectiveIndex} is out of range! Quest only has {nametagQuest.Objectives.Count} objectives.");
                }

                // Log completion status - DO NOT call CompleteQuest directly
                if (totalNametagsPlaced == nametagPlacementSpots.Count)
                {
                    DebugLog($"All nametags placed! ({totalNametagsPlaced}/{nametagPlacementSpots.Count}) Quest completion will be handled by objectives system.");
                }
            }
            else
            {
                DebugLog("No nametag quest assigned to TableController.");
            }

            // Notify the NameTagManager that we placed this tag
            if (NameTagManager.Instance != null)
            {
                NameTagManager.Instance.NotifyNameTagPlaced();
            }
            else
            {
                Debug.LogWarning("NameTagManager.Instance is null! Cannot notify name tag placement.");
            }
        }
    }

    // Returns the total number of placement spots on this table
    public int GetTotalPlacementSpots()
    {
        return nametagPlacementSpots.Count;
    }

    // Returns how many nametags have been placed
    public int GetPlacedNametagCount()
    {
        return totalNametagsPlaced;
    }

    // Check if a specific nametag is allowed to be placed (for validation based on order)
    public bool CanPlaceSpecificNameTag(NameTag nametag)
    {
        if (!requireCorrectNametagOrder)
            return true;

        // Add custom logic here if you want specific nametags to go to specific spots
        // For example: return nametag.GuestName == expectedGuestNames[totalNametagsPlaced];

        return true;
    }

    // Get the next expected nametag name (if order matters)
    public string GetNextExpectedNameTag()
    {
        if (!requireCorrectNametagOrder || totalNametagsPlaced >= nametagPlacementSpots.Count)
            return string.Empty;

        // Add custom logic here if you have specific expected nametag order
        // For example: return expectedGuestNames[totalNametagsPlaced];

        return "Any nametag";
    }

    // For debugging purposes
    private void DebugLog(string message)
    {
        if (showDebugMessages)
        {
            Debug.Log($"[TableController:{gameObject.name}] {message}");
        }
    }

    // For editor debugging
    public void ResetTable()
    {
        totalNametagsPlaced = 0;
        placedNametags.Clear();
        objectiveIndex = startingObjectiveIndex;
        DebugLog("Table has been reset");
    }

    // For visual debugging of placement spots
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        if (nametagPlacementSpots != null)
        {
            foreach (Transform spot in nametagPlacementSpots)
            {
                if (spot != null)
                {
                    Gizmos.DrawWireSphere(spot.position, 0.05f);
                    Gizmos.DrawLine(transform.position, spot.position);
                }
            }
        }
    }
}