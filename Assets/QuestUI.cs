using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class QuestUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject questPanel;
    [SerializeField] private TextMeshProUGUI questTitleText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [SerializeField] private TextMeshProUGUI questStatusText;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI questCounterText; // Optional: Shows "Quest X/Y"
    [SerializeField] private Button nextQuestButton;          // Optional: For multiple quests
    [SerializeField] private Button prevQuestButton;          // Optional: For multiple quests

    [Header("Text Settings")]
    [SerializeField] private Color inProgressColor = new Color(1f, 0.92f, 0.016f); // Yellow
    [SerializeField] private Color completedColor = new Color(0f, 0.75f, 0.22f);   // Green
    [SerializeField] private float hideDelay = 3f; // Seconds to show completed quest before hiding

    public Quest currentQuest;

    // Singleton pattern for easy access
    public static QuestUI Instance { get; private set; }

    private Coroutine hideCoroutine;

    private void Awake()
    {
        // Setup singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Set up button listeners
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideQuestPanel);
        }

        if (nextQuestButton != null)
        {
            nextQuestButton.onClick.AddListener(ShowNextQuest);
            nextQuestButton.gameObject.SetActive(false); // Hide initially
        }

        if (prevQuestButton != null)
        {
            prevQuestButton.onClick.AddListener(ShowPreviousQuest);
            prevQuestButton.gameObject.SetActive(false); // Hide initially
        }

        // Register for quest events
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted += HandleQuestStarted;
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
            QuestManager.Instance.OnObjectiveUpdated += HandleObjectiveUpdated;
        }
        else
        {
            Debug.LogWarning("QuestUI: No QuestManager instance found");
        }

        // Hide initially
        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }

        // Check for active quests at start
        CheckForActiveQuests();
    }

    private void OnDestroy()
    {
        // Unregister from quest events
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted -= HandleQuestStarted;
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
            QuestManager.Instance.OnObjectiveUpdated -= HandleObjectiveUpdated;
        }

        // Clean up button listeners
        if (closeButton != null)
            closeButton.onClick.RemoveListener(HideQuestPanel);

        if (nextQuestButton != null)
            nextQuestButton.onClick.RemoveListener(ShowNextQuest);

        if (prevQuestButton != null)
            prevQuestButton.onClick.RemoveListener(ShowPreviousQuest);

        // Clear singleton if this is the instance
        if (Instance == this)
            Instance = null;
    }

    // Check for and display any active quests
    private void CheckForActiveQuests()
    {
        if (QuestManager.Instance != null && QuestManager.Instance.activeQuests.Count > 0)
        {
            currentQuest = QuestManager.Instance.activeQuests[0];
            UpdateQuestDisplay();
            ShowQuestPanel();
        }
    }

    // Event Handlers
    private void HandleQuestStarted(Quest quest)
    {
        Debug.Log($"QuestUI: Quest started - {quest.questName}");
        
        // Cancel any pending hide operations
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
            Debug.Log("QuestUI: Canceled UI hiding due to new quest");
        }
        
        currentQuest = quest;
        UpdateQuestDisplay();
        ShowQuestPanel();
        UpdateNavigationButtons();
    }

    private void HandleQuestCompleted(Quest quest)
    {
        Debug.Log($"QuestUI: Quest completed - {quest.questName}");
        
        // Update display to show completion status
        if (currentQuest == quest)
        {
            UpdateQuestDisplay();
            
            // Check if there are more active quests
            if (QuestManager.Instance != null && QuestManager.Instance.activeQuests.Count > 0)
            {
                // Schedule switching to next quest after delay
                // Store the coroutine reference so we can cancel it
                hideCoroutine = StartCoroutine(SwitchToNextQuestAfterDelay(hideDelay));
            }
            else
            {
                // No more quests, schedule hiding after delay
                // Store the coroutine reference so we can cancel it
                hideCoroutine = StartCoroutine(HideQuestPanelAfterDelay(hideDelay));
                return; // Exit early to avoid updating the now-hidden UI
            }
        }

        UpdateNavigationButtons();
    }

    private void HandleObjectiveUpdated(QuestObjective objective)
    {
        // Only update if the objective belongs to the current quest
        if (currentQuest != null && currentQuest.objectives.Contains(objective))
        {
            Debug.Log($"QuestUI: Objective updated for {currentQuest.questName}");
            UpdateQuestDisplay();
        }
    }

    // Panel visibility methods
    public void ShowQuestPanel()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(true);
        }
    }

    public void HideQuestPanel()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }
    }

    public void ForceHideQuestPanel()
    {
        Debug.Log("QuestUI: Force hiding quest panel");
        currentQuest = null;
        HideQuestPanel();

        // If we're using a coroutine to hide, stop it
        StopAllCoroutines();
    }

    // Display update methods
    public void UpdateQuestDisplay()
    {
        if (currentQuest == null)
        {
            Debug.LogWarning("QuestUI: Can't update display - no current quest");
            return;
        }

        // Update title
        if (questTitleText != null)
        {
            questTitleText.text = currentQuest.questName;
            Debug.Log($"Setting title text to: {currentQuest.questName}");
        }

        // Update description
        if (questDescriptionText != null)
        {
            questDescriptionText.text = currentQuest.description;
            Debug.Log($"Setting description text to: {currentQuest.description}");
        }

        // Update status and progress
        if (questStatusText != null)
        {
            int completedObjectives = 0;
            foreach (var objective in currentQuest.objectives)
            {
                if (objective.isCompleted)
                    completedObjectives++;
            }

            bool isCompleted = completedObjectives >= currentQuest.objectives.Count;

            if (isCompleted)
            {
                questStatusText.text = "[COMPLETED]";
                questStatusText.color = completedColor;
            }
            else
            {
                questStatusText.text = "[IN PROGRESS]";
                questStatusText.color = inProgressColor;

                // Add progress info
                if (currentQuest.objectives.Count > 0)
                {
                    questStatusText.text += $" ({completedObjectives}/{currentQuest.objectives.Count})";
                }
            }

            Debug.Log($"Setting status text to: {questStatusText.text}");
        }

        // Update quest counter if available
        UpdateQuestCounter();
    }

    // Quest navigation methods
    public void ShowNextQuest()
    {
        if (QuestManager.Instance == null || QuestManager.Instance.activeQuests.Count <= 1)
            return;

        int currentIndex = QuestManager.Instance.activeQuests.IndexOf(currentQuest);
        if (currentIndex == -1)
            currentIndex = 0;

        int nextIndex = (currentIndex + 1) % QuestManager.Instance.activeQuests.Count;
        currentQuest = QuestManager.Instance.activeQuests[nextIndex];

        UpdateQuestDisplay();
    }

    public void ShowPreviousQuest()
    {
        if (QuestManager.Instance == null || QuestManager.Instance.activeQuests.Count <= 1)
            return;

        int currentIndex = QuestManager.Instance.activeQuests.IndexOf(currentQuest);
        if (currentIndex == -1)
            currentIndex = 0;

        int prevIndex = (currentIndex - 1 + QuestManager.Instance.activeQuests.Count) % QuestManager.Instance.activeQuests.Count;
        currentQuest = QuestManager.Instance.activeQuests[prevIndex];

        UpdateQuestDisplay();
    }

    private void UpdateQuestCounter()
    {
        if (questCounterText == null || QuestManager.Instance == null)
            return;

        int totalQuests = QuestManager.Instance.activeQuests.Count;

        if (totalQuests > 0)
        {
            int currentIndex = QuestManager.Instance.activeQuests.IndexOf(currentQuest);
            if (currentIndex == -1) currentIndex = 0;

            questCounterText.text = $"Quest {currentIndex + 1}/{totalQuests}";
            questCounterText.gameObject.SetActive(totalQuests > 1);
        }
        else
        {
            questCounterText.gameObject.SetActive(false);
        }
    }

    private void UpdateNavigationButtons()
    {
        bool hasMultipleQuests = (QuestManager.Instance != null && QuestManager.Instance.activeQuests.Count > 1);

        if (nextQuestButton != null)
            nextQuestButton.gameObject.SetActive(hasMultipleQuests);

        if (prevQuestButton != null)
            prevQuestButton.gameObject.SetActive(hasMultipleQuests);
    }

    // Delayed actions
    private IEnumerator HideQuestPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Double-check that we should still hide the panel
        if (QuestManager.Instance != null && QuestManager.Instance.activeQuests.Count > 0)
        {
            Debug.Log("QuestUI: Not hiding panel after delay because there are active quests");
            yield break;
        }
        
        Debug.Log("QuestUI: Hiding quest panel after delay");
        HideQuestPanel();
    }

    private IEnumerator SwitchToNextQuestAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (QuestManager.Instance != null && QuestManager.Instance.activeQuests.Count > 0)
        {
            Debug.Log("QuestUI: Switching to next quest after delay");
            currentQuest = QuestManager.Instance.activeQuests[0];
            UpdateQuestDisplay();
        }
        else
        {
            Debug.Log("QuestUI: No more quests to switch to, hiding panel");
            HideQuestPanel();
        }
    }

    // Testing methods
    public void ForceShowTestQuest()
    {
        // Create a test quest
        Quest testQuest = ScriptableObject.CreateInstance<Quest>();
        testQuest.questName = "Test Quest";
        testQuest.description = "This is a test quest for UI debugging.";

        // Create objectives
        testQuest.objectives = new List<QuestObjective>();

        QuestObjective objective1 = new QuestObjective
        {
            description = "First test objective",
            isCompleted = false
        };

        QuestObjective objective2 = new QuestObjective
        {
            description = "Second test objective",
            isCompleted = false
        };

        testQuest.objectives.Add(objective1);
        testQuest.objectives.Add(objective2);

        // Display the test quest
        currentQuest = testQuest;
        UpdateQuestDisplay();
        ShowQuestPanel();

        Debug.Log("QuestUI: Showing test quest");
    }
}