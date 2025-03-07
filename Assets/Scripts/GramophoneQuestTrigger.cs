using UnityEngine;
using System.Collections;

public class GramophoneQuestTrigger : MonoBehaviour
{
    [Header("References")]
    // referens till grammofon
    [SerializeField] private Gramophone gramophone;
    // punkt f�r interaktion
    [SerializeField] private Transform interactionPoint;
    // visuell interaktionsuppmaning
    [SerializeField] private GameObject interactionPrompt;
    // avst�nd f�r interaktion
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private LayerMask playerLayer;
    // knapp f�r interaktion
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Quest Settings")]
    // uppdrag att aktivera
    [SerializeField] private Quest musicQuest;
    // m�l att slutf�ra
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

        // uppt�ck spelare n�ra
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
        // v�xla musik status
        if (gramophone != null)
        {
            gramophone.ToggleMusic();

            // slutf�r vid interaktion
            if (musicQuest != null && QuestManager.Instance != null && !questCompleted)
            {
                // aktivera uppdrag om inaktivt
                if (!QuestManager.Instance.IsQuestActive(musicQuest) &&
                    !QuestManager.Instance.IsQuestCompleted(musicQuest))
                {
                    Debug.Log($"Activated gramophone quest: {musicQuest.questName}");
                    QuestManager.Instance.AddQuest(musicQuest);
                }

                // slutf�r del m�l
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