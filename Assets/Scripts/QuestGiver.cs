using UnityEngine;
using UnityEngine.Events;

public class QuestGiver : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private Quest questToGive;
    
    [Header("Interaction Settings")]
    [SerializeField] private bool startQuestOnTriggerEnter = false;
    [SerializeField] private bool requireButtonPress = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";
    
    [Header("UI Elements (Optional)")]
    [SerializeField] private GameObject interactionPrompt;
    
    [Header("Events")]
    public UnityEvent OnQuestAccepted;
    
    private bool playerInRange = false;
    private bool questGiven = false;
    
    private void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            
            if (interactionPrompt != null && !questGiven)
                interactionPrompt.SetActive(true);
            
            if (startQuestOnTriggerEnter && !questGiven)
                GiveQuest();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (requireButtonPress && playerInRange && Input.GetKeyDown(interactKey) && !questGiven)
        {
            GiveQuest();
        }
    }
    
    public void GiveQuest()
    {
        if (questToGive == null || QuestManager.Instance == null || questGiven)
            return;
        
        // Check if quest is already active or completed
        if (QuestManager.Instance.IsQuestActive(questToGive) || QuestManager.Instance.IsQuestCompleted(questToGive))
        {
            Debug.Log($"Quest '{questToGive.questName}' is already active or completed.");
            return;
        }
        
        // Add the quest to the player's active quests
        QuestManager.Instance.AddQuest(questToGive);
        
        // Mark as given so we don't give it again
        questGiven = true;
        
        // Hide the interaction prompt if we have one
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
        
        // Trigger event
        OnQuestAccepted?.Invoke();
        
        Debug.Log($"Gave quest: {questToGive.questName}");
    }

    // Call this to reset the quest giver (for example, if you want to allow the quest to be taken again)
    public void ResetQuestGiver()
    {
        questGiven = false;
        
        if (playerInRange && interactionPrompt != null)
            interactionPrompt.SetActive(true);
        
        Debug.Log($"Quest giver reset for quest: {questToGive.questName}");
    }
} 