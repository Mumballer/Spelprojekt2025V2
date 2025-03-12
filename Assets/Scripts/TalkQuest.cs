using UnityEngine;
public class TalkQuest : Quest
{
    [SerializeField] private GameObject character;
    private bool questCompleted = false;

    private void Start()
    {
        if (character == null)
        {
            Debug.LogError($"Character not assigned in TalkQuest {gameObject.name}");
            return;
        }

        Interactable interactable = character.GetComponent<Interactable>();
        if (interactable == null)
        {
            Debug.LogError($"Interactable not found on character in TalkQuest {gameObject.name}");
            return;
        }

        interactable.onInteract.AddListener(Talk);
        Debug.Log($"TalkQuest {questTitle} initialized with character {character.name}");
    }

    private void Talk()
    {
        // Prevent multiple completions
        if (questCompleted)
        {
            Debug.Log($"Talk quest {questTitle} already completed, ignoring interaction");
            return;
        }

        // Check if quest is active
        if (!QuestManager.Instance.IsQuestActive(id))
        {
            Debug.Log($"Talk quest {questTitle} not active, ignoring interaction");
            return;
        }

        Debug.Log($"Talk quest {questTitle} interaction triggered");

        // IMPORTANT: Do not complete the quest here if you're using DialogTrigger.OnDialogCompleted
        // to complete it. Only uncomment the line below if you're NOT using DialogTrigger.

        // questCompleted = true;
        // QuestManager.Instance.CompleteQuest(id);
    }
}