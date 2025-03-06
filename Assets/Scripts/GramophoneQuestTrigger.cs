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
    [SerializeField] private int startMusicObjectiveIndex = 0;
    [SerializeField] private int stopMusicObjectiveIndex = 1;

    private bool playerInRange = false;
    private bool canInteract = true;
    private Camera mainCamera;
    private bool musicQuestActivated = false;

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

            // MUSIC QUEST ONLY - don't affect other quests
            if (musicQuest != null && QuestManager.Instance != null)
            {
                // First time interacting with gramophone, add the quest
                if (!musicQuestActivated && !QuestManager.Instance.IsQuestActive(musicQuest) &&
                    !QuestManager.Instance.GetCompletedQuests().Contains(musicQuest))
                {
                    Debug.Log($"Activated music quest: {musicQuest.questName}");
                    QuestManager.Instance.AddQuest(musicQuest);
                    musicQuestActivated = true;
                }

                // Complete specific objectives based on gramophone state
                if (gramophone.IsPlaying && QuestManager.Instance.IsQuestActive(musicQuest))
                {
                    Debug.Log($"Music quest only: Completing start music objective");
                    QuestManager.Instance.CompleteObjective(musicQuest, startMusicObjectiveIndex);
                }
                else if (!gramophone.IsPlaying && QuestManager.Instance.IsQuestActive(musicQuest))
                {
                    Debug.Log($"Music quest only: Completing stop music objective");
                    QuestManager.Instance.CompleteObjective(musicQuest, stopMusicObjectiveIndex);
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