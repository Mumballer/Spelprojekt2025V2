using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuestUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject questLogPanel;
    [SerializeField] private Transform questListContainer;
    [SerializeField] private GameObject questEntryPrefab;
    [SerializeField] private KeyCode toggleKey = KeyCode.J;
    [SerializeField] private TextMeshProUGUI questLogHeaderText;
    [SerializeField] private GameObject noQuestsMessage;

    [Header("Tab System")]
    [SerializeField] private Button activeQuestsTab;
    [SerializeField] private Button completedQuestsTab;
    [SerializeField] private Color selectedTabColor = Color.white;
    [SerializeField] private Color unselectedTabColor = new Color(0.7f, 0.7f, 0.7f);

    [Header("Color Settings")]
    [SerializeField] private Color inProgressColor = new Color(1f, 0.92f, 0.016f); 
    [SerializeField] private Color completedColor = new Color(0f, 0.75f, 0.22f); 
    [SerializeField] private Color questTitleColor = Color.white;
    [SerializeField] private Color questCompletedTitleColor = new Color(0.5f, 0.5f, 0.5f); 

    private List<GameObject> questEntries = new List<GameObject>();
    private bool showingActiveQuests = true;
    private PlayerController playerController;

    private void Start()
    {
        if (questLogPanel != null)
        {
            questLogPanel.SetActive(false);
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAdded += RefreshQuestList;
            QuestManager.Instance.OnQuestCompleted += RefreshQuestList;
            QuestManager.Instance.OnObjectiveCompleted += (quest, index) => RefreshQuestList(quest);
        }

        playerController = FindFirstObjectByType<PlayerController>();

        // Setup tabs
        if (activeQuestsTab != null)
        {
            activeQuestsTab.onClick.AddListener(() => {
                showingActiveQuests = true;
                UpdateTabVisuals();
                RefreshQuestList();
            });
        }

        if (completedQuestsTab != null)
        {
            completedQuestsTab.onClick.AddListener(() => {
                showingActiveQuests = false;
                UpdateTabVisuals();
                RefreshQuestList();
            });
        }

        UpdateTabVisuals();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleQuestLog();
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAdded -= RefreshQuestList;
            QuestManager.Instance.OnQuestCompleted -= RefreshQuestList;
            QuestManager.Instance.OnObjectiveCompleted -= (quest, index) => RefreshQuestList(quest);
        }
    }

    public void ToggleQuestLog()
    {
        if (questLogPanel != null)
        {
            bool newState = !questLogPanel.activeSelf;
            questLogPanel.SetActive(newState);

            if (playerController != null)
            {
                playerController.SetCanMove(!newState);
            }

            if (newState)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                RefreshQuestList();
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void UpdateTabVisuals()
    {
        if (activeQuestsTab != null)
        {
            Image tabImage = activeQuestsTab.GetComponent<Image>();
            if (tabImage != null)
            {
                tabImage.color = showingActiveQuests ? selectedTabColor : unselectedTabColor;
            }

            TextMeshProUGUI tabText = activeQuestsTab.GetComponentInChildren<TextMeshProUGUI>();
            if (tabText != null)
            {
                tabText.fontStyle = showingActiveQuests ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        if (completedQuestsTab != null)
        {
            Image tabImage = completedQuestsTab.GetComponent<Image>();
            if (tabImage != null)
            {
                tabImage.color = showingActiveQuests ? unselectedTabColor : selectedTabColor;
            }

            TextMeshProUGUI tabText = completedQuestsTab.GetComponentInChildren<TextMeshProUGUI>();
            if (tabText != null)
            {
                tabText.fontStyle = showingActiveQuests ? FontStyles.Normal : FontStyles.Bold;
            }
        }

        if (questLogHeaderText != null)
        {
            questLogHeaderText.text = showingActiveQuests ? "ACTIVE QUESTS" : "COMPLETED QUESTS";
        }
    }

    private void RefreshQuestList(Quest quest = null)
    {
        foreach (var entry in questEntries)
        {
            Destroy(entry);
        }
        questEntries.Clear();

        if (QuestManager.Instance == null || questListContainer == null || questEntryPrefab == null)
            return;

        List<Quest> questsToShow = showingActiveQuests ?
            QuestManager.Instance.GetActiveQuests() :
            QuestManager.Instance.GetCompletedQuests();

        if (noQuestsMessage != null)
        {
            noQuestsMessage.SetActive(questsToShow.Count == 0);
        }

        foreach (var currentQuest in questsToShow)
        {
            GameObject entryObj = Instantiate(questEntryPrefab, questListContainer);
            QuestEntryUI entryUI = entryObj.GetComponent<QuestEntryUI>();

            if (entryUI != null)
            {
                entryUI.SetupQuest(
                    currentQuest,
                    currentQuest.IsCompleted,
                    inProgressColor,
                    completedColor,
                    questTitleColor,
                    questCompletedTitleColor
                );
            }
            else
            {
                SetupQuestEntryManually(entryObj, currentQuest);
            }

            questEntries.Add(entryObj);
        }
    }

    private void SetupQuestEntryManually(GameObject entryObj, Quest currentQuest)
    {
        TextMeshProUGUI questNameText = entryObj.transform.Find("QuestTitleText")?.GetComponent<TextMeshProUGUI>();
        if (questNameText != null)
        {
            questNameText.text = currentQuest.questName;
            questNameText.color = currentQuest.IsCompleted ? questCompletedTitleColor : questTitleColor;
        }
        else
        {
            Debug.LogWarning("QuestTitleText not found on quest entry prefab");
        }
        TextMeshProUGUI descriptionText = entryObj.transform.Find("QuestDescriptionText")?.GetComponent<TextMeshProUGUI>();
        if (descriptionText != null)
        {
            descriptionText.text = currentQuest.description;
        }
        else
        {
            Debug.LogWarning("QuestDescriptionText not found on quest entry prefab");
        }
        TextMeshProUGUI objectivesText = entryObj.transform.Find("QuestObjectivesText")?.GetComponent<TextMeshProUGUI>();
        if (objectivesText != null && currentQuest.Objectives != null)
        {
            string objectivesContent = "";

            for (int i = 0; i < currentQuest.Objectives.Count; i++)
            {
                var objective = currentQuest.Objectives[i];
                if (objective == null) continue;

                string colorHex = objective.isCompleted ?
                    ColorUtility.ToHtmlStringRGB(completedColor) :
                    ColorUtility.ToHtmlStringRGB(inProgressColor);

                string checkmark = objective.isCompleted ? "✓ " : "• ";
                objectivesContent += $"<color=#{colorHex}>{checkmark}{objective.description}</color>\n";
            }

            objectivesText.text = objectivesContent.TrimEnd('\n');
        }
        else
        {
            Debug.LogWarning("QuestObjectivesText not found on quest entry prefab");
        }
    }
}