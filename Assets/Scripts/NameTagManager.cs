using System.Collections.Generic;
using UnityEngine;
using System;

public class NameTagManager : MonoBehaviour
{
    public static NameTagManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private LayerMask interactableLayers;
    [SerializeField] private bool requireCorrectPlacement = true;
    [SerializeField] private bool allowRemovingPlacedTags = true;

    [Header("Quest Integration")]
    [SerializeField] private Quest nameTagQuest;
    [SerializeField] private bool completeQuestWhenAllPlaced = true;

    [Header("UI")]
    [SerializeField] private GameObject nameTagHintUI;

    // Events
    public event Action<NameTag> OnNameTagPickup;
    public event Action<ChairNameTagSpot, NameTag, bool> OnNameTagPlaced;
    public event Action<int, int> OnProgressUpdated; // current, total

    private List<NameTag> allNameTags = new List<NameTag>();
    private List<ChairNameTagSpot> allChairSpots = new List<ChairNameTagSpot>();
    private NameTag currentlyHeldNameTag = null;
    private Camera mainCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        mainCamera = Camera.main;
    }

    private void Start()
    {
        // Find all nametags and chair spots in the scene using the non-deprecated method
        allNameTags.AddRange(FindObjectsByType<NameTag>(FindObjectsSortMode.None));
        allChairSpots.AddRange(FindObjectsByType<ChairNameTagSpot>(FindObjectsSortMode.None));

        Debug.Log($"Found {allNameTags.Count} nametags and {allChairSpots.Count} chair spots");

        // Show/hide the hint UI
        if (nameTagHintUI != null)
        {
            nameTagHintUI.SetActive(false);
        }
    }

    private void Update()
    {
        // Don't process interactions if dialog is active
        if (DialogManager.Instance != null && DialogManager.Instance.IsDialogActive)
            return;

        HandleInteraction();
        UpdatePrompts();

        // Show/hide hint UI based on whether we're holding a nametag
        if (nameTagHintUI != null)
        {
            nameTagHintUI.SetActive(currentlyHeldNameTag != null);
        }
    }

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentlyHeldNameTag != null)
            {
                // Try to place the held nametag
                TryPlaceHeldNameTag();
            }
            else
            {
                // Try to pick up a nametag
                TryPickupNameTag();
            }
        }
    }

    private void TryPickupNameTag()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayers))
        {
            NameTag nameTag = hit.collider.GetComponent<NameTag>();
            if (nameTag != null && !nameTag.IsPickedUp)
            {
                nameTag.PickUp();
                currentlyHeldNameTag = nameTag;
                OnNameTagPickup?.Invoke(nameTag);
            }
        }
    }

    private void TryPlaceHeldNameTag()
    {
        if (mainCamera == null || currentlyHeldNameTag == null) return;

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayers))
        {
            ChairNameTagSpot chairSpot = hit.collider.GetComponent<ChairNameTagSpot>();
            if (chairSpot != null && !chairSpot.HasNameTag)
            {
                bool placed = chairSpot.TryPlaceNameTag(currentlyHeldNameTag);
                if (placed)
                {
                    currentlyHeldNameTag = null;
                    UpdateQuestProgress();
                }
            }
        }
        else
        {
            // If not pointing at a chair, drop the nametag back at its original position
            currentlyHeldNameTag.ReturnToOriginalPosition();
            currentlyHeldNameTag = null;
        }
    }

    private void UpdatePrompts()
    {
        if (mainCamera == null) return;

        // Hide all prompts first
        foreach (var nameTag in allNameTags)
        {
            if (nameTag != null)
                nameTag.ShowInteractionPrompt(false);
        }

        foreach (var chairSpot in allChairSpots)
        {
            if (chairSpot != null)
                chairSpot.ShowInteractionPrompt(false);
        }

        // If holding a nametag, show prompts for chairs
        if (currentlyHeldNameTag != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayers))
            {
                ChairNameTagSpot chairSpot = hit.collider.GetComponent<ChairNameTagSpot>();
                if (chairSpot != null && !chairSpot.HasNameTag)
                {
                    chairSpot.ShowInteractionPrompt(true);
                }
            }
        }
        else
        {
            // If not holding a nametag, show prompts for nametags
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayers))
            {
                NameTag nameTag = hit.collider.GetComponent<NameTag>();
                if (nameTag != null && !nameTag.IsPickedUp)
                {
                    nameTag.ShowInteractionPrompt(true);
                }
            }
        }
    }

    public void OnNameTagPickedUp(NameTag nameTag)
    {
        // This is called by the NameTag when it's picked up
        OnNameTagPickup?.Invoke(nameTag);
    }

    public void OnCorrectNameTagPlaced(ChairNameTagSpot chairSpot, NameTag nameTag)
    {
        // This is called by the ChairNameTagSpot when a correct nametag is placed
        OnNameTagPlaced?.Invoke(chairSpot, nameTag, true);
        UpdateQuestProgress();
    }

    public void OnIncorrectNameTagPlaced(ChairNameTagSpot chairSpot, NameTag nameTag)
    {
        // This is called by the ChairNameTagSpot when an incorrect nametag is placed
        OnNameTagPlaced?.Invoke(chairSpot, nameTag, false);

        // If we require correct placement, return the nametag to its original position
        if (requireCorrectPlacement)
        {
            nameTag.ReturnToOriginalPosition();
            chairSpot.RemoveNameTag();
        }
    }

    private void UpdateQuestProgress()
    {
        // Count correctly placed nametags
        int correctlyPlaced = 0;
        int total = allChairSpots.Count;

        foreach (var chairSpot in allChairSpots)
        {
            if (chairSpot != null && chairSpot.HasNameTag)
            {
                correctlyPlaced++;
            }
        }

        OnProgressUpdated?.Invoke(correctlyPlaced, total);

        // Check if all nametags are placed correctly
        if (correctlyPlaced == total && completeQuestWhenAllPlaced && nameTagQuest != null)
        {
            // Complete the entire quest using the Quest ScriptableObject
            nameTagQuest.CompleteQuest();
            Debug.Log($"All nametags placed correctly! Completed quest: {nameTagQuest.questName}");

            // If you have a QuestManager, you can notify it here
            if (QuestManager.Instance != null)
            {
                // Instead of directly accessing the event, call a public method
                QuestManager.Instance.NotifyQuestCompleted(nameTagQuest);
            }
        }
    }
}