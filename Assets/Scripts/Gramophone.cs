using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Gramophone : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource externalAudioSource;
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

    private bool isPlaying = false;
    private Coroutine fadeCoroutine;
    private bool playerInRange = false;
    private bool objectiveCompleted = false;
    private string questName;
    private float currentVolume = 0f;

    public bool IsPlaying => isPlaying;

    private void Start()
    {
        // kollar ljudkällan
        if (externalAudioSource == null)
        {
            Debug.LogError($"External AudioSource not assigned to Gramophone '{name}'! Please assign an AudioSource in the inspector.");
            return;
        }

        currentVolume = 0f;
        externalAudioSource.volume = 0;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        if (associatedQuest != null && objectiveIndex >= 0 &&
            objectiveIndex < associatedQuest.objectives.Count)
        {
            objectiveCompleted = associatedQuest.objectives[objectiveIndex].isCompleted;
        }

        if (associatedQuest != null)
        {
            Debug.Log($"<color=yellow>Gramophone '{name}' is linked to quest '{associatedQuest.questName}', objective {objectiveIndex}</color>");
        }
        else
        {
            Debug.Log($"<color=yellow>Gramophone '{name}' is not linked to any quest</color>");
        }

        CheckAndActivateIfNeeded();

        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName == "Scene1" || currentSceneName == "YourFirstSceneName")
        {
            Debug.Log($"<color=cyan>Gramophone '{gameObject.name}' is scene-specific</color>");
        }
        else
        {
            Debug.Log($"<color=cyan>Gramophone '{gameObject.name}' is scene-specific and won't persist</color>");
        }
    }

    private void OnEnable()
    {
        CheckAndActivateIfNeeded();
    }

    private void Update()
    {
        // kollar kontinuerligt
        CheckAndActivateIfNeeded();

        if (playerInRange && Input.GetKeyDown(interactKey))
        {
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

        questCheckTimer += Time.deltaTime;
        if (questCheckTimer >= QUEST_CHECK_INTERVAL)
        {
            questCheckTimer = 0f;

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
        if (associatedQuest == null)
            return true;

        bool isActive = IsQuestActive();

        Debug.Log($"<color=cyan>CanInteract check - Quest '{associatedQuest.questName}' is active: {isActive}, Objective completed: {objectiveCompleted}</color>");

        if (!requireActiveQuest || isActive)
        {
            return !objectiveCompleted || allowReplayAfterCompletion;
        }

        return false;
    }

    private void UpdatePrompt()
    {
        if (interactionPrompt == null) return;

        if (associatedQuest != null)
        {
            Debug.Log($"<color=magenta>UpdatePrompt - Quest '{associatedQuest.questName}' Active: {associatedQuest.IsActive}, ObjectiveCompleted: {objectiveCompleted}</color>");
        }

        if (!playerInRange)
        {
            interactionPrompt.SetActive(false);
            return;
        }

        interactionPrompt.SetActive(true);

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

    public void ToggleMusic()
    {
        Debug.Log($"<color=cyan>Toggling music on gramophone {name}</color>");

        isPlaying = !isPlaying;

        if (isPlaying)
        {
            if (!externalAudioSource.isPlaying)
            {
                externalAudioSource.Play();
            }
            StartMusic();

            if (completeObjectiveOnPlay)
            {
                FindMatchingQuestAndUpdate();
                CompleteQuestObjective();

                Debug.Log($"<color=cyan>Attempting to complete objective for quest: {associatedQuest?.questName}</color>");
            }
        }
        else
        {
            StopMusic();

            if (completeObjectiveOnStop)
            {
                FindMatchingQuestAndUpdate();
                CompleteQuestObjective();
            }
        }
    }

    private void CompleteQuestObjective()
    {
        if (associatedQuest != null)
        {
            FindMatchingQuestAndUpdate();

            bool isActive = IsQuestActive();
            Debug.Log($"<color=orange>CompleteQuestObjective - Quest '{associatedQuest.questName}' Active check: {isActive}</color>");

            if (isActive)
            {
                if (objectiveIndex >= 0 && objectiveIndex < associatedQuest.objectives.Count)
                {
                    if (!associatedQuest.objectives[objectiveIndex].isCompleted)
                    {
                        Debug.Log($"<color=yellow>Completing objective {objectiveIndex} for quest '{associatedQuest.questName}'</color>");

                        QuestManager.Instance.CompleteObjective(associatedQuest, objectiveIndex);

                        objectiveCompleted = true;

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

                if (QuestManager.Instance != null && objectiveIndex >= 0 && objectiveIndex < associatedQuest.objectives.Count)
                {
                    Debug.Log($"<color=orange>FORCING objective completion despite inactive quest status</color>");
                    QuestManager.Instance.CompleteObjective(associatedQuest, objectiveIndex);
                    objectiveCompleted = true;
                }
            }
        }
    }

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
                associatedQuest = quest;
                return true;
            }
        }

        return false;
    }

    private void StartMusic()
    {
        if (externalAudioSource == null) return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeAudio(0, volumeLevel, fadeTime));
    }

    private void StopMusic()
    {
        if (externalAudioSource == null) return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeAudio(currentVolume, 0, fadeTime));
    }

    private IEnumerator FadeAudio(float startVolume, float targetVolume, float duration)
    {
        float timeElapsed = 0;
        currentVolume = startVolume;
        externalAudioSource.volume = startVolume;

        while (timeElapsed < duration)
        {
            currentVolume = Mathf.Lerp(startVolume, targetVolume, timeElapsed / duration);
            externalAudioSource.volume = currentVolume;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        currentVolume = targetVolume;
        externalAudioSource.volume = targetVolume;

        if (targetVolume <= 0.01f && externalAudioSource.isPlaying)
        {
            externalAudioSource.Stop();
        }

        fadeCoroutine = null;
    }

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }

    public void CheckAndActivateIfNeeded()
    {
        if (!gameObject.activeSelf)
        {
            Debug.Log("Gramophone was inactive - activating now");
            gameObject.SetActive(true);
        }

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

    private void Awake()
    {
        if (associatedQuest != null)
        {
            questName = associatedQuest.questName;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        Debug.Log($"<color=cyan>Gramophone '{gameObject.name}' is scene-specific</color>");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ReconnectToQuestAfterDelay());
    }

    private IEnumerator ReconnectToQuestAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        if (!string.IsNullOrEmpty(questName) && QuestManager.Instance != null)
        {
            Debug.Log($"<color=orange>Gramophone attempting to reconnect to quest: {questName}</color>");

            foreach (var quest in QuestManager.Instance.activeQuests)
            {
                if (quest.questName == questName)
                {
                    associatedQuest = quest;
                    Debug.Log($"<color=orange>Gramophone successfully reconnected to quest: {questName}</color>");

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

    private float questCheckTimer = 0f;
    private const float QUEST_CHECK_INTERVAL = 1.0f;

    private bool IsQuestActive()
    {
        if (associatedQuest == null) return false;

        if (associatedQuest.IsActive) return true;

        if (QuestManager.Instance == null) return false;

        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            if (quest.questName == associatedQuest.questName)
                return true;
        }

        return false;
    }
}