using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class QuestCompleteSceneLoader : MonoBehaviour
{
    [Header("Quest to Check")]
    [SerializeField] private string questNameToCheck; // Name of the quest to check
    [SerializeField] private Quest questReferenceToCheck; // Direct reference (optional)
    
    [Header("Scene Loading")]
    [SerializeField] private string sceneToLoad; // Scene to load when quest completes
    [SerializeField] private float loadDelay = 2f; // Optional delay before loading
    [SerializeField] private bool showLoadingScreen = false; // Whether to show a loading screen
    [SerializeField] private GameObject loadingScreenPrefab; // Optional loading screen
    
    [Header("Check Settings")]
    [SerializeField] private float initialDelay = 5f; // Delay before loader becomes active
    [SerializeField] private bool checkOnStart = true; // Check when script starts
    [SerializeField] private bool checkContinuously = false; // Check periodically
    [SerializeField] private float checkInterval = 1f; // How often to check (seconds)
    
    private float timeSinceLastCheck = 0f;
    private bool isLoading = false;
    private bool isActive = false; // Whether this loader is currently active
    
    private void Start()
    {
        // Start with the loader inactive
        isActive = false;
        
        // Start the activation delay
        StartCoroutine(ActivateAfterDelay());
    }
    
    private IEnumerator ActivateAfterDelay()
    {
        Debug.Log($"QuestCompleteSceneLoader will activate in {initialDelay} seconds");
        yield return new WaitForSeconds(initialDelay);
        
        // Now activate the loader
        isActive = true;
        Debug.Log("QuestCompleteSceneLoader is now active");
        
        // Subscribe to quest completion events
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        }
        
        // Initial check if configured
        if (checkOnStart)
        {
            CheckQuestAndLoadScene();
        }
    }
    
    private void OnDestroy()
    {
        // Clean up event subscription
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        }
    }
    
    private void Update()
    {
        // Don't do anything until we're active
        if (!isActive) return;
        
        if (checkContinuously && !isLoading)
        {
            timeSinceLastCheck += Time.deltaTime;
            if (timeSinceLastCheck >= checkInterval)
            {
                CheckQuestAndLoadScene();
                timeSinceLastCheck = 0f;
            }
        }
    }
    
    // Event handler for quest completion
    private void HandleQuestCompleted(Quest quest)
    {
        // Skip if not active yet
        if (!isActive) return;
        
        if (quest == questReferenceToCheck || 
            (!string.IsNullOrEmpty(questNameToCheck) && quest.questName == questNameToCheck))
        {
            LoadNextScene();
        }
    }
    
    // Public method for manual checking (can be called by buttons, triggers, etc.)
    public void CheckQuestAndLoadScene()
    {
        // Skip if not active yet
        if (!isActive) return;
        
        if (QuestManager.Instance == null || isLoading) return;
        
        bool isComplete = false;
        
        // Check with direct reference if provided
        if (questReferenceToCheck != null)
        {
            isComplete = IsQuestComplete(questReferenceToCheck);
        }
        // Otherwise check by name
        else if (!string.IsNullOrEmpty(questNameToCheck))
        {
            // Check if this quest is in completed quests list
            if (QuestManager.Instance.completedQuests != null)
            {
                foreach (var quest in QuestManager.Instance.completedQuests)
                {
                    if (quest.questName == questNameToCheck)
                    {
                        isComplete = true;
                        break;
                    }
                }
            }
        }
        
        if (isComplete)
        {
            LoadNextScene();
        }
    }
    
    // Helper method to check if a quest is complete
    private bool IsQuestComplete(Quest quest)
    {
        if (quest == null) return false;
        
        // Check if it's in the completed list
        if (QuestManager.Instance.completedQuests.Contains(quest))
        {
            return true;
        }
        
        // Require at least one objective
        if (quest.objectives == null || quest.objectives.Count == 0) 
        {
            return false; // Empty quests are not "complete"
        }
        
        // Check all objectives
        bool allObjectivesComplete = true;
        foreach (var objective in quest.objectives)
        {
            if (!objective.isCompleted)
            {
                allObjectivesComplete = false;
                break;
            }
        }
        
        return allObjectivesComplete;
    }
    
    private void LoadNextScene()
    {
        if (isLoading) return;
        
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            isLoading = true;
            Debug.Log($"Quest '{questNameToCheck}' is complete. Loading scene: {sceneToLoad}");
            
            // Show loading screen if configured
            if (showLoadingScreen && loadingScreenPrefab != null)
            {
                Instantiate(loadingScreenPrefab, Vector3.zero, Quaternion.identity);
            }
            
            // Load with optional delay
            if (loadDelay > 0)
            {
                StartCoroutine(LoadSceneWithDelay(sceneToLoad, loadDelay));
            }
            else
            {
                // Make sure the scene is in the build settings
                if (Application.CanStreamedLevelBeLoaded(sceneToLoad))
                {
                    SceneManager.LoadScene(sceneToLoad);
                }
                else
                {
                    Debug.LogError($"Scene '{sceneToLoad}' cannot be loaded. Check if it's added to the build settings.");
                    isLoading = false;
                }
            }
        }
        else
        {
            Debug.LogWarning("Scene to load is not specified!");
        }
    }
    
    private IEnumerator LoadSceneWithDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        SceneManager.LoadScene(sceneName);
    }
} 