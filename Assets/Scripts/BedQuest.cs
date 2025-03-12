using UnityEngine;
using UnityEngine.SceneManagement;

public class BedQuest : Quest
{
    [SerializeField] private GameObject bed;
    [SerializeField] private string nextSceneName;
    [SerializeField] private float fadeTime = 1.0f;
    [SerializeField] private Collider triggerZone; // Assign in Inspector

    private bool playerInZone = false; // Track if player is inside the trigger

    private void Start()
    {
        if (triggerZone == null)
        {
            Debug.LogError("Trigger zone is not assigned in BedQuest!");
        }
    }

    private void Update()
    {
        // Only allow pressing E if the player is inside the trigger zone
        if (playerInZone && Input.GetKeyDown(KeyCode.E))
        {
            GoToBed();
        }
    }

    private void GoToBed()
    {
        if (!QuestManager.Instance.IsQuestActive(id)) return;

        Debug.Log("Player interacted with bed. Fading out...");

        // Trigger the fade to black and scene transition
        SceneTransitionManager.Instance.FadeToScene(nextSceneName, fadeTime);

        // Mark the quest as completed
        QuestManager.Instance.CompleteQuest(id);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            UIManager.Instance.UpdateQuestText("Press E to go to bed.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            UIManager.Instance.UpdateQuestText(""); // Clear quest text when leaving
        }
    }
}
