using UnityEngine;
using TMPro;

public class QuestEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questTitleText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [SerializeField] private TextMeshProUGUI questObjectivesText;
    [SerializeField] private TextMeshProUGUI questProgressText; // New field for progress counter

    [Header("Color Settings")]
    [SerializeField] private Color inProgressColor = Color.yellow;
    [SerializeField] private Color completedColor = Color.green;
    [SerializeField] private Color titleColor = Color.white;
    [SerializeField] private Color completedTitleColor = Color.green;

    private Quest currentQuest;

    public Quest CurrentQuest => currentQuest;

    public void Setup(Quest quest)
    {
        currentQuest = quest;
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        if (currentQuest == null) return;

        bool isCompleted = currentQuest.IsCompleted;

        if (questTitleText != null)
        {
            questTitleText.text = currentQuest.questName;
            questTitleText.color = isCompleted ? completedTitleColor : titleColor;
        }

        if (questDescriptionText != null)
        {
            questDescriptionText.text = currentQuest.description;
        }

        if (questObjectivesText != null && currentQuest.Objectives != null)
        {
            string objectivesContent = "";
            foreach (var objective in currentQuest.Objectives)
            {
                string colorHex = ColorUtility.ToHtmlStringRGB(objective.isCompleted ? completedColor : inProgressColor);
                string checkmark = objective.isCompleted ? "✓ " : "• ";
                objectivesContent += $"<color=#{colorHex}>{checkmark}{objective.description}</color>\n";
            }
            questObjectivesText.text = objectivesContent.TrimEnd('\n');
        }

        // Add progress counter at the bottom
        if (questProgressText != null && currentQuest.Objectives != null && currentQuest.Objectives.Count > 0)
        {
            int completedCount = 0;
            int totalCount = currentQuest.Objectives.Count;

            foreach (var objective in currentQuest.Objectives)
            {
                if (objective.isCompleted)
                    completedCount++;
            }

            string colorHex = completedCount == totalCount ?
                ColorUtility.ToHtmlStringRGB(completedColor) :
                ColorUtility.ToHtmlStringRGB(inProgressColor);

            questProgressText.text = $"<color=#{colorHex}>{completedCount}/{totalCount} Completed</color>";
        }
    }
}