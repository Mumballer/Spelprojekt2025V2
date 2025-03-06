using UnityEngine;
using System.Collections;

public class NameTagQuestGiver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Quest Settings")]
    [SerializeField] private Quest quest;
    [SerializeField] private Dialog questDialog;
    [SerializeField] private bool giveQuestAfterDialog = true;

    private bool playerInRange = false;
    private bool canInteract = true;
    private bool hasGivenQuest = false;

    private void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (hasGivenQuest) return;

        CheckPlayerDistance();

        if (playerInRange && canInteract && Input.GetKeyDown(interactKey))
        {
            Interact();
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
                interactionPrompt.SetActive(playerInRange && !hasGivenQuest);
            }
        }
    }

    public void Interact()
    {
        if (hasGivenQuest) return;

        // Start dialog - use your actual method name
        if (questDialog != null && DialogManager.Instance != null)
        {
            // Try different method name - your actual implementation might differ
            DialogManager.Instance.ShowDialog(questDialog);

            // If dialog is handled differently in your system, give quest immediately
            if (!giveQuestAfterDialog)
            {
                GiveQuestToPlayer();
            }
            // For quest giving after dialog, we'll rely on the DialogTrigger to handle it
            // Your dialog system likely has its own completion mechanism
        }
        else
        {
            // No dialog, just give quest immediately
            GiveQuestToPlayer();
        }

        // Hide prompt after interaction
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    // Called by your DialogManager (if it supports callbacks) or by other means
    public void OnDialogComplete()
    {
        // Give quest after dialog completes
        if (giveQuestAfterDialog)
        {
            GiveQuestToPlayer();
        }
    }

    private void GiveQuestToPlayer()
    {
        // Give quest
        if (quest != null && QuestManager.Instance != null && !hasGivenQuest)
        {
            QuestManager.Instance.AddQuest(quest);
            hasGivenQuest = true;
            Debug.Log($"NameTagQuestGiver: Gave quest to player: {quest.questName}");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (interactionPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(interactionPoint.position, interactionDistance);
        }
    }
}