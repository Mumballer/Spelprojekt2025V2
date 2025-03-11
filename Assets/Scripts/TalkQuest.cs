using UnityEngine;

public class TalkQuest : Quest
{
    [SerializeField] private GameObject character;

    private void Start()
    {
        character.GetComponent<Interactable>().onInteract.AddListener(Talk);
    }

    private void Talk()
    {
        // Check if quest is active
        if (!QuestManager.Instance.IsQuestActive(id)) return;

        // Since you mentioned dialog is already handled, just trigger it here
        // DialogManager.Instance.StartDialog(characterID);

        // Complete quest
        QuestManager.Instance.CompleteQuest(id);
    }
}