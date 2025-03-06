using UnityEngine;
using System.Collections;

public class GramophoneQuestTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Gramophone gramophone;
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Quest Settings")]
    [SerializeField] private Quest musicQuest;
    [SerializeField] private int musicObjectiveIndex = 0;

    private bool playerInRange = false;
    private bool canInteract = true;
    private Camera mainCamera;
    private bool questCompleted = false;

    private void Start()
    {
        mainCamera = Camera.main;

        if (gramophone == null)
        {
            gramophone = GetComponent<Gramophone>();
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
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
                interactionPrompt.SetActive(playerInRange);
            }
        }
    }

    public void Interact()
    {
        // Toggle music on the gramophone
        if (gramophone != null)
        {
            gramophone.ToggleMusic();

            // Complete the quest when the gramophone is interacted with
            if (musicQuest != null && QuestManager.Instance != null && !questCompleted)
            {
                // If quest is not active yet, add it
                if (!QuestManager.Instance.IsQuestActive(musicQuest) &&
                    !QuestManager.Instance.IsQuestCompleted(musicQuest))
                {
                    Debug.Log($"Activated gramophone quest: {musicQuest.questName}");
                    QuestManager.Instance.AddQuest(musicQuest);
                }

                // Complete the objective
                if (QuestManager.Instance.IsQuestActive(musicQuest))
                {
                    Debug.Log($"Completing gramophone interaction objective");
                    QuestManager.Instance.CompleteObjective(musicQuest, musicObjectiveIndex);
                    questCompleted = true;
                }
            }
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