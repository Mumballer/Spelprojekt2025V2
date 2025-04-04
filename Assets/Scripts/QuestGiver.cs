using UnityEngine;
using System.Collections;

public class QuestGiver : MonoBehaviour
{
    [Header("Quest Settings")]
    [SerializeField] private Quest questToGive; // questen som ges
    [SerializeField] private bool startQuestOnTriggerEnter = false; // ge direkt när man går nära?

    [Header("Dialog Integration")]
    [SerializeField] private Dialog initialDialog; // prat innan quest
    [SerializeField] private Dialog questOfferDialog; // prat som erbjuder quest
    [SerializeField] private Dialog postAcceptanceDialog; // prat direkt efter ja tack
    [SerializeField] private Dialog activeQuestDialog; // prat medan quest pågår
    [SerializeField] private Dialog completedQuestDialog; // prat när quest är klar
    [SerializeField] private bool useDialogForQuest = true; // använd dialogval för quest?

    [Header("Interaction Settings")]
    [SerializeField] private bool requireButtonPress = true; // måste man trycka knapp?
    [SerializeField] private KeyCode interactKey = KeyCode.E; // vilken knapp
    [SerializeField] private string playerTag = "Player"; // spelarens tag
    [SerializeField] private float interactionDistance = 3f; // avstånd för prat
    [SerializeField] private GameObject interactionPrompt; // "tryck E" prompt

    private bool playerInRange = false; // är spelaren nära?
    private bool questOfferedOrGiven = false; // har questen erbjudits?
    private bool shownPostAcceptanceDialog = false; // har "efter-ja" dialogen visats?

    private DialogTrigger dialogTrigger; // trigger-komponenten på samma objekt

    private void Start()
    {
        // sätt upp trigger om vi använder dialog
        if (useDialogForQuest)
        {
            dialogTrigger = GetComponent<DialogTrigger>(); // försök hitta
            if (dialogTrigger == null)
            {
                dialogTrigger = gameObject.AddComponent<DialogTrigger>(); // lägg till om saknas
            }

            // sätt rätt dialog från start
            dialogTrigger.dialog = GetAppropriateDialog();
            dialogTrigger.triggerDistance = interactionDistance;
            dialogTrigger.interactionPrompt = interactionPrompt;

            // lyssna på när dialoger blir klara
            if (DialogManager.Instance != null)
            {
                DialogManager.Instance.OnDialogComplete += HandleDialogComplete;
            }
        }
        // om vi INTE ger quest via dialog, men HAR en startdialog
        else if (initialDialog != null)
        {
             dialogTrigger = GetComponent<DialogTrigger>();
            if (dialogTrigger == null)
            {
                dialogTrigger = gameObject.AddComponent<DialogTrigger>();
            }
             // använd bara startdialogen
             dialogTrigger.dialog = initialDialog;
             dialogTrigger.triggerDistance = interactionDistance;
             dialogTrigger.interactionPrompt = interactionPrompt;
        }


        // göm prompten
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // kolla quest-status när spelet startar
        if (questToGive != null && QuestManager.Instance != null)
        {
            // om quest redan är aktiv/klar
            if (QuestManager.Instance.IsQuestActive(questToGive) || QuestManager.Instance.IsQuestCompleted(questToGive))
            {
                questOfferedOrGiven = true;
                // då har "efter-ja" fasen passerat
                shownPostAcceptanceDialog = true;
            }
        }
    }

    private void OnDestroy()
    {
        // sluta lyssna om vi använde dialog för quest
        if (useDialogForQuest && DialogManager.Instance != null)
        {
            DialogManager.Instance.OnDialogComplete -= HandleDialogComplete;
        }
    }

    // körs när en dialog är klar (om vi lyssnar)
    private void HandleDialogComplete(Dialog completedDialog)
    {
        // om det var erbjudande- eller efter-ja-dialogen
        if (completedDialog == questOfferDialog || completedDialog == postAcceptanceDialog)
        {
             // kolla om questen nu är aktiv
             if (questToGive != null && QuestManager.Instance != null && QuestManager.Instance.IsQuestActive(questToGive))
             {
                 questOfferedOrGiven = true; // markera som given
                 shownPostAcceptanceDialog = true; // markera "efter-ja" som visad
                 Debug.Log($"givare: dialog '{completedDialog.name}' klar. quest aktiv.");
             }
        }
    }

    // välj rätt dialog att visa nu
    private Dialog GetAppropriateDialog()
    {
        if (questToGive == null || QuestManager.Instance == null)
        {
            Debug.Log("givare: ingen quest/chef, visar startdialog.");
            return initialDialog;
        }

        // 1. är questen klar?
        if (QuestManager.Instance.IsQuestCompleted(questToGive))
        {
            Debug.Log($"givare: quest '{questToGive.questName}' klar. visar klar-dialog.");
            return completedQuestDialog ?? initialDialog; // visa klar-dialog, annars start
        }

        // 2. är questen aktiv?
        if (QuestManager.Instance.IsQuestActive(questToGive))
        {
            // 2a. ska vi visa "efter-ja" dialogen? (bara en gång)
            if (questOfferedOrGiven && !shownPostAcceptanceDialog && postAcceptanceDialog != null)
            {
                 Debug.Log($"givare: quest '{questToGive.questName}' nyss accepterad. visar efter-ja.");
                 return postAcceptanceDialog;
            }
            // 2b. visa vanliga "pågående quest"-dialogen
            else
            {
                Debug.Log($"givare: quest '{questToGive.questName}' aktiv. visar aktiv-dialog.");
                shownPostAcceptanceDialog = true; // nu är efter-ja fasen över
                return activeQuestDialog ?? initialDialog; // visa aktiv-dialog, annars start
            }
        }

        // 3. om inte klar/aktiv, har den erbjudits förut?
        if (questOfferedOrGiven)
        {
             // spelaren sa kanske nej? visa erbjudandet igen?
             Debug.Log($"givare: quest '{questToGive.questName}' erbjuden men ej aktiv. visar erbjudande/start.");
             return questOfferDialog ?? initialDialog; // visa erbjudande, annars start
        }

        // 4. annars, erbjud questen för första gången
        Debug.Log($"givare: quest '{questToGive.questName}' ej erbjuden. visar erbjudande/start.");
        return questOfferDialog ?? initialDialog; // visa erbjudande, annars start
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) // om spelaren kommer nära
        {
            playerInRange = true;

            // uppdatera dialog-triggern om vi använder den
            if (useDialogForQuest && dialogTrigger != null)
            {
                 dialogTrigger.dialog = GetAppropriateDialog(); // se till att den har rätt dialog
            }

            // visa prompten om ingen dialog pågår
            if (interactionPrompt != null && !DialogManager.Instance.IsDialogActive)
            {
                interactionPrompt.SetActive(true);
            }

            // ge quest direkt om inställt så
            if (startQuestOnTriggerEnter && !questOfferedOrGiven && !useDialogForQuest)
            {
                GiveQuest();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag)) // om spelaren går
        {
            playerInRange = false;
            // göm prompten
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    private void Update()
    {
        // om spelaren är nära, trycker E
        if (playerInRange && requireButtonPress && Input.GetKeyDown(interactKey))
        {
            // om vi använder dialog för quest
            if (useDialogForQuest)
            {
                if (dialogTrigger != null)
                {
                    // om vi just visat efter-ja, markera den som klar nu
                    if (dialogTrigger.dialog == postAcceptanceDialog)
                    {
                        shownPostAcceptanceDialog = true;
                    }

                    // hämta rätt dialog igen precis innan start
                    dialogTrigger.dialog = GetAppropriateDialog();
                    dialogTrigger.TriggerDialog(); // starta prat
                }
                else {
                     Debug.LogError($"givare {gameObject.name}: ska använda dialog men trigger saknas!");
                }
            }
            else // om vi ger quest direkt
            {
                GiveQuest();
            }
        }
    }

    // ge quest direkt utan dialogval
    public void GiveQuest()
    {
        if (questToGive != null && !questOfferedOrGiven && QuestManager.Instance != null)
        {
            // dubbelkolla om den redan finns
            if (QuestManager.Instance.IsQuestActive(questToGive) || QuestManager.Instance.IsQuestCompleted(questToGive))
            {
                Debug.Log($"quest {questToGive.questName} redan aktiv/klar");
                questOfferedOrGiven = true;
                shownPostAcceptanceDialog = true; // hoppa över efter-ja
                return;
            }

            // lägg till questen hos chefen
            QuestManager.Instance.AddQuest(questToGive);
            questOfferedOrGiven = true; // markera som erbjuden
            shownPostAcceptanceDialog = false; // redo för efter-ja dialog

            Debug.Log($"gav quest direkt: {questToGive.questName}");

            // starta efter-ja dialogen direkt (om den finns)
            if (!useDialogForQuest && postAcceptanceDialog != null && dialogTrigger != null)
            {
                dialogTrigger.dialog = postAcceptanceDialog;
                dialogTrigger.TriggerDialog();
            }
        }
    }

    // tvinga fram quest-givning (t.ex. från knapp)
    public void ForceGiveQuest()
    {
        useDialogForQuest = false; // använd direkt-metoden
        GiveQuest();
    }

    // körs från dialog-chef när quest accepteras via val
    public void HandleQuestAccepted(Quest quest)
    {
        if (quest == questToGive)
        {
            questOfferedOrGiven = true; // nu är den erbjuden
            shownPostAcceptanceDialog = false; // nollställ så efter-ja kan visas
            Debug.Log($"quest accepterad via val: {quest.questName}.");

            // uppdatera triggerns dialog direkt om spelaren är kvar
            if (playerInRange && dialogTrigger != null)
            {
                 dialogTrigger.dialog = GetAppropriateDialog();
            }
        }
    }
} 