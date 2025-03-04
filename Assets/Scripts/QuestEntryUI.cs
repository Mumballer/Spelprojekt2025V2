using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questTitleText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [SerializeField] private TextMeshProUGUI questObjectivesText;

    public void SetupQuest(Quest quest, bool isCompleted, Color inProgressColor, Color completedColor, Color titleColor, Color completedTitleColor)
    {
        if (questTitleText != null)
        {
            questTitleText.text = quest.questName;
            questTitleText.color = isCompleted ? completedTitleColor : titleColor;
        }

        if (questDescriptionText != null)
        {
            questDescriptionText.text = quest.description;
        }

        if (questObjectivesText != null && quest.Objectives != null)
        {
            string objectivesContent = "";

            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                var objective = quest.Objectives[i];
                if (objective == null) continue;

                string colorHex = objective.isCompleted ?
                    ColorUtility.ToHtmlStringRGB(completedColor) :
                    ColorUtility.ToHtmlStringRGB(inProgressColor);

                string checkmark = objective.isCompleted ? "✓ " : "• ";
                objectivesContent += $"<color=#{colorHex}>{checkmark}{objective.description}</color>\n";
            }

            questObjectivesText.text = objectivesContent.TrimEnd('\n');
        }
    }
}