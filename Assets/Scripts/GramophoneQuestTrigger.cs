using UnityEngine;
using System.Collections;

public class GramophoneQuestTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip gramophoneMusic;
    [SerializeField] private Quest musicQuest;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    private bool isPlayerInRange = false;
    private bool isPlayingMusic = false;
    private Gramophone gramophone;

    private void Start()
    {
        // Find the Gramophone component
        gramophone = GetComponent<Gramophone>();
        if (gramophone == null)
        {
            Debug.LogError("Gramophone component not found on GramophoneQuestTrigger object!");
        }
    }

    private void Update()
    {
        // Check if player is in range
        if (CheckPlayerInRange())
        {
            isPlayerInRange = true;

            // Check for interaction input
            if (Input.GetKeyDown(interactionKey))
            {
                Interact();
            }
        }
        else
        {
            isPlayerInRange = false;
        }
    }

    private bool CheckPlayerInRange()
    {
        // Find player GameObject
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;

        // Check distance
        float distance = Vector3.Distance(transform.position, player.transform.position);
        return distance <= interactionDistance;
    }

    private void Interact()
    {
        // Toggle gramophone music if component exists
        if (gramophone != null)
        {
            // Toggle music
            gramophone.ToggleMusic();
            isPlayingMusic = gramophone.IsPlaying;

            // Handle quest objectives based on music state
            if (isPlayingMusic)
            {
                // Player turned ON the gramophone
                if (gramophoneMusic != null && musicQuest != null)
                {
                    Debug.Log("Music quest only: Completing start music objective");
                    QuestManager.Instance.CompleteObjective(musicQuest, 1);

                    // Add direct debugging to check quest status
                    CheckQuestStatus(musicQuest);
                }
            }
            else
            {
                // Player turned OFF the gramophone
                if (musicQuest != null)
                {
                    Debug.Log("Music quest only: Completing stop music objective");
                    QuestManager.Instance.CompleteObjective(musicQuest, 0);

                    // Add direct debugging to check quest status
                    CheckQuestStatus(musicQuest);

                    // Force quest completion if both objectives are done
                    ForceCompleteIfDone(musicQuest);
                }
            }
        }
    }

    // Debug helper method to check quest state
    private void CheckQuestStatus(Quest quest)
    {
        if (quest == null) return;

        // Check each objective
        Debug.Log($"QUEST STATUS CHECK - '{quest.questName}':");
        for (int i = 0; i < quest.Objectives.Count; i++)
        {
            Debug.Log($"  Objective {i}: {quest.Objectives[i].description} - Completed: {quest.Objectives[i].isCompleted}");
        }

        // Check overall quest state
        Debug.Log($"  Overall quest IsCompleted: {quest.IsCompleted}");
        Debug.Log($"  In activeQuests list: {QuestManager.Instance.IsQuestActive(quest)}");
        Debug.Log($"  In completedQuests list: {QuestManager.Instance.IsQuestCompleted(quest)}");
    }

    // Helper method to force completion
    private void ForceCompleteIfDone(Quest quest)
    {
        if (quest == null) return;

        bool allDone = true;
        foreach (var objective in quest.Objectives)
        {
            if (!objective.isCompleted)
            {
                allDone = false;
                break;
            }
        }

        if (allDone)
        {
            Debug.Log($"MANUALLY FORCING QUEST COMPLETION for '{quest.questName}'");

            // Force quest to complete and remove
            QuestManager.Instance.CompleteQuest(quest);

            // Force UI to refresh
            QuestDisplayManager[] displayManagers = FindObjectsOfType<QuestDisplayManager>();
            foreach (var manager in displayManagers)
            {
                Debug.Log($"Forcing refresh on QuestDisplayManager #{manager.gameObject.GetInstanceID()}");
                manager.RefreshQuestDisplay();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visual indicator for interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}