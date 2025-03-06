using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

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
    [SerializeField] private Button availableQuestsTab;
    [SerializeField] private Color selectedTabColor = Color.white;
    [SerializeField] private Color unselectedTabColor = new Color(0.7f, 0.7f, 0.7f);

    [Header("Color Settings")]
    [SerializeField] private Color inProgressColor = new Color(1f, 0.92f, 0.016f);
    [SerializeField] private Color completedColor = new Color(0f, 0.75f, 0.22f);
    [SerializeField] private Color questTitleColor = Color.white;
    [SerializeField] private Color questCompletedTitleColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color availableQuestColor = new Color(0.4f, 0.7f, 1f);

    [Header("Notifications")]
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private Transform notificationContainer;
    [SerializeField] private float notificationDuration = 3f;

    private List<GameObject> questEntries = new List<GameObject>();
    private enum QuestTabType { Active, Completed, Available }
    private QuestTabType currentTab = QuestTabType.Active;

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
            QuestManager.Instance.OnQuestAdded += HandleQuestAdded;
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
            QuestManager.Instance.OnQuestRemoved += HandleQuestRemoved;
            QuestManager.Instance.OnQuestAvailable += HandleQuestAvailable;
            QuestManager.Instance.OnObjectiveCompleted += (quest, index) => RefreshQuestList();
        }

        // Setup tabs
        if (activeQuestsTab != null)
        {
            activeQuestsTab.onClick.AddListener(() => {
                currentTab = QuestTabType.Active;
                UpdateTabVisuals();
                RefreshQuestList();
            });
        }

        if (completedQuestsTab != null)
        {
            completedQuestsTab.onClick.AddListener(() => {
                currentTab = QuestTabType.Completed;
                UpdateTabVisuals();
                RefreshQuestList();
            });
        }

        if (availableQuestsTab != null)
        {
            availableQuestsTab.onClick.AddListener(() => {
                currentTab = QuestTabType.Available;
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
            QuestManager.Instance.OnQuestAdded -= HandleQuestAdded;
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
            QuestManager.Instance.OnQuestRemoved -= HandleQuestRemoved;
            QuestManager.Instance.OnQuestAvailable -= HandleQuestAvailable;
            QuestManager.Instance.OnObjectiveCompleted -= (quest, index) => RefreshQuestList();
        }
    }

    private void UpdateTabVisuals()
    {
        // Active tab
        if (activeQuestsTab != null)
        {
            Image tabImage = activeQuestsTab.GetComponent<Image>();
            if (tabImage != null)
            {
                tabImage.color = currentTab == QuestTabType.Active ? selectedTabColor : unselectedTabColor;
            }

            TextMeshProUGUI tabText = activeQuestsTab.GetComponentInChildren<TextMeshProUGUI>();
            if (tabText != null)
            {
                tabText.fontStyle = currentTab == QuestTabType.Active ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        // Completed tab
        if (completedQuestsTab != null)
        {
            Image tabImage = completedQuestsTab.GetComponent<Image>();
            if (tabImage != null)
            {
                tabImage.color = currentTab == QuestTabType.Completed ? selectedTabColor : unselectedTabColor;
            }

            TextMeshProUGUI tabText = completedQuestsTab.GetComponentInChildren<TextMeshProUGUI>();
            if (tabText != null)
            {
                tabText.fontStyle = currentTab == QuestTabType.Completed ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        // Available tab
        if (availableQuestsTab != null)
        {
            Image tabImage = availableQuestsTab.GetComponent<Image>();
            if (tabImage != null)
            {
                tabImage.color = currentTab == QuestTabType.Available ? selectedTabColor : unselectedTabColor;
            }

            TextMeshProUGUI tabText = availableQuestsTab.GetComponentInChildren<TextMeshProUGUI>();
            if (tabText != null)
            {
                tabText.fontStyle = currentTab == QuestTabType.Available ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        if (questLogHeaderText != null)
        {
            switch (currentTab)
            {
                case QuestTabType.Active:
                    questLogHeaderText.text = "ACTIVE QUESTS";
                    break;
                case QuestTabType.Completed:
                    questLogHeaderText.text = "COMPLETED QUESTS";
                    break;
                case QuestTabType.Available:
                    questLogHeaderText.text = "AVAILABLE QUESTS";
                    break;
            }
        }
    }

    private void RefreshQuestList()
    {
        foreach (var entry in questEntries)
        {
            Destroy(entry);
        }
        questEntries.Clear();

        if (QuestManager.Instance == null || questListContainer == null || questEntryPrefab == null)
            return;

        List<Quest> questsToShow;

        switch (currentTab)
        {
            case QuestTabType.Active:
                questsToShow = QuestManager.Instance.GetActiveQuests();
                break;
            case QuestTabType.Completed:
                questsToShow = QuestManager.Instance.GetCompletedQuests();
                break;
            case QuestTabType.Available:
                questsToShow = QuestManager.Instance.GetAvailableQuests();
                break;
            default:
                questsToShow = new List<Quest>();
                break;
        }

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
            GameObject entryObj = Instantiate(questEntryPrefab, questListContainer);

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
                Color titleColorToUse = currentTab == QuestTabType.Available ?
                    availableQuestColor :
                    (currentQuest.IsCompleted ? questCompletedTitleColor : questTitleColor);

                entryUI.SetupQuest(
                    currentQuest,
                    currentQuest.IsCompleted,
                    inProgressColor,
                    completedColor,
                    titleColorToUse,
                    questCompletedTitleColor
                );

                // Add accept button for available quests
                if (currentTab == QuestTabType.Available)
                {
                    Button acceptButton = entryObj.GetComponentInChildren<Button>();
                    if (acceptButton != null)
                    {
                        acceptButton.onClick.AddListener(() => {
                            QuestManager.Instance.AddQuest(currentQuest);
                            RefreshQuestList();
                        });
                    }
                }
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

            if (currentTab == QuestTabType.Available)
            {
                questNameText.color = availableQuestColor;
            }
            else
            {
                questNameText.color = currentQuest.IsCompleted ? questCompletedTitleColor : questTitleColor;
            }
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

        // Add accept button for available quests
        if (currentTab == QuestTabType.Available)
        {
            Button acceptButton = entryObj.transform.Find("AcceptButton")?.GetComponent<Button>();
            if (acceptButton != null)
            {
                acceptButton.gameObject.SetActive(true);
                acceptButton.onClick.AddListener(() => {
                    QuestManager.Instance.AddQuest(currentQuest);
                    RefreshQuestList();
                    ShowNotification($"Accepted quest: {currentQuest.questName}", Color.green);
                });
            }
        }
    }

    private void HandleQuestAdded(Quest quest)
    {
        if (currentTab == QuestTabType.Active)
        {
            RefreshQuestList();
        }
        ShowNotification($"New Quest: {quest.questName}", Color.yellow);
    }

    private void HandleQuestCompleted(Quest quest)
    {
        RefreshQuestList();
        ShowNotification($"Quest Completed: {quest.questName}", Color.green);
    }

    private void HandleQuestRemoved(Quest quest)
    {
        if (currentTab == QuestTabType.Active)
        {
            RefreshQuestList();
        }
    }

    private void HandleQuestAvailable(Quest quest)
    {
        if (currentTab == QuestTabType.Available)
        {
            RefreshQuestList();
        }
        ShowNotification($"New Quest Available: {quest.questName}", Color.cyan);
    }

    private void ShowNotification(string message, Color color)
    {
        if (notificationPrefab == null || notificationContainer == null)
            return;

        GameObject notification = Instantiate(notificationPrefab, notificationContainer);

        TextMeshProUGUI textComponent = notification.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = message;
            textComponent.color = color;
        }

        StartCoroutine(RemoveNotificationAfterDelay(notification));
    }

    private IEnumerator RemoveNotificationAfterDelay(GameObject notification)
    {
        yield return new WaitForSeconds(notificationDuration);

        // Fade out
        CanvasGroup canvasGroup = notification.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            float startTime = Time.time;
            float endTime = startTime + 1f;

            while (Time.time < endTime)
            {
                float t = (Time.time - startTime) / 1f;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        Destroy(notification);
    }
}