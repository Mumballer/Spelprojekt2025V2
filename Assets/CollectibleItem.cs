using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("Quest Settings")]
    [SerializeField] private Quest associatedQuest;
    [SerializeField] private int objectiveIndex;

    [Header("Collection Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool requireButtonPress = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool destroyOnCollect = true;

    [Header("UI Settings")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private string promptText = "Press E to collect";

    [Header("Hover UI Settings")]
    [SerializeField] private GameObject hoverUIElement;
    [SerializeField] private float hoverUIDisplayTime = 0f;
    [SerializeField] private bool showOnlyForActiveQuest = true;

    private bool isPlayerInRange = false;
    private bool isCollected = false;
    private float hoverUITimer = 0f;

    private void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        if (hoverUIElement != null)
        {
            hoverUIElement.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // spelaren är nära objektet
        if (other.CompareTag(playerTag) && !isCollected)
        {
            isPlayerInRange = true;

            if (associatedQuest != null)
            {
                bool isActive = CheckQuestIsActive();
                
                Debug.Log($"<color=magenta>CollectibleItem - OnTriggerEnter - Quest '{associatedQuest.questName}' active: {isActive}</color>");
                
                if (isActive && objectiveIndex < associatedQuest.objectives.Count &&
                    !associatedQuest.objectives[objectiveIndex].isCompleted)
                {
                    ShowPrompt();
                    
                    if (hoverUIElement != null && showOnlyForActiveQuest)
                    {
                        hoverUIElement.SetActive(true);
                        hoverUITimer = hoverUIDisplayTime;
                    }
                    
                    if (!requireButtonPress)
                    {
                        CollectItem();
                    }
                }
                else if (hoverUIElement != null && !showOnlyForActiveQuest)
                {
                    hoverUIElement.SetActive(true);
                    hoverUITimer = hoverUIDisplayTime;
                }
            }
            else if (interactionPrompt != null)
            {
                ShowPrompt();
                
                if (hoverUIElement != null)
                {
                    hoverUIElement.SetActive(true);
                    hoverUITimer = hoverUIDisplayTime;
                }
                
                if (!requireButtonPress)
                {
                    CollectItem();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
            
            if (hoverUIElement != null)
            {
                hoverUIElement.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (requireButtonPress && isPlayerInRange && Input.GetKeyDown(interactKey) && !isCollected)
        {
            CollectItem();
        }
        
        if (hoverUIElement != null && hoverUIElement.activeSelf && hoverUIDisplayTime > 0)
        {
            hoverUITimer -= Time.deltaTime;
            if (hoverUITimer <= 0)
            {
                hoverUIElement.SetActive(false);
            }
        }
    }

    private void ShowPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
            
            TMPro.TextMeshProUGUI promptTextComponent = 
                interactionPrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (promptTextComponent != null)
            {
                promptTextComponent.text = promptText;
            }
        }
    }

    private bool CheckQuestIsActive()
    {
        if (associatedQuest == null)
            return false;
        
        if (associatedQuest.IsActive)
            return true;
        
        if (QuestManager.Instance != null)
        {
            foreach (var quest in QuestManager.Instance.activeQuests)
            {
                if (quest.questName == associatedQuest.questName)
                    return true;
            }
        }
        
        return false;
    }

    public void CollectItem()
    {
        // samlar in föremålet
        if (isCollected) return;

        if (associatedQuest != null && QuestManager.Instance != null)
        {
            bool isActive = CheckQuestIsActive();
            Debug.Log($"<color=magenta>CollectibleItem - Collecting - Quest '{associatedQuest.questName}' active: {isActive}</color>");
            
            if (!isActive)
            {
                Debug.Log("<color=red>Cannot collect item - quest is not active</color>");
                return;
            }

            Debug.Log($"<color=green>Completing objective {objectiveIndex} for quest {associatedQuest.questName}</color>");

            QuestManager.Instance.CompleteObjective(associatedQuest, objectiveIndex);

            isCollected = true;

            Debug.Log("Quest objectives status after collection:");
            foreach (var objective in associatedQuest.objectives)
            {
                Debug.Log($"- {objective.description}: {(objective.isCompleted ? "Completed" : "Incomplete")}");
            }
            Debug.Log($"Quest completion status: {associatedQuest.IsCompleted}");

            Debug.Log($"Collected item for quest: {associatedQuest.questName}, objective: {objectiveIndex}");

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            if (hoverUIElement != null)
            {
                hoverUIElement.SetActive(false);
            }

            if (destroyOnCollect)
            {
                Destroy(gameObject);
            }
            else
            {
                MeshRenderer renderer = GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = false;
                }

                Collider itemCollider = GetComponent<Collider>();
                if (itemCollider != null)
                {
                    itemCollider.enabled = false;
                }
            }
        }
    }
}