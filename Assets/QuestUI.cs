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
    [SerializeField] private TextMeshProUGUI questCounterText;
    [SerializeField] private Button nextQuestButton;
    [SerializeField] private Button prevQuestButton;

    [Header("Text Settings")]
    [SerializeField] private Color inProgressColor = new Color(1f, 0.92f, 0.016f);
    [SerializeField] private Color completedColor = new Color(0f, 0.75f, 0.22f);
    [SerializeField] private float hideDelay = 3f;

    public Quest currentQuest;

    public static QuestUI Instance { get; private set; }

    private Coroutine hideCoroutine;
    private int currentQuestIndex = 0;
    private bool isAutoHiding = false;
    private float lastQuestCompletionTime = 0f;

    private void Awake()
    {
        // skapar singleton
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
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideQuestPanel);
        }

        if (nextQuestButton != null)
        {
            nextQuestButton.onClick.AddListener(ShowNextQuest);
            nextQuestButton.gameObject.SetActive(false);
        }

        if (prevQuestButton != null)
        {
            prevQuestButton.onClick.AddListener(ShowPreviousQuest);
            prevQuestButton.gameObject.SetActive(false);
        }

        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAdded += HandleQuestAdded;
            QuestManager.Instance.OnQuestObjectiveCompleted += HandleObjectiveCompleted;
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAdded -= HandleQuestAdded;
            QuestManager.Instance.OnQuestObjectiveCompleted -= HandleObjectiveCompleted;
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        }
    }

    private void HandleQuestAdded(Quest quest)
    {
        // visar nytt uppdrag
        ShowQuest(quest);
    }

    private void HandleObjectiveCompleted(Quest quest, int objectiveIndex)
    {
        // uppdaterar när uppdragsmål avklaras
        if (quest == currentQuest)
        {
            UpdateQuestDisplay();
        }

        questPanel.SetActive(true);
        
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        
        hideCoroutine = StartCoroutine(HideQuestPanelAfterDelay(hideDelay));
    }

    private void HandleQuestCompleted(Quest quest)
    {
        // uppdaterar när uppdrag avklaras
        lastQuestCompletionTime = Time.time;
        
        if (quest == currentQuest)
        {
            UpdateQuestDisplay();
        }
        
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        
        hideCoroutine = StartCoroutine(HideQuestPanelAfterDelay(hideDelay * 1.5f));
    }

    public void ShowQuest(Quest quest)
    {
        // visar specifikt uppdrag
        currentQuest = quest;
        
        if (quest == null)
        {
            Debug.LogWarning("Trying to show null quest");
            return;
        }
        
        UpdateQuestDisplay();
        ShowQuestPanel();
        
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        
        hideCoroutine = StartCoroutine(HideQuestPanelAfterDelay(hideDelay));
    }

    private void ShowNextQuest()
    {
        // visar nästa uppdrag
        if (QuestManager.Instance == null || QuestManager.Instance.activeQuests.Count == 0)
            return;
            
        currentQuestIndex = (currentQuestIndex + 1) % QuestManager.Instance.activeQuests.Count;
        currentQuest = QuestManager.Instance.activeQuests[currentQuestIndex];
        UpdateQuestDisplay();
    }

    private void ShowPreviousQuest()
    {
        // visar föregående uppdrag
        if (QuestManager.Instance == null || QuestManager.Instance.activeQuests.Count == 0)
            return;
            
        currentQuestIndex--;
        if (currentQuestIndex < 0)
            currentQuestIndex = QuestManager.Instance.activeQuests.Count - 1;
            
        currentQuest = QuestManager.Instance.activeQuests[currentQuestIndex];
        UpdateQuestDisplay();
    }

    public void ShowQuestPanel()
    {
        // visar uppdragspanelen
        if (questPanel != null)
        {
            questPanel.SetActive(true);
            UpdateNavigationButtons();
        }
    }

    private void UpdateNavigationButtons()
    {
        if (QuestManager.Instance == null)
            return;
            
        bool multipleQuests = QuestManager.Instance.activeQuests.Count > 1;
        
        if (nextQuestButton != null)
            nextQuestButton.gameObject.SetActive(multipleQuests);
            
        if (prevQuestButton != null)
            prevQuestButton.gameObject.SetActive(multipleQuests);
            
        if (questCounterText != null && multipleQuests)
        {
            int index = QuestManager.Instance.activeQuests.IndexOf(currentQuest) + 1;
            questCounterText.text = $"{index}/{QuestManager.Instance.activeQuests.Count}";
            questCounterText.gameObject.SetActive(true);
        }
        else if (questCounterText != null)
        {
            questCounterText.gameObject.SetActive(false);
        }
    }

    private IEnumerator HideQuestPanelAfterDelay(float delay)
    {
        // gömmer panelen efter tid
        isAutoHiding = true;
        yield return new WaitForSeconds(delay);
        HideQuestPanel();
        isAutoHiding = false;
        hideCoroutine = null;
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
        // tvingar gömma panel
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
        
        HideQuestPanel();
    }

    private void UpdateQuestDisplay()
    {
        // uppdaterar uppdragsinformation
        if (currentQuest == null || questTitleText == null || 
            questDescriptionText == null || questStatusText == null)
            return;

        questTitleText.text = currentQuest.questName;
        questDescriptionText.text = currentQuest.description;

        int completedObjectives = 0;
        string objectivesText = "";

        foreach (QuestObjective objective in currentQuest.objectives)
        {
            if (objective.isCompleted)
            {
                completedObjectives++;
                objectivesText += $"<color=#{ColorUtility.ToHtmlStringRGB(completedColor)}>{objective.description} ✓</color>\n";
            }
            else
            {
                objectivesText += $"<color=#{ColorUtility.ToHtmlStringRGB(inProgressColor)}>{objective.description}</color>\n";
            }
        }

        questDescriptionText.text += $"\n\n{objectivesText}";

        if (currentQuest.IsCompleted)
        {
            questStatusText.text = "Completed";
            questStatusText.color = completedColor;
        }
        else
        {
            questStatusText.text = "In Progress";
            questStatusText.color = inProgressColor;
        }

        UpdateNavigationButtons();
    }

    private void Update()
    {
        // uppdaterar knappar
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!questPanel.activeSelf)
            {
                ShowAllQuests();
            }
            else
            {
                HideQuestPanel();
            }
        }
    }

    public void ShowAllQuests()
    {
        // visar alla uppdrag
        if (QuestManager.Instance == null || QuestManager.Instance.activeQuests.Count == 0)
        {
            Debug.Log("No active quests to show");
            return;
        }

        currentQuestIndex = 0;
        currentQuest = QuestManager.Instance.activeQuests[0];
        
        UpdateQuestDisplay();
        ShowQuestPanel();
    }

    public void ShowTestQuest()
    {
        Quest testQuest = ScriptableObject.CreateInstance<Quest>();
        testQuest.questName = "Test Quest";
        testQuest.description = "This is a test quest for UI debugging";

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

        currentQuest = testQuest;
        UpdateQuestDisplay();
        ShowQuestPanel();

        Debug.Log("QuestUI: Showing test quest");
    }
}