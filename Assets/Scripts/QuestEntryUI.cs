using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class QuestEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questTitleText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [SerializeField] private TextMeshProUGUI questObjectivesText;
    [SerializeField] private TextMeshProUGUI questProgressText;

    [Header("Color Settings")]
    [SerializeField] private Color inProgressColor = Color.yellow;
    [SerializeField] private Color completedColor = Color.green;
    [SerializeField] private Color titleColor = Color.white;
    [SerializeField] private Color completedTitleColor = Color.green;

    private Quest currentQuest;

    public Quest CurrentQuest => currentQuest;

    public void Setup(Quest quest)
    {
        if (quest == null) return;

        currentQuest = quest;
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        if (currentQuest == null) return;
        if (!gameObject || !gameObject.activeInHierarchy) return;

        bool isCompleted = currentQuest.IsCompleted;

        // Update title with color
        if (questTitleText != null)
        {
            questTitleText.text = currentQuest.questName;
            questTitleText.color = isCompleted ? completedTitleColor : titleColor;
        }

        // Update description
        if (questDescriptionText != null)
        {
            questDescriptionText.text = currentQuest.description;
        }

        // Update objectives list
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

        // Update progress counter
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