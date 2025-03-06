using UnityEngine;
using System.Collections;

public class QuestTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Quest Settings")]
    [SerializeField] private Quest quest;
    [SerializeField] private int objectiveIndex = -1; // -1 means no objective (just activate quest)
    [SerializeField] private bool autoCompleteQuest = false;
    [SerializeField] private bool oneTimeInteraction = true;

    private bool playerInRange = false;
    private bool canInteract = true;
    private bool hasInteracted = false;

    private void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (oneTimeInteraction && hasInteracted) return;

        CheckPlayerDistance();

        if (playerInRange && canInteract && Input.GetKeyDown(interactKey))
        {
            Interact();
            if (oneTimeInteraction) hasInteracted = true;
        }
    }

    private void CheckPlayerDistance()
    {
        if (interactionPoint == null) return;

        Collider[] colliders = Physics.OverlapSphere(interactionPoint.position, interactionDistance, playerLayer);
        bool isPlayerNear = colliders.Length > 0;

        if (isPlayerNear != playerInRange)
        {
            playerInRange = isPlayerNear;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(playerInRange);
            }
        }
    }

    public virtual void Interact()
    {
        // If set to activate a quest, do so
        if (quest != null)
        {
            TryActivateQuest();
        }

        // If set to complete an objective, do so
        if (objectiveIndex >= 0)
        {
            CompleteObjective();
        }

        // If set to auto-complete the entire quest, do so
        if (autoCompleteQuest)
        {
            CompleteQuest();
        }
    }

    private void TryActivateQuest()
    {
        // Only activate if:
        // 1. Quest exists
        // 2. Quest is not already active
        // 3. Quest is not already completed
        if (quest != null && QuestManager.Instance != null &&
            !QuestManager.Instance.IsQuestActive(quest) &&
            !QuestManager.Instance.GetCompletedQuests().Contains(quest))
        {
            Debug.Log($"QuestTrigger: Activating quest: {quest.questName}");
            QuestManager.Instance.AddQuest(quest);
        }
    }

    private void CompleteObjective()
    {
        // Only complete if:
        // 1. Quest exists
        // 2. Quest is active
        // 3. Objective index is valid
        if (quest != null && QuestManager.Instance != null &&
            QuestManager.Instance.IsQuestActive(quest) && objectiveIndex >= 0)
        {
            Debug.Log($"QuestTrigger: Completing objective {objectiveIndex} for quest: {quest.questName}");
            QuestManager.Instance.CompleteObjective(quest, objectiveIndex);
        }
    }

    private void CompleteQuest()
    {
        if (quest != null && QuestManager.Instance != null &&
            QuestManager.Instance.IsQuestActive(quest))
        {
            Debug.Log($"QuestTrigger: Auto-completing quest: {quest.questName}");
            QuestManager.Instance.CompleteQuest(quest);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (interactionPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(interactionPoint.position, interactionDistance);
        }
    }
}