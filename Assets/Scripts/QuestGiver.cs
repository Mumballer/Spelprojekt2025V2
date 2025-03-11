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
    
    [Header("UI Elements")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private string promptText = "Press E to talk";
    
    [Header("Events")]
    public UnityEvent OnQuestAccepted;
    
    private bool playerInRange = false;
    private bool questGiven = false;
    
    private void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
            
        startQuestOnTriggerEnter = false;
        requireButtonPress = true;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            
            if (interactionPrompt != null && !questGiven)
            {
                interactionPrompt.SetActive(true);
                
                TMPro.TextMeshProUGUI promptTextComponent = interactionPrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (promptTextComponent != null)
                {
                    promptTextComponent.text = promptText;
                }
            }
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
        
        if (QuestManager.Instance.IsQuestActive(questToGive) || QuestManager.Instance.IsQuestCompleted(questToGive))
        {
            Debug.Log($"Quest '{questToGive.questName}' is already active or completed.");
            return;
        }
        
        QuestManager.Instance.AddQuest(questToGive);
        
        questGiven = true;
        
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
        
        HandleQuestAccepted(questToGive);
        
        OnQuestAccepted?.Invoke();
        
        Debug.Log($"Gave quest: {questToGive.questName}");
    }

    public void HandleQuestAccepted(Quest quest)
    {
        if (GetComponent<Animator>())
        {
            GetComponent<Animator>().SetTrigger("QuestGiven");
        }
        
        Debug.Log($"Quest accepted: {quest.questName}");
    }

    public void ResetQuestGiver()
    {
        questGiven = false;
        
        if (playerInRange && interactionPrompt != null)
            interactionPrompt.SetActive(true);
        
        Debug.Log($"Quest giver reset for quest: {questToGive.questName}");
    }
} 