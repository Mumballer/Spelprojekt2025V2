using UnityEngine;

public class ChairNameTagSpot : MonoBehaviour
{
    [Header("Chair Settings")]
    [SerializeField] private string chairOwner;
    [SerializeField] private string tagID; // This should match the nametag's tagID
    [SerializeField] private Transform placementPoint;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private GameObject correctPlacementEffect;

    [Header("Quest Integration")]
    [SerializeField] private Quest relatedQuest;
    [SerializeField] private int objectiveIndex;

    private bool hasNameTag = false;
    private NameTag currentNameTag = null;

    public string TagID => tagID;
    public bool HasNameTag => hasNameTag;
    public string ChairOwner => chairOwner;

    private void Start()
    {
        if (placementPoint == null)
        {
            placementPoint = transform;
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        if (correctPlacementEffect != null)
        {
            correctPlacementEffect.SetActive(false);
        }
    }

    public void ShowInteractionPrompt(bool show)
    {
        if (interactionPrompt != null && !hasNameTag)
        {
            interactionPrompt.SetActive(show);
        }
    }

    public bool TryPlaceNameTag(NameTag nameTag)
    {
        if (hasNameTag || nameTag == null) return false;

        // Place the nametag
        nameTag.Place(placementPoint);
        hasNameTag = true;
        currentNameTag = nameTag;

        // Check if it's the correct nametag
        bool isCorrect = nameTag.TagID == tagID;

        if (isCorrect)
        {
            // Show success effect
            if (correctPlacementEffect != null)
            {
                correctPlacementEffect.SetActive(true);
            }

            // Complete quest objective if applicable
            if (relatedQuest != null)
            {
                relatedQuest.CompleteObjective(objectiveIndex);
                Debug.Log($"Completed objective {objectiveIndex} for quest {relatedQuest.questName}");
            }

            // Notify the manager
            NameTagManager.Instance?.OnCorrectNameTagPlaced(this, nameTag);
        }
        else
        {
            // Notify the manager of incorrect placement
            NameTagManager.Instance?.OnIncorrectNameTagPlaced(this, nameTag);

            // You could add a hint or feedback here
            Debug.Log($"Incorrect nametag placed at {chairOwner}'s spot. Expected {tagID}, got {nameTag.TagID}");
        }

        return true;
    }

    public void RemoveNameTag()
    {
        if (!hasNameTag || currentNameTag == null) return;

        hasNameTag = false;

        if (correctPlacementEffect != null)
        {
            correctPlacementEffect.SetActive(false);
        }

        currentNameTag = null;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a visual indicator in the editor
        Gizmos.color = Color.green;
        Vector3 position = placementPoint != null ? placementPoint.position : transform.position;
        Gizmos.DrawWireSphere(position, 0.1f);

        // Draw text for the chair owner
#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(chairOwner))
        {
            UnityEditor.Handles.Label(position + Vector3.up * 0.2f, chairOwner);
        }
#endif
    }
}