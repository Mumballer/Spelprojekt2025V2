using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class QuestCompleteSceneLoader : MonoBehaviour
{
    [Header("Quest to Check")]
    [SerializeField] private string questNameToCheck;
    [SerializeField] private Quest questReferenceToCheck;
    
    [Header("Scene Loading")]
    [SerializeField] private string sceneToLoad;
    [SerializeField] private float loadDelay = 2f;
    [SerializeField] private bool showLoadingScreen = false;
    [SerializeField] private GameObject loadingScreenPrefab;
    
    [Header("Check Settings")]
    [SerializeField] private float initialDelay = 5f;
    [SerializeField] private bool checkOnStart = false;
    [SerializeField] private bool checkContinuously = false;
    [SerializeField] private bool requireExplicitCompletion = true;
    [SerializeField] private float checkInterval = 1f;
    
    private float timeSinceLastCheck = 0f;
    private bool isLoading = false;
    private bool isActive = false;
    
    private void Start()
    {
        isActive = false;
        
        StartCoroutine(ActivateAfterDelay());
    }
    
    private IEnumerator ActivateAfterDelay()
    {
        // väntar innan aktivering
        Debug.Log($"QuestCompleteSceneLoader will activate in {initialDelay} seconds");
        yield return new WaitForSeconds(initialDelay);
        
        isActive = true;
        Debug.Log("QuestCompleteSceneLoader is now active");
        
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        }
        
        if (checkOnStart)
        {
            CheckQuestAndLoadScene();
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        }
    }
    
    private void Update()
    {
        if (!isActive || isLoading) return;
        
        if (checkContinuously)
        {
            timeSinceLastCheck += Time.deltaTime;
            
            if (timeSinceLastCheck >= checkInterval)
            {
                timeSinceLastCheck = 0f;
                CheckQuestAndLoadScene();
            }
        }
    }
    
    private void HandleQuestCompleted(Quest completedQuest)
    {
        // kollar uppdrag är klart
        if (!isActive || isLoading) return;
        
        if (questReferenceToCheck != null)
        {
            if (completedQuest == questReferenceToCheck)
            {
                LoadNextScene();
            }
        }
        else if (!string.IsNullOrEmpty(questNameToCheck))
        {
            if (completedQuest.questName == questNameToCheck)
            {
                LoadNextScene();
            }
        }
    }
    
    private void CheckQuestAndLoadScene()
    {
        if (isLoading) return;
        
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("No QuestManager instance found!");
            return;
        }
        
        Quest questToCheck = null;
        
        if (questReferenceToCheck != null)
        {
            questToCheck = questReferenceToCheck;
        }
        else if (!string.IsNullOrEmpty(questNameToCheck))
        {
            foreach (var quest in QuestManager.Instance.completedQuests)
            {
                if (quest.questName == questNameToCheck)
                {
                    questToCheck = quest;
                    break;
                }
            }
            
            if (questToCheck == null)
            {
                foreach (var quest in QuestManager.Instance.activeQuests)
                {
                    if (quest.questName == questNameToCheck)
                    {
                        questToCheck = quest;
                        break;
                    }
                }
            }
        }
        
        if (questToCheck != null)
        {
            bool isCompleted = false;
            
            if (requireExplicitCompletion)
            {
                isCompleted = QuestManager.Instance.IsQuestCompleted(questToCheck);
            }
            else
            {
                isCompleted = AreAllObjectivesComplete(questToCheck);
            }
            
            if (isCompleted)
            {
                LoadNextScene();
            }
        }
    }
    
    private bool AreAllObjectivesComplete(Quest quest)
    {
        if (quest == null || quest.objectives == null) return false;
        
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
        // laddar nästa scen
        if (isLoading) return;
        
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            isLoading = true;
            Debug.Log($"Quest '{questNameToCheck}' is complete. Loading scene: {sceneToLoad}");
            
            if (showLoadingScreen && loadingScreenPrefab != null)
            {
                Instantiate(loadingScreenPrefab, Vector3.zero, Quaternion.identity);
            }
            
            if (loadDelay > 0)
            {
                StartCoroutine(LoadSceneWithDelay(sceneToLoad, loadDelay));
            }
            else
            {
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