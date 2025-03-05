using UnityEngine;

public class ChairNameTagSpot : MonoBehaviour
{
    [Header("Chair Settings")]
    [SerializeField] private string chairOwner;
    [SerializeField] private Transform placementPoint;
    [SerializeField] private GameObject interactionPrompt;

    [Header("Quest Integration")]
    [SerializeField] private Quest relatedQuest;
    [SerializeField] private int objectiveIndex;

    private bool hasNameTag = false;
    private NameTag currentNameTag = null;

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
    }

    public void ShowInteractionPrompt(bool show)
    {
        if (interactionPrompt != null)
        {
            // Only show prompt if we don't already have a nametag
            if (show && !hasNameTag)
            {
                interactionPrompt.SetActive(true);
            }
            else
            {
                // Always hide when requested
                interactionPrompt.SetActive(false);
            }
        }
    }

    public bool TryPlaceNameTag(NameTag nameTag)
    {
        if (hasNameTag || nameTag == null) return false;

        // Place the nametag
        nameTag.Place(placementPoint);
        hasNameTag = true;
        currentNameTag = nameTag;

        // Hide the interaction prompt since we now have a nametag
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // Check if it's the correct nametag by comparing tags
        bool isCorrect = nameTag.gameObject.tag == gameObject.tag;

        if (isCorrect)
        {
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
            Debug.Log($"Incorrect nametag placed at {chairOwner}'s spot. Expected {gameObject.tag}, got {nameTag.gameObject.tag}");
        }

        return true;
    }

    public void RemoveNameTag()
    {
        if (!hasNameTag || currentNameTag == null) return;
        hasNameTag = false;
        currentNameTag = null;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a visual indicator in the editor
        Gizmos.color = Color.green;
        Vector3 position = placementPoint != null ? placementPoint.position : transform.position;
        Gizmos.DrawWireSphere(position, 0.1f);

#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(chairOwner))
        {
            UnityEditor.Handles.Label(position + Vector3.up * 0.2f, chairOwner);
        }
#endif
    }
}