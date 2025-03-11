using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int defaultQuestSequence = 0;
    [SerializeField] private bool startQuestsAutomatically = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (startQuestsAutomatically)
        {
            // Start the default quest sequence
            QuestManager.Instance.StartQuestSequence(defaultQuestSequence);
        }
    }

    // Call this method to switch to a different quest sequence
    public void SwitchQuestSequence(int sequenceIndex)
    {
        QuestManager.Instance.StartQuestSequence(sequenceIndex);
    }

    // You can add methods here to save/load game state
    // Or handle scene transitions, etc.
}