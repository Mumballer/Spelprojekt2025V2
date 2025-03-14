using UnityEngine;
using System.Collections;

public class QuestGiver : MonoBehaviour
{
    [Header("Quest Settings")]
    [SerializeField] private Quest questToGive;
    [SerializeField] private bool startQuestOnTriggerEnter = false;

    [Header("Dialog Integration")]
    [SerializeField] private Dialog initialDialog;
    [SerializeField] private Dialog questOfferDialog;
    [SerializeField] private Dialog acceptedDialog;
    [SerializeField] private Dialog activeQuestDialog;
    [SerializeField] private Dialog completedQuestDialog;
    [SerializeField] private bool useDialogForQuest = true;

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
        // fixar dialogen för uppdrag
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
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.OnDialogComplete -= HandleDialogComplete;
        }
    }
    
    private void HandleDialogComplete(Dialog completedDialog)
    {
    }
    
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
        // kollar efter knapptryck
        if (playerInRange && requireButtonPress && Input.GetKeyDown(interactKey))
        {
            if (useDialogForQuest)
            {
                if (dialogTrigger != null)
                {
                    dialogTrigger.dialog = GetAppropriateDialog();
                    dialogTrigger.TriggerDialog();
                }
            }
            else
            {
                GiveQuest();
            }
        }
    }

    public void GiveQuest()
    {
        // ger uppdrag till spelaren
        if (questToGive != null && !questGiven && QuestManager.Instance != null)
        {
            if (QuestManager.Instance.availableQuests.Contains(questToGive) || 
                QuestManager.Instance.completedQuests.Contains(questToGive))
            {
                Debug.Log($"Quest {questToGive.questName} already given or completed");
                return;
            }

            QuestManager.Instance.AddQuest(questToGive);
            questGiven = true;
            
            Debug.Log($"Gave quest: {questToGive.questName}");
            
            if (!useDialogForQuest && acceptedDialog != null && dialogTrigger != null)
            {
                dialogTrigger.dialog = acceptedDialog;
                dialogTrigger.TriggerDialog();
            }
        }
    }
    
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