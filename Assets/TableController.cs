using System.Collections.Generic;
using UnityEngine;

public class TableController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private List<GameObject> tableNameTags = new List<GameObject>();
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private AudioClip placementSound;

    [Header("Quest Integration")]
    [SerializeField] private Quest relatedQuest;
    [SerializeField] private int objectiveIndex;

    private List<GameObject> availableNameTags = new List<GameObject>();
    private AudioSource audioSource;
    private int nameTagsPlaced = 0;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && placementSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Hide all nametags initially
        availableNameTags.Clear();
        foreach (var nameTag in tableNameTags)
        {
            if (nameTag != null)
            {
                nameTag.SetActive(false);
                availableNameTags.Add(nameTag);
            }
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // Make sure this object is on the Table layer
        int tableLayer = LayerMask.NameToLayer("Table");
        if (tableLayer >= 0)
        {
            gameObject.layer = tableLayer;
        }
        else
        {
            Debug.LogWarning("Table layer not found. Please create a layer named 'Table'");
        }
    }

    public void ShowInteractionPrompt(bool show)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(show && availableNameTags.Count > 0);
        }
    }

    public bool CanPlaceNameTag()
    {
        return availableNameTags.Count > 0;
    }

    public void PlaceNameTag(NameTag heldNameTag)
    {
        if (availableNameTags.Count == 0 || heldNameTag == null) return;

        // Get a random nametag from the available ones
        int randomIndex = Random.Range(0, availableNameTags.Count);
        GameObject tableNameTag = availableNameTags[randomIndex];

        // Remove it from the available list
        availableNameTags.RemoveAt(randomIndex);

        // Show the table nametag
        tableNameTag.SetActive(true);

        // Play sound
        if (audioSource != null && placementSound != null)
        {
            audioSource.PlayOneShot(placementSound);
        }

        // Update quest
        nameTagsPlaced++;
        if (relatedQuest != null)
        {
            relatedQuest.CompleteObjective(objectiveIndex);
            Debug.Log($"Completed objective {objectiveIndex} for quest {relatedQuest.questName}");

            if (nameTagsPlaced >= tableNameTags.Count)
            {
                relatedQuest.CompleteQuest();
                Debug.Log($"All nametags placed! Completed quest: {relatedQuest.questName}");
            }

            relatedQuest.CheckQuestCompletion();
        }

        // Notify manager
        if (NameTagManager.Instance != null)
        {
            NameTagManager.Instance.NotifyNameTagPlaced(heldNameTag);
            NameTagManager.Instance.UpdateProgress(nameTagsPlaced, tableNameTags.Count);
        }
    }
}