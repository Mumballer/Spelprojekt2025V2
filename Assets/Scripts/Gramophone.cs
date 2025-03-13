using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Gramophone : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private float volumeLevel = 0.5f;
    [SerializeField] private float fadeTime = 1.0f;
    
    [Header("Interaction Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool requireButtonPress = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 3f;
    
    [Header("Quest Integration")]
    [SerializeField] private Quest associatedQuest;
    [SerializeField] private int objectiveIndex;
    [SerializeField] private bool requireActiveQuest = true;
    [SerializeField] private bool completeObjectiveOnPlay = true;
    [SerializeField] private bool completeObjectiveOnStop = false;
    
    [Header("UI Settings")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private string playPromptText = "Press E to play music";
    [SerializeField] private string stopPromptText = "Press E to stop music";
    [SerializeField] private string lockedPromptText = "This gramophone is locked";

    [Header("Advanced Settings")]
    [SerializeField] private bool allowReplayAfterCompletion = false;

    private AudioSource audioSource;
    private bool isPlaying = false;
    private Coroutine fadeCoroutine;
    private bool playerInRange = false;
    private bool objectiveCompleted = false;
    private string questName; // Store quest name for reconnection

    // Public property to check if music is playing
    public bool IsPlaying => isPlaying;

    private void Start()
    {
        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = musicClip;
        audioSource.volume = 0;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
        
        // Initialize UI
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // Check if objective is already completed BUT DON'T COMPLETE IT
        if (associatedQuest != null && objectiveIndex >= 0 && 
            objectiveIndex < associatedQuest.objectives.Count)
        {
            objectiveCompleted = associatedQuest.objectives[objectiveIndex].isCompleted;
            // Don't add any code here that might complete the objective
        }

        // Add enhanced quest association logging
        if (associatedQuest != null)
        {
            Debug.Log($"<color=yellow>Gramophone '{name}' is linked to quest '{associatedQuest.questName}', objective {objectiveIndex}</color>");
        }
        else
        {
            Debug.Log($"<color=yellow>Gramophone '{name}' is not linked to any quest</color>");
        }

        // Call immediately on start
        CheckAndActivateIfNeeded();

        // Only persist in certain scenes (e.g., Scene1)
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName == "Scene1" || currentSceneName == "YourFirstSceneName") 
        {
            Debug.Log($"<color=cyan>Gramophone '{gameObject.name}' is scene-specific</color>");
        }
        else
        {
            // Don't persist this gramophone if it's in other scenes
            Debug.Log($"<color=cyan>Gramophone '{gameObject.name}' is scene-specific and won't persist</color>");
        }
    }
    
    private void OnEnable()
    {
        // Also call when object is enabled
        CheckAndActivateIfNeeded();
    }
    
    // Add this to ensure the gramophone stays active
    private void Update()
    {
        // Check periodically while game is running
        CheckAndActivateIfNeeded();

        // Handle player interaction
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            // Check if interaction is allowed based on quest state
            if (CanInteract()) 
            {
                Debug.Log($"<color=cyan>Player interacting with gramophone {name}</color>");
                ToggleMusic();
                UpdatePrompt();
            }
            else
            {
                Debug.Log($"<color=red>Gramophone is locked or quest objective already completed</color>");
            }
        }

        // Quest reconnection code...
        questCheckTimer += Time.deltaTime;
        if (questCheckTimer >= QUEST_CHECK_INTERVAL)
        {
            questCheckTimer = 0f;
            
            // If our quest is not active, try to find it
            if (associatedQuest != null && !associatedQuest.IsActive)
            {
                if (FindMatchingQuestAndUpdate())
                {
                    Debug.Log($"<color=lime>Successfully reconnected to active quest: {associatedQuest.questName}</color>");
                    UpdatePrompt();
                }
            }
        }
    }
    
    private bool CanInteract()
    {
        // If no quest restrictions, always allow interaction
        if (associatedQuest == null)
            return true;
        
        // Use our custom active check
        bool isActive = IsQuestActive();
        
        // Show debug info
        Debug.Log($"<color=cyan>CanInteract check - Quest '{associatedQuest.questName}' is active: {isActive}, Objective completed: {objectiveCompleted}</color>");
        
        if (!requireActiveQuest || isActive)
        {
            return !objectiveCompleted || allowReplayAfterCompletion;
        }
        
        return false;
    }
    
    private void UpdatePrompt()
    {
        // Safety check
        if (interactionPrompt == null) return;

        // Debug quest state during prompt update
        if (associatedQuest != null)
        {
            Debug.Log($"<color=magenta>UpdatePrompt - Quest '{associatedQuest.questName}' Active: {associatedQuest.IsActive}, ObjectiveCompleted: {objectiveCompleted}</color>");
        }

        if (!playerInRange)
        {
            interactionPrompt.SetActive(false);
            return;
        }

        // Show prompt when player is in range
        interactionPrompt.SetActive(true);

        // Update interaction message based on state
        TMPro.TextMeshProUGUI promptText = interactionPrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (promptText != null)
        {
            if (isPlaying)
            {
                promptText.text = stopPromptText;
            }
            else
            {
                promptText.text = playPromptText;
            }
            
            Debug.Log($"<color=magenta>Prompt text set to: {promptText.text}</color>");
        }
    }

    // Public method for toggling music state
    public void ToggleMusic()
    {
        Debug.Log($"<color=cyan>Toggling music on gramophone {name}</color>");
        
        isPlaying = !isPlaying;
        
        if (isPlaying)
        {
            // Start playing
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
            StartMusic();
            
            // Complete quest objective if required
            if (completeObjectiveOnPlay)
            {
                // Make sure quest is up to date before completing
                FindMatchingQuestAndUpdate(); 
                CompleteQuestObjective();
                
                // Log objective completion attempt
                Debug.Log($"<color=cyan>Attempting to complete objective for quest: {associatedQuest?.questName}</color>");
            }
        }
        else
        {
            // Stop playing
            StopMusic();
            
            // Complete quest objective if required
            if (completeObjectiveOnStop)
            {
                // Make sure quest is up to date before completing
                FindMatchingQuestAndUpdate();
                CompleteQuestObjective();
            }
        }
    }
    
    private void CompleteQuestObjective()
    {
        if (associatedQuest != null)
        {
            // Try to find active quest with the same name
            FindMatchingQuestAndUpdate();
            
            // Use our custom active check instead of relying on the quest's IsActive property
            bool isActive = IsQuestActive();
            Debug.Log($"<color=orange>CompleteQuestObjective - Quest '{associatedQuest.questName}' Active check: {isActive}</color>");
            
            if (isActive) // Use our custom check here
            {
                if (objectiveIndex >= 0 && objectiveIndex < associatedQuest.objectives.Count)
                {
                    if (!associatedQuest.objectives[objectiveIndex].isCompleted)
                    {
                        Debug.Log($"<color=yellow>Completing objective {objectiveIndex} for quest '{associatedQuest.questName}'</color>");
                        
                        // Use the QuestManager instance to complete the objective
                        QuestManager.Instance.CompleteObjective(associatedQuest, objectiveIndex);
                        
                        objectiveCompleted = true;
                        
                        // Log completion status
                        Debug.Log($"<color=green>Quest objective completed! Quest status: {associatedQuest.IsCompleted}</color>");
                    }
                    else
                    {
                        Debug.Log($"<color=yellow>Objective {objectiveIndex} already completed for quest '{associatedQuest.questName}'</color>");
                    }
                }
                else
                {
                    Debug.LogWarning($"Invalid objective index {objectiveIndex} for quest {associatedQuest.questName}");
                }
            }
            else
            {
                Debug.Log($"<color=red>Cannot complete objective - quest is not active (custom check)</color>");
                
                // Force-complete the objective anyway as a fallback
                if (QuestManager.Instance != null && objectiveIndex >= 0 && objectiveIndex < associatedQuest.objectives.Count)
                {
                    Debug.Log($"<color=orange>FORCING objective completion despite inactive quest status</color>");
                    QuestManager.Instance.CompleteObjective(associatedQuest, objectiveIndex);
                    objectiveCompleted = true;
                }
            }
        }
    }

    // New method to find matching quest by name in active quests
    private bool FindMatchingQuestAndUpdate()
    {
        if (associatedQuest == null || QuestManager.Instance == null)
            return false;
        
        string questName = associatedQuest.questName;
        
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            if (quest.questName == questName)
            {
                Debug.Log($"<color=lime>Gramophone found matching active quest: {questName}</color>");
                associatedQuest = quest; // Update to the active instance
                return true;
            }
        }
        
        return false;
    }

    private void StartMusic()
    {
        if (audioSource == null) return;
        
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        fadeCoroutine = StartCoroutine(FadeAudio(0, volumeLevel, fadeTime));
    }

    private void StopMusic()
    {
        if (audioSource == null) return;
        
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        fadeCoroutine = StartCoroutine(FadeAudio(audioSource.volume, 0, fadeTime));
    }

    private IEnumerator FadeAudio(float startVolume, float targetVolume, float duration)
    {
        float timeElapsed = 0;
        audioSource.volume = startVolume;

        while (timeElapsed < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = targetVolume;

        if (targetVolume <= 0.01f && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        fadeCoroutine = null;
    }

    // For editor testing
    public void ForcePlayMusic()
    {
        isPlaying = true;
        StartMusic();
        UpdatePrompt();
    }

    public void ForceStopMusic()
    {
        isPlaying = false;
        StopMusic();
        UpdatePrompt();
    }
    
    // Draw gizmo for interaction range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }

    public void CheckAndActivateIfNeeded()
    {
        // Check if this gameObject is inactive
        if (!gameObject.activeSelf)
        {
            Debug.Log("Gramophone was inactive - activating now");
            gameObject.SetActive(true);
        }
        
        // Also check any child objects that need to be active
        if (transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                if (!child.gameObject.activeSelf)
                {
                    Debug.Log($"Gramophone child '{child.name}' was inactive - activating now");
                    child.gameObject.SetActive(true);
                }
            }
        }
    }
    
    // Optional - make the gramophone persistent between scenes
    private void Awake()
    {
        // Store quest name for quest tracking
        if (associatedQuest != null)
        {
            questName = associatedQuest.questName;
        }
        
        // Subscribe to scene loaded events for quest reconnection
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        Debug.Log($"<color=cyan>Gramophone '{gameObject.name}' is scene-specific</color>");
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reconnect to quest when a new scene loads
        StartCoroutine(ReconnectToQuestAfterDelay());
    }

    private IEnumerator ReconnectToQuestAfterDelay()
    {
        // Wait for QuestManager to initialize in new scene
        yield return new WaitForSeconds(0.5f);
        
        if (!string.IsNullOrEmpty(questName) && QuestManager.Instance != null)
        {
            Debug.Log($"<color=orange>Gramophone attempting to reconnect to quest: {questName}</color>");
            
            // Look for quest with matching name in active quests
            foreach (var quest in QuestManager.Instance.activeQuests)
            {
                if (quest.questName == questName)
                {
                    associatedQuest = quest;
                    Debug.Log($"<color=orange>Gramophone successfully reconnected to quest: {questName}</color>");
                    
                    // Reset objective completed flag
                    objectiveCompleted = false;
                    if (objectiveIndex >= 0 && objectiveIndex < associatedQuest.objectives.Count)
                    {
                        objectiveCompleted = associatedQuest.objectives[objectiveIndex].isCompleted;
                    }
                    yield break;
                }
            }
            
            Debug.LogWarning($"<color=orange>Failed to find quest '{questName}' in active quests!</color>");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            UpdatePrompt();
            Debug.Log($"Player entered gramophone range: {name}");
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
            Debug.Log($"Player exited gramophone range: {name}");
        }
    }

    // Add this to update method to periodically check for quest updates
    private float questCheckTimer = 0f;
    private const float QUEST_CHECK_INTERVAL = 1.0f;

    // Add this method to override the IsActive check
    private bool IsQuestActive()
    {
        if (associatedQuest == null) return false;
        
        // First check the object's own property
        if (associatedQuest.IsActive) return true;
        
        // Then double-check if it's in the active quests list
        if (QuestManager.Instance == null) return false;
        
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            if (quest.questName == associatedQuest.questName)
                return true;
        }
        
        return false;
    }
}