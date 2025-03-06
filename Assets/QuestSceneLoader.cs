using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class QuestSceneLoader : MonoBehaviour
{
    [Header("Quest Reference")]
    [SerializeField] private Quest questToCheck;

    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad;
    [SerializeField] private float loadDelay = 2.0f;

    [Header("Check Settings")]
    [SerializeField] private bool checkOnStart = false;
    [SerializeField] private bool checkContinuously = true; // Changed default to true
    [SerializeField] private float checkInterval = 0.5f;

    [Header("Debug Options")]
    [SerializeField] private bool debugMode = true;
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private bool forceLoadScene = false;

    private bool hasLoadedScene = false;
    private float timeSinceLastCheck = 0f;

    private void Start()
    {
        Debug.Log($"QuestSceneLoader initialized. Watching quest: {questToCheck?.questName ?? "NULL"}");

        if (questToCheck == null)
        {
            LogError("No quest assigned to QuestSceneLoader!");
            return;
        }

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            LogError("No target scene specified in QuestSceneLoader!");
            return;
        }

        // Check if scene exists in build settings
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == sceneToLoad)
            {
                sceneExists = true;
                break;
            }
        }

        if (!sceneExists)
        {
            LogError($"Scene '{sceneToLoad}' is not included in build settings! Scene loading will fail.");
        }

        if (checkOnStart)
        {
            CheckQuestAndLoadScene();
        }

        // Update debug info
        UpdateDebugText();
    }

    private void Update()
    {
        if (hasLoadedScene)
            return;

        if (forceLoadScene)
        {
            forceLoadScene = false;
            LogDebug("Force loading scene requested");
            LoadTargetScene();
            return;
        }

        if (checkContinuously && questToCheck != null)
        {
            timeSinceLastCheck += Time.deltaTime;

            if (timeSinceLastCheck >= checkInterval)
            {
                CheckQuestAndLoadScene();
                timeSinceLastCheck = 0f;

                // Update debug info periodically
                UpdateDebugText();
            }
        }
    }

    // Can be called by other scripts when needed
    public void CheckQuestAndLoadScene()
    {
        if (hasLoadedScene || questToCheck == null)
            return;

        bool isComplete = questToCheck.IsCompleted;
        LogDebug($"Checking quest '{questToCheck.questName}' completion status: {isComplete}");

        if (isComplete)
        {
            LogDebug($"Quest '{questToCheck.questName}' is completed! Preparing to load scene.");
            hasLoadedScene = true;
            StartCoroutine(LoadSceneAfterDelay());
        }
    }

    private System.Collections.IEnumerator LoadSceneAfterDelay()
    {
        LogDebug($"Loading scene '{sceneToLoad}' in {loadDelay} seconds.");

        // Show a countdown if debug text is available
        float remainingTime = loadDelay;
        while (remainingTime > 0)
        {
            remainingTime -= 0.1f;
            UpdateDebugText($"LOADING SCENE: {sceneToLoad} in {remainingTime:F1} seconds...");
            yield return new WaitForSeconds(0.1f);
        }

        LoadTargetScene();
    }

    private void LoadTargetScene()
    {
        LogDebug($"NOW LOADING SCENE: {sceneToLoad}");

        try
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        catch (System.Exception e)
        {
            LogError($"Failed to load scene '{sceneToLoad}': {e.Message}");
        }
    }

    private void UpdateDebugText(string additionalInfo = "")
    {
        if (!debugMode || debugText == null)
            return;

        string questStatus = questToCheck != null ?
            $"Quest: {questToCheck.questName} - Complete: {questToCheck.IsCompleted}" :
            "No quest assigned";

        string objectiveStatus = "";
        if (questToCheck != null && questToCheck.Objectives != null)
        {
            for (int i = 0; i < questToCheck.Objectives.Count; i++)
            {
                var obj = questToCheck.Objectives[i];
                objectiveStatus += $"\n - Obj {i}: {obj.description} [{(obj.isCompleted ? "✓" : "×")}]";
            }
        }

        debugText.text = $"QuestSceneLoader Debug:\n{questStatus}{objectiveStatus}";

        if (!string.IsNullOrEmpty(additionalInfo))
        {
            debugText.text += $"\n\n{additionalInfo}";
        }
    }

    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[QuestSceneLoader] {message}");
            UpdateDebugText();
        }
    }

    private void LogError(string message)
    {
        Debug.LogError($"[QuestSceneLoader] {message}");
        UpdateDebugText($"ERROR: {message}");
    }
}