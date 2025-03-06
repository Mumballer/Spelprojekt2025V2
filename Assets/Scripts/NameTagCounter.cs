using UnityEngine;
using TMPro;

public class NametagCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private int totalNametags = 6;
    [SerializeField] private Color inProgressColor = Color.yellow;
    [SerializeField] private Color completedColor = Color.green;

    [Header("Quest Integration")]
    [SerializeField] private Quest relatedQuest;
    [SerializeField] private int objectiveIndex = 0;

    private int nametagsPlaced = 0;
    private bool objectiveComplete = false;

    private void Start()
    {
        UpdateCounterDisplay();
    }

    public void IncrementNametagCount()
    {
        nametagsPlaced++;

        if (nametagsPlaced > totalNametags)
        {
            nametagsPlaced = totalNametags;
        }

        UpdateCounterDisplay();

        // If all nametags placed, complete the quest objective (only once)
        if (nametagsPlaced >= totalNametags && !objectiveComplete)
        {
            objectiveComplete = true;

            // Only update quest if it's active
            if (relatedQuest != null && QuestManager.Instance != null && QuestManager.Instance.IsQuestActive(relatedQuest))
            {
                QuestManager.Instance.CompleteObjective(relatedQuest, objectiveIndex);
                Debug.Log($"All nametags placed! Completed objective {objectiveIndex} for quest {relatedQuest.questName}");
            }
        }
    }

    private void UpdateCounterDisplay()
    {
        if (counterText == null) return;

        string colorHex = nametagsPlaced >= totalNametags ?
            ColorUtility.ToHtmlStringRGB(completedColor) :
            ColorUtility.ToHtmlStringRGB(inProgressColor);

        counterText.text = $"<color=#{colorHex}>{nametagsPlaced}/{totalNametags} Nametags Placed</color>";
    }

    public int GetNametagsPlaced()
    {
        return nametagsPlaced;
    }

    public bool AllNametagsPlaced()
    {
        return nametagsPlaced >= totalNametags;
    }

    // For testing/debugging
    public void ResetCounter()
    {
        nametagsPlaced = 0;
        objectiveComplete = false;
        UpdateCounterDisplay();
    }
}