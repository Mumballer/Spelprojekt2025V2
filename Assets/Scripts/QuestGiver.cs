using UnityEngine;
using System.Collections;

public class QuestGiver : MonoBehaviour
{
    [Header("Quest Settings")]
    [SerializeField] private Quest questToGive;
    [SerializeField] private bool startQuestOnTriggerEnter = false;

    [Header("Dialog Integration")]
    [SerializeField] private Dialog initialDialog; // Initial dialog before quest
    [SerializeField] private Dialog questOfferDialog; // Dialog offering the quest
    [SerializeField] private Dialog acceptedDialog; // Dialog after quest acceptance
    [SerializeField] private Dialog activeQuestDialog; // Dialog when quest is active
    [SerializeField] private Dialog completedQuestDialog; // Dialog after quest completion
    [SerializeField] private bool useDialogForQuest = true; // Use dialog choices to offer quest

    [Header("Interaction Settings")]
    [SerializeField] private bool requireButtonPress = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private GameObject interactionPrompt;

    private bool playerInRange = false;
    private bool questGiven = false;
    private DialogTrigger dialogTrigger;

    private void Start()
    {
        // If we're using dialog for quest, set up a DialogTrigger component
        if (useDialogForQuest && initialDialog != null)
        {
            dialogTrigger = GetComponent<DialogTrigger>();
            if (dialogTrigger == null)
            {
                dialogTrigger = gameObject.AddComponent<DialogTrigger>();
            }
            
            dialogTrigger.dialog = GetAppropriateDialog();
            dialogTrigger.triggerDistance = interactionDistance;
            dialogTrigger.interactionPrompt = interactionPrompt;
            
            // Subscribe to dialog completion events
            if (DialogManager.Instance != null)
            {
                DialogManager.Instance.OnDialogComplete += HandleDialogComplete;
            }
        }
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.OnDialogComplete -= HandleDialogComplete;
        }
    }
    
    // Handle dialog completion events
    private void HandleDialogComplete(Dialog completedDialog)
    {
        // If the completed dialog is the quest offer dialog, we don't need to do anything
        // because quest acceptance is handled by dialog choices
        
        // If needed, you could add special logic here for other dialog events
    }
    
    // Get the appropriate dialog based on quest state
    private Dialog GetAppropriateDialog()
    {
        if (questToGive == null) return initialDialog;
        
        if (QuestManager.Instance != null)
        {
            if (QuestManager.Instance.IsQuestCompleted(questToGive))
            {
                return completedQuestDialog ?? initialDialog;
            }
            else if (QuestManager.Instance.IsQuestActive(questToGive))
            {
                return activeQuestDialog ?? initialDialog;
            }
            else if (questGiven)
            {
                return acceptedDialog ?? initialDialog;
            }
            else
            {
                // If we have a specific quest offering dialog, use it
                return questOfferDialog ?? initialDialog;
            }
        }
        
        return initialDialog;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            
            if (interactionPrompt != null && !DialogManager.Instance.IsDialogActive)
            {
                interactionPrompt.SetActive(true);
            }
            
            if (startQuestOnTriggerEnter && !questGiven && !useDialogForQuest)
            {
                GiveQuest();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (playerInRange && requireButtonPress && Input.GetKeyDown(interactKey))
        {
            if (useDialogForQuest)
            {
                // Update dialog based on current quest state
                if (dialogTrigger != null)
                {
                    dialogTrigger.dialog = GetAppropriateDialog();
                    dialogTrigger.TriggerDialog();
                }
            }
            else
            {
                // Legacy direct quest giving without dialog
                GiveQuest();
            }
        }
    }

    // Original method for directly giving quests (no dialog integration)
    public void GiveQuest()
    {
        if (questToGive != null && !questGiven && QuestManager.Instance != null)
        {
            if (QuestManager.Instance.availableQuests.Contains(questToGive) || 
                QuestManager.Instance.completedQuests.Contains(questToGive))
            {
                Debug.Log($"Quest {questToGive.questName} already given or completed");
                return;
            }

            // Add the quest to the player's active quests
            QuestManager.Instance.AddQuest(questToGive);
            questGiven = true;
            
            Debug.Log($"Gave quest: {questToGive.questName}");
            
            // Trigger accepted dialog if available and not using dialog choice system
            if (!useDialogForQuest && acceptedDialog != null && dialogTrigger != null)
            {
                dialogTrigger.dialog = acceptedDialog;
                dialogTrigger.TriggerDialog();
            }
        }
    }
    
    // Called by UI button or other components to force give the quest
    public void ForceGiveQuest()
    {
        useDialogForQuest = false;
        GiveQuest();
    }

    public void HandleQuestAccepted(Quest quest)
    {
        if (quest == questToGive)
        {
            questGiven = true;
            Debug.Log($"Quest accepted: {quest.questName}");
        }
    }
} 