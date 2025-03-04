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

    // Store original prefab properties
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Vector2 originalSizeDelta;
    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;
    private Vector2 originalPivot;

    private void Awake()
    {
        // Store the original prefab transform properties
        if (questEntryPrefab != null)
        {
            // Store original transform properties
            RectTransform prefabRect = questEntryPrefab.GetComponent<RectTransform>();
            if (prefabRect != null)
            {
                originalPosition = prefabRect.localPosition;
                originalScale = prefabRect.localScale;
                originalSizeDelta = prefabRect.sizeDelta;
                originalAnchorMin = prefabRect.anchorMin;
                originalAnchorMax = prefabRect.anchorMax;
                originalPivot = prefabRect.pivot;
            }

            // Make sure the prefab is hidden
            questEntryPrefab.SetActive(false);
        }
    }

    private void Start()
    {
        // Ensure quest log is active from the start
        if (questLogPanel != null)
        {
            questLogPanel.SetActive(true);
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAdded += RefreshQuestList;
            QuestManager.Instance.OnQuestCompleted += RefreshQuestList;
            QuestManager.Instance.OnObjectiveCompleted += (quest, index) => RefreshQuestList(quest);
        }

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
        RefreshQuestList(); // Refresh initially
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

        // Disable any layout groups temporarily
        LayoutGroup[] layoutGroups = questListContainer.GetComponents<LayoutGroup>();
        foreach (var layout in layoutGroups)
        {
            layout.enabled = false;
        }

        // If we have quests to show, hide the original prefab and show clones
        for (int i = 0; i < questsToShow.Count; i++)
        {
            var currentQuest = questsToShow[i];

            // Instantiate DIRECTLY in the same parent as the prefab
            GameObject entryObj = Instantiate(questEntryPrefab, questEntryPrefab.transform.parent);

            // Force exact same position and properties in world space
            RectTransform entryRect = entryObj.GetComponent<RectTransform>();
            if (entryRect != null)
            {
                // Set exact same position as original prefab
                entryRect.position = questEntryPrefab.GetComponent<RectTransform>().position;
                entryRect.localScale = originalScale;
                entryRect.sizeDelta = originalSizeDelta;
                entryRect.anchorMin = originalAnchorMin;
                entryRect.anchorMax = originalAnchorMax;
                entryRect.pivot = originalPivot;
            }

            // Make it visible
            entryObj.SetActive(true);

            // Setup the quest data
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

        // Re-enable layout groups
        foreach (var layout in layoutGroups)
        {
            layout.enabled = true;
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