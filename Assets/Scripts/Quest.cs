using UnityEngine.Events;
using UnityEngine;

public abstract class Quest : MonoBehaviour
{
    public QuestManager.QuestID id;
    public string questTitle;
    public string questDescription;

    public UnityEvent onQuestActivate;
    public UnityEvent onQuestComplete;

    public virtual void OnActivate()
    {
        Debug.Log($"Quest activated: {questTitle}");
        onQuestActivate?.Invoke();
    }

    public virtual void OnComplete()
    {
        Debug.Log($"Quest completed: {questTitle}");
        onQuestComplete?.Invoke();


    }

    // Add this to your Quest class to make conversion from Quest to QuestID work
    public static implicit operator QuestManager.QuestID(Quest quest)
    {
        return quest.id;
    }

    // Modify your Quest class to add this property
    public string questName
    {
        get { return questTitle; } // Use the existing questTitle property
        set { questTitle = value; }
    }
}