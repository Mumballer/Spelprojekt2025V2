using UnityEngine;
using UnityEngine.SceneManagement;

public class BedQuest : Quest
{
    [SerializeField] private GameObject bed;
    [SerializeField] private string nextSceneName;
    [SerializeField] private float fadeTime = 1.0f;

    private void Start()
    {
        // Make sure the bed has an Interactable component
        if (!bed.GetComponent<Interactable>())
        {
            bed.AddComponent<Interactable>();
        }

        // Register the GoToBed method to be called when the player interacts with the bed
        bed.GetComponent<Interactable>().onInteract.AddListener(GoToBed);
    }

    // This method will be called when the player presses E near the bed
    private void GoToBed()
    {
        // Check if this quest is currently active
        if (QuestManager.Instance.IsQuestActive(id))
        {
            // Trigger the fade to black and scene transition
            SceneTransitionManager.Instance.FadeToScene(nextSceneName, fadeTime);

            // Mark the quest as completed
            QuestManager.Instance.CompleteQuest(id);
        }
    }

    // Optionally override OnActivate if you want special behavior when this quest becomes active
    public override void OnActivate()
    {
        base.OnActivate(); // Call the base class method

        // For example, you might want to highlight the bed or show an arrow pointing to it
        // UIManager.Instance.ShowObjective("Go to bed");
    }

    public override void OnComplete()
    {
        base.OnComplete();
        Debug.Log("Quest completed!");

        // Keep track of completed quests
        UIManager.Instance.UpdateQuestText("✔ " + questTitle);
    }

}