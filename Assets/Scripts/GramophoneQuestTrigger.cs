using UnityEngine;
using System.Collections;

public class GramophoneQuestTrigger : MonoBehaviour
{
    [Header("References")]
    // referens till grammofon
    [SerializeField] private Gramophone gramophone;
    // punkt för interaktion
    [SerializeField] private Transform interactionPoint;
    // visuell interaktionsuppmaning
    [SerializeField] private GameObject interactionPrompt;
    // avstånd för interaktion
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private LayerMask playerLayer;
    // knapp för interaktion
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Quest Settings")]
    // uppdrag att aktivera
    [SerializeField] private Quest musicQuest;
    // mål att slutföra
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

        // hantera interaktion
        if (playerInRange && canInteract && Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    private void CheckPlayerDistance()
    {
        if (interactionPoint == null) return;

        // upptäck spelare nära
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
        // växla musik status
        if (gramophone != null)
        {
            gramophone.ToggleMusic();

            // slutför vid interaktion
            if (musicQuest != null && QuestManager.Instance != null && !questCompleted)
            {
                // aktivera uppdrag om inaktivt
                if (!QuestManager.Instance.IsQuestActive(musicQuest) &&
                    !QuestManager.Instance.IsQuestCompleted(musicQuest))
                {
                    Debug.Log($"Activated gramophone quest: {musicQuest.questName}");
                    QuestManager.Instance.AddQuest(musicQuest);
                }

                // slutför del mål
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