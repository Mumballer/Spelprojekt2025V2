using UnityEngine;
using System;
using System.Collections.Generic;

public class NameTagManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerHoldPoint;
    [SerializeField] private LayerMask nametagLayer;
    [SerializeField] private LayerMask tableLayer;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Quest Integration")]
    [SerializeField] private Quest relatedQuest;
    [SerializeField] private int objectiveIndex;

    // Events
    public event Action<int, int> OnProgressUpdated;
    public event Action<NameTag> OnNameTagPickup;
    public event Action<NameTag> OnNameTagPlaced;

    private NameTag currentNameTag = null;
    private bool canInteract = true;
    private Camera mainCamera;
    private List<NameTag> allNameTags = new List<NameTag>();
    private TableController currentTable = null;
    private NametagCounter nametagCounter; // Reference to your counter

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
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                Transform cameraTransform = player.transform.Find("Camera") ?? mainCamera?.transform;
                GameObject holdPoint = new GameObject("NameTagHoldPoint");
                playerHoldPoint = holdPoint.transform;
                playerHoldPoint.SetParent(cameraTransform);
                playerHoldPoint.localPosition = new Vector3(0, 0, 0.5f);
            }
        }

        NameTag[] nameTags = FindObjectsByType<NameTag>(FindObjectsSortMode.None);
        allNameTags.AddRange(nameTags);

        // Find the nametag counter
        nametagCounter = FindFirstObjectByType<NametagCounter>();

        Debug.Log($"NameTagManager initialized. Found {allNameTags.Count} nametags.");
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

        UpdateTableInteraction();
    }

    private void UpdateTableInteraction()
    {
        if (mainCamera == null || currentNameTag == null) return;

        // Hide previous table prompt
        if (currentTable != null)
        {
            currentTable.ShowInteractionPrompt(false);
            currentTable = null;
        }

        // Check if looking at table
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, tableLayer))
        {
            TableController table = hit.collider.GetComponent<TableController>();
            if (table != null && table.CanPlaceNameTag())
            {
                table.ShowInteractionPrompt(true);
                currentTable = table;
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
                Debug.Log($"Picking up nametag: {nameTag.GuestName}");
                nameTag.PickUp(playerHoldPoint);
                currentNameTag = nameTag;
                NotifyNameTagPickup(nameTag);
            }
        }
    }

    private void TryPlaceNameTag()
    {
        if (mainCamera == null || currentNameTag == null) return;

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        Debug.Log("Attempting to place nametag...");

        if (Physics.Raycast(ray, out hit, interactionDistance, tableLayer))
        {
            Debug.Log($"Hit object: {hit.collider.gameObject.name}");

            TableController table = hit.collider.GetComponent<TableController>();
            if (table != null && table.CanPlaceNameTag())
            {
                Debug.Log("Found table, placing nametag");

                // Place the nametag on the table
                table.PlaceNameTag(currentNameTag);

                // Hide the original nametag
                currentNameTag.gameObject.SetActive(false);

                // Clear references
                currentNameTag = null;

                // Hide the prompt
                table.ShowInteractionPrompt(false);
            }
        }
        else
        {
            Debug.Log("Raycast didn't hit a table");
        }
    }

    public void NotifyNameTagPickup(NameTag nameTag)
    {
        OnNameTagPickup?.Invoke(nameTag);
    }

    public void NotifyNameTagPlaced(NameTag nameTag)
    {
        // Invoke the event
        OnNameTagPlaced?.Invoke(nameTag);

        // Update the counter directly
        if (nametagCounter != null)
        {
            nametagCounter.IncrementNametagCount();
        }
    }

    // Overloaded method with no parameters
    public void NotifyNameTagPlaced()
    {
        // Update the counter directly without calling the event
        if (nametagCounter != null)
        {
            nametagCounter.IncrementNametagCount();
        }
    }

    public void UpdateProgress(int current, int total)
    {
        OnProgressUpdated?.Invoke(current, total);
    }
}