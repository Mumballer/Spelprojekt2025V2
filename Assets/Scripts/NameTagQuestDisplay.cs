using UnityEngine;
using System.Collections.Generic;

public class NameTagQuestDisplay : MonoBehaviour
{
    [Header("Quest References")]
    [SerializeField] private Quest nametagQuest;
    [SerializeField] private QuestEntryUI questEntryUI;

    [Header("Color Settings")]
    [SerializeField] private Color inProgressColor = Color.yellow;
    [SerializeField] private Color completedColor = Color.green;
    [SerializeField] private Color titleColor = Color.white;
    [SerializeField] private Color completedTitleColor = Color.green;

    [Header("Updates")]
    [SerializeField] private float refreshInterval = 0.5f;

    private bool isQuestComplete = false;
    private float refreshTimer = 0;

    private void Start()
    {
        if (nametagQuest == null)
        {
            Debug.LogError("Nametag Quest not assigned to NameTagQuestDisplay!");
            return;
        }

        if (questEntryUI == null)
        {
            questEntryUI = GetComponent<QuestEntryUI>();
            if (questEntryUI == null)
            {
                Debug.LogError("QuestEntryUI component not found!");
                return;
            }
        }

        RefreshQuestDisplay();

        if (NameTagManager.Instance != null)
        {
            NameTagManager.Instance.OnNameTagPlaced += (nameTag) => {
                RefreshQuestDisplay();
            };
        }
    }

    private void Update()
    {
        refreshTimer += Time.deltaTime;
        if (refreshTimer >= refreshInterval)
        {
            RefreshQuestDisplay();
            refreshTimer = 0;
        }
    }

    public void RefreshQuestDisplay()
    {
        if (nametagQuest == null || questEntryUI == null) return;

        isQuestComplete = nametagQuest.IsCompleted;

        questEntryUI.SetupQuest(
            nametagQuest,
            isQuestComplete,
            inProgressColor,
            completedColor,
            titleColor,
            completedTitleColor
        );
    }
}