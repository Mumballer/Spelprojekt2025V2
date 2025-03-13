using UnityEngine;

public class QuestUIHelper : MonoBehaviour
{
    [SerializeField] private QuestUI questUI;

    private void Start()
    {
        // If not assigned in inspector, try to find in scene
        if (questUI == null)
        {
            questUI = FindObjectOfType<QuestUI>();
        }
    }

    public void HideAllQuestUI()
    {
        if (questUI != null)
        {
            questUI.ForceHideQuestPanel();
        }
        else
        {
            Debug.LogWarning("QuestUIHelper: No QuestUI reference found");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) // H for hide
        {
            HideAllQuestUI();
        }
    }
}