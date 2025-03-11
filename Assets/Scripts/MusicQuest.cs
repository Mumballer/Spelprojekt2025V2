using UnityEngine;

public class MusicQuest : Quest
{
    [SerializeField] private GameObject musicObject;

    private void Start()
    {
        musicObject.GetComponent<Interactable>().onInteract.AddListener(PlayMusic);
    }

    private void PlayMusic()
    {
        // Check if quest is active
        if (!QuestManager.Instance.IsQuestActive(id)) return;

        // Play music
        // AudioManager.Instance.PlayMusic(musicID);

        // Complete quest
        QuestManager.Instance.CompleteQuest(id);
    }
}