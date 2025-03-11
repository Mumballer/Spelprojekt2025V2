using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestUIChecker : MonoBehaviour
{
    public QuestUI questUI;
    public Canvas mainCanvas;
    public GameObject questPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statusText;
    
    // Reference to QuestTest to start a real quest
    public QuestTest questTester;
    
    public void Start()
    {
        Debug.Log("===== Quest UI Diagnostic =====");

        // Check Canvas
        if (mainCanvas != null)
        {
            Debug.Log($"Canvas render mode: {mainCanvas.renderMode}");
            Debug.Log($"Canvas scale factor: {mainCanvas.scaleFactor}");
            Debug.Log($"Canvas size: {mainCanvas.pixelRect.width}x{mainCanvas.pixelRect.height}");
        }
        else
        {
            Debug.LogError("Canvas reference missing!");
        }

        // Check Panel
        if (questPanel != null)
        {
            RectTransform rt = questPanel.GetComponent<RectTransform>();
            Debug.Log($"Panel position: {rt.anchoredPosition}");
            Debug.Log($"Panel size: {rt.sizeDelta}");
            Debug.Log($"Panel active: {questPanel.activeSelf}");

            // Check Image
            Image img = questPanel.GetComponent<Image>();
            if (img != null)
            {
                Debug.Log($"Panel color: {img.color} (Alpha: {img.color.a})");
            }
        }
        else
        {
            Debug.LogError("Quest panel reference missing!");
        }

        // Check Text Components
        CheckText("Title", titleText);
        CheckText("Description", descriptionText);
        CheckText("Status", statusText);

        // Check Manager connection
        if (QuestManager.Instance != null)
        {
            Debug.Log("QuestManager instance found");
            Debug.Log($"Active quests: {QuestManager.Instance.activeQuests.Count}");
        }
        else
        {
            Debug.LogError("QuestManager instance missing!");
        }

        Debug.Log("==============================");
    }

    private void CheckText(string name, TextMeshProUGUI text)
    {
        if (text != null)
        {
            Debug.Log($"{name} text: \"{text.text}\"");
            Debug.Log($"{name} color: {text.color}");
            Debug.Log($"{name} font size: {text.fontSize}");
        }
        else
        {
            Debug.LogError($"{name} text reference missing!");
        }
    }

    private void StartRealQuest()
    {
        Debug.Log("Starting real quest from QuestUIChecker");
        
        // If we have a quest tester, use it
        if (questTester != null)
        {
            questTester.StartTestQuest();
        }
        // Otherwise create a quest directly here
        else if (QuestManager.Instance != null)
        {
            // Create a quest through the manager
            QuestManager.Instance.CreateAndStartQuest("Organize Room", "Clean up the mess", "organize_task");
        }
    }
}