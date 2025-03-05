using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class NameTagManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerHoldPoint;
    [SerializeField] private LayerMask nametagLayer;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Settings")]
    [SerializeField] private bool autoPickUpCorrectNametags = false;

    [Header("Quest Integration")]
    [SerializeField] private Quest relatedQuest;
    [SerializeField] private int objectiveIndex;
    [SerializeField] private bool completeQuestWhenAllPlaced = false;

    // Events for UI
    public event Action<int, int> OnProgressUpdated;    // Progress (current, total)
    public event Action<NameTag> OnNameTagPickup;       // When player picks up a nametag
    public event Action<ChairNameTagSpot, NameTag, bool> OnNameTagPlaced;  // When nametag is placed (spot, tag, isCorrect)

    private NameTag currentNameTag = null;
    private bool canInteract = true;
    private Camera mainCamera;
    private List<ChairNameTagSpot> allChairSpots = new List<ChairNameTagSpot>();
    private int correctPlacementCount = 0;
    private ChairNameTagSpot currentlyLookingAt = null;

    public static NameTagManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        mainCamera = Camera.main;

        if (playerHoldPoint == null)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                Transform cameraTransform = player.transform.Find("Camera") ?? mainCamera?.transform;
                GameObject holdPoint = new GameObject("NameTagHoldPoint");
                playerHoldPoint = holdPoint.transform;
                playerHoldPoint.SetParent(cameraTransform);
                playerHoldPoint.localPosition = new Vector3(0, 0, 0.5f);
            }
        }

        // Find all chair spots in the scene
        ChairNameTagSpot[] spots = FindObjectsOfType<ChairNameTagSpot>();
        allChairSpots.AddRange(spots);
    }

    private void Start()
    {
        // Make sure all prompts are hidden initially
        HideAllInteractionPrompts();
    }

    private void Update()
    {
        if (!canInteract) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (currentNameTag == null)
            {
                TryPickUpNameTag();
            }
            else
            {
                TryPlaceNameTag();
            }
        }

        // Only update look target if we're holding a nametag
        if (currentNameTag != null)
        {
            UpdateLookTarget();
        }
        else if (currentlyLookingAt != null)
        {
            // If we're not holding a nametag but still have a cached reference, clear it
            currentlyLookingAt.ShowInteractionPrompt(false);
            currentlyLookingAt = null;
        }
    }

    // Method to update what we're looking at
    private void UpdateLookTarget()
    {
        if (mainCamera == null) return;

        // Hide the previous target's prompt
        if (currentlyLookingAt != null)
        {
            currentlyLookingAt.ShowInteractionPrompt(false);
            currentlyLookingAt = null;
        }

        // Only continue if we're holding a nametag
        if (currentNameTag == null) return;

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            ChairNameTagSpot spot = hit.collider.GetComponent<ChairNameTagSpot>();

            if (spot != null && !spot.HasNameTag)
            {
                // We're looking at a valid chair spot
                currentlyLookingAt = spot;
                spot.ShowInteractionPrompt(true);
            }
        }
    }

    private void TryPickUpNameTag()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, nametagLayer))
        {
            NameTag nameTag = hit.collider.GetComponent<NameTag>();
            if (nameTag != null && !nameTag.IsPickedUp)
            {
                // Pick up the nametag
                nameTag.PickUp(playerHoldPoint);
                currentNameTag = nameTag;

                // Notify listeners
                NotifyNameTagPickup(nameTag);
            }
        }
    }

    private void TryPlaceNameTag()
    {
        if (mainCamera == null || currentNameTag == null) return;

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            ChairNameTagSpot spot = hit.collider.GetComponent<ChairNameTagSpot>();

            if (spot != null && !spot.HasNameTag)
            {
                bool success = spot.TryPlaceNameTag(currentNameTag);
                if (success)
                {
                    // Check if it's the correct spot (matching tags)
                    bool isCorrect = currentNameTag.gameObject.tag == spot.gameObject.tag;

                    // Trigger UI event
                    OnNameTagPlaced?.Invoke(spot, currentNameTag, isCorrect);

                    // Hide all chair interaction prompts
                    HideAllInteractionPrompts();

                    currentNameTag = null;
                }
            }
            else
            {
                // Player tried to place it somewhere invalid - return to original position
                currentNameTag.Reset();
                currentNameTag = null;

                // Hide all interaction prompts
                HideAllInteractionPrompts();
            }
        }
        else
        {
            // Player tried to place it in thin air - return to original position
            currentNameTag.Reset();
            currentNameTag = null;

            // Hide all interaction prompts
            HideAllInteractionPrompts();
        }
    }

    // Helper method to hide all interaction prompts
    private void HideAllInteractionPrompts()
    {
        // Hide all interaction prompts for chair spots
        foreach (var chairSpot in allChairSpots)
        {
            chairSpot.ShowInteractionPrompt(false);
        }

        // Reset the currently looking at reference
        currentlyLookingAt = null;
    }

    public void OnCorrectNameTagPlaced(ChairNameTagSpot spot, NameTag nameTag)
    {
        correctPlacementCount++;
        Debug.Log($"Correct placement! {correctPlacementCount}/{allChairSpots.Count} placed correctly");

        // Update progress
        OnProgressUpdated?.Invoke(correctPlacementCount, allChairSpots.Count);

        // Hide all interaction prompts
        HideAllInteractionPrompts();

        // Check if all nametags are correctly placed
        if (completeQuestWhenAllPlaced && correctPlacementCount >= allChairSpots.Count)
        {
            Debug.Log("All nametags correctly placed!");

            if (relatedQuest != null)
            {
                relatedQuest.CompleteObjective(objectiveIndex);

                // Optionally complete the entire quest
                if (completeQuestWhenAllPlaced)
                {
                    relatedQuest.CompleteQuest();
                }
            }
        }
    }

    public void OnIncorrectNameTagPlaced(ChairNameTagSpot spot, NameTag nameTag)
    {
        Debug.Log("Incorrect nametag placement!");

        // Notify UI but keep the nametag where it is
        OnNameTagPlaced?.Invoke(spot, nameTag, false);

        // Hide all interaction prompts
        HideAllInteractionPrompts();

        // Make sure we can interact immediately 
        canInteract = true;
    }

    // Method to allow other classes to notify about nametag pickup
    public void NotifyNameTagPickup(NameTag nameTag)
    {
        // Invoke the event from within the NameTagManager class
        OnNameTagPickup?.Invoke(nameTag);
    }
}