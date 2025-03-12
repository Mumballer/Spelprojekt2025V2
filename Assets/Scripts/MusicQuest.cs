using UnityEngine;

public class MusicQuest : Quest
{
    [SerializeField] private GameObject musicObject;
    private Gramophone gramophone;
    private bool questCompleted = false;

    private void Start()
    {
        if (musicObject == null)
        {
            Debug.LogError("Music object not assigned in MusicQuest!");
            return;
        }

        // Try to find the Interactable component on the music object or its children
        Interactable interactable = musicObject.GetComponent<Interactable>();
        if (interactable == null)
        {
            // Try to find it on children if not on the parent
            interactable = musicObject.GetComponentInChildren<Interactable>();
        }

        if (interactable != null)
        {
            interactable.onInteract.AddListener(PlayMusic);
            Debug.Log("Successfully connected to Interactable component");
        }
        else
        {
            Debug.LogError("Interactable component not found on musicObject or its children!");
        }

        // Get the Gramophone component (might be on child)
        gramophone = musicObject.GetComponentInChildren<Gramophone>();
        if (gramophone == null)
        {
            Debug.LogError("Gramophone component not found on musicObject or its children!");
        }
        else
        {
            Debug.Log("Successfully found Gramophone component");
        }
    }

    private void PlayMusic()
    {
        // Check if quest is active
        if (!QuestManager.Instance.IsQuestActive(id) || questCompleted)
        {
            Debug.Log($"Music quest not activating: Active={QuestManager.Instance.IsQuestActive(id)}, Completed={questCompleted}");
            return;
        }

        Debug.Log("Music quest interaction triggered");

        // Toggle the gramophone if available
        if (gramophone != null)
        {
            gramophone.ToggleMusic();

            // Only complete the quest if the gramophone is now playing
            if (gramophone.IsPlaying)
            {
                Debug.Log($"Music quest completing: {questTitle}");
                questCompleted = true;
                QuestManager.Instance.CompleteQuest(id);
            }
        }
        else
        {
            // If no gramophone available, just complete the quest
            Debug.Log($"Music quest completing (no gramophone): {questTitle}");
            questCompleted = true;
            QuestManager.Instance.CompleteQuest(id);
        }
    }
}