using UnityEngine;

public class QuestUIHelper : MonoBehaviour
{
    [SerializeField] private QuestUI questUI;

    private void Start()
    {
        if (questUI == null)
        {
            questUI = FindObjectOfType<QuestUI>();
        }
    }

    public void HideAllQuestUI()
    {
        // gömmer uppdragsgränssnittet
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
        if (Input.GetKeyDown(KeyCode.H))
        {
            HideAllQuestUI();
        }
    }
}