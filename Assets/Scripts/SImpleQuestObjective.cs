using UnityEngine;

public class SimpleQuestObjective : MonoBehaviour
{
    public Quest quest;
    public int objectiveIndex;
    public bool completeAfterDialog = true;

    private DialogTrigger dialogTrigger;
    private bool completed = false;

    void Start()
    {
        dialogTrigger = GetComponent<DialogTrigger>();

        if (dialogTrigger != null && completeAfterDialog)
        {
            // Hook into dialog completed event if your DialogManager has one
            // If not, you'll need to add this event to your DialogManager
            DialogManager.Instance.OnDialogComplete += CheckDialogComplete;
        }
    }

    private void CheckDialogComplete(Dialog dialog)
    {
        if (completed) return;

        if (dialogTrigger != null && dialogTrigger.dialog == dialog)
        {
            CompleteObjective();
        }
    }

    private void CompleteObjective()
    {
        if (completed) return;

        if (QuestManager.Instance != null && quest != null)
        {
            QuestManager.Instance.CompleteObjective(quest, objectiveIndex);
            Debug.Log($"Completed objective {objectiveIndex} for quest {quest.questName}");
            completed = true;
        }
    }

    void OnDestroy()
    {
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.OnDialogComplete -= CheckDialogComplete;
        }
    }
}