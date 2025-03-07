using UnityEngine;
using System.Collections;

public class MusicQuestActivator : MonoBehaviour
{
    [Header("Quest References")]
    [SerializeField] private MusicQuest musicQuest; // Assign in inspector
    [SerializeField] private bool findQuestIfNotAssigned = true;

    [Header("Activation Settings")]
    [SerializeField] private bool activateOnStart = true;
    [SerializeField] private float activationDelay = 0.5f; // Delay before activation

    [Header("Music Control")]
    [SerializeField] private bool autoStartMusic = false; // Automatically complete the first objective
    [SerializeField] private float autoStartDelay = 2.0f; // Delay before auto-starting music
    [SerializeField] private bool autoStopMusic = false; // Automatically complete the second objective
    [SerializeField] private float autoStopDelay = 5.0f; // Delay before auto-stopping music
    [SerializeField] private GameObject gramophoneToFind; // Optional reference to gramophone

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private bool questActivated = false;
    private bool musicStarted = false;
    private bool musicStopped = false;

    private void Start()
    {
        if (activateOnStart)
        {
            StartCoroutine(ActivateWithDelay());
        }
    }

    private IEnumerator ActivateWithDelay()
    {
        // Short delay to ensure all systems are initialized
        yield return new WaitForSeconds(activationDelay);

        ActivateMusicQuest();

        // Auto-start music if enabled
        if (autoStartMusic && musicQuest != null)
        {
            yield return new WaitForSeconds(autoStartDelay);
            CompleteStartMusicObjective();
        }

        // Auto-stop music if enabled
        if (autoStopMusic && musicQuest != null && musicQuest.requireBothStartAndStop)
        {
            yield return new WaitForSeconds(autoStopDelay);
            CompleteStopMusicObjective();
        }
    }

    // Can be called manually via other scripts or events
    public void ActivateMusicQuest()
    {
        if (questActivated) return; // Only activate once

        // Find music quest if not assigned
        if (musicQuest == null && findQuestIfNotAssigned)
        {
            FindMusicQuest();
        }

        // Activate the quest if found
        if (musicQuest != null && QuestManager.Instance != null)
        {
            // Check if quest is already active
            if (!QuestManager.Instance.IsQuestActive(musicQuest))
            {
                DebugLog($"Adding music quest: {musicQuest.questName}");
                QuestManager.Instance.AddQuest(musicQuest);

                // Verify quest was added successfully
                if (QuestManager.Instance.IsQuestActive(musicQuest))
                {
                    DebugLog("Music quest activated successfully");
                    questActivated = true;

                    // Force setup to ensure objectives are properly set
                    musicQuest.SetupMusicQuest();
                }
                else
                {
                    DebugLog("Failed to activate music quest", true);
                }
            }
            else
            {
                DebugLog("Music quest already active");
                questActivated = true;
            }
        }
        else
        {
            DebugLog("Music quest or QuestManager not found", true);
        }
    }

    // Complete the first objective (start music)
    public void CompleteStartMusicObjective()
    {
        if (musicStarted) return; // Only complete once

        if (musicQuest != null && musicQuest.Objectives.Count > 0 && QuestManager.Instance != null)
        {
            DebugLog("Completing 'start music' objective");

            // Complete the first objective (starting music)
            QuestManager.Instance.CompleteObjective(musicQuest, 0);
            musicStarted = true;

            // Find and activate gramophone if needed
            if (gramophoneToFind == null)
            {
                FindGramophone();
            }

            if (gramophoneToFind != null)
            {
                // Try to find and call PlayMusic method on the gramophone
                AudioSource audioSource = gramophoneToFind.GetComponent<AudioSource>();
                if (audioSource != null && !audioSource.isPlaying)
                {
                    audioSource.Play();
                    DebugLog("Started playing music on gramophone");
                }

                // Try to find and call a PlayMusic method if it exists
                var playMethod = gramophoneToFind.GetType().GetMethod("PlayMusic");
                if (playMethod != null)
                {
                    playMethod.Invoke(gramophoneToFind, null);
                    DebugLog("Called PlayMusic on gramophone");
                }
            }

            // Check if quest should auto-complete on music start
            if (musicQuest.completeOnMusicStart)
            {
                musicQuest.ForceCompletionCheck();
            }
        }
        else
        {
            DebugLog("Cannot complete 'start music' objective - quest not properly set up", true);
        }
    }

    // Complete the second objective (stop music)
    public void CompleteStopMusicObjective()
    {
        if (musicStopped) return; // Only complete once

        if (musicQuest != null && musicQuest.requireBothStartAndStop &&
            musicQuest.Objectives.Count > 1 && QuestManager.Instance != null)
        {
            DebugLog("Completing 'stop music' objective");

            // Complete the second objective (stopping music)
            QuestManager.Instance.CompleteObjective(musicQuest, 1);
            musicStopped = true;

            // Find and deactivate gramophone if needed
            if (gramophoneToFind == null)
            {
                FindGramophone();
            }

            if (gramophoneToFind != null)
            {
                // Try to find and stop the audio source
                AudioSource audioSource = gramophoneToFind.GetComponent<AudioSource>();
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Stop();
                    DebugLog("Stopped playing music on gramophone");
                }

                // Try to find and call a StopMusic method if it exists
                var stopMethod = gramophoneToFind.GetType().GetMethod("StopMusic");
                if (stopMethod != null)
                {
                    stopMethod.Invoke(gramophoneToFind, null);
                    DebugLog("Called StopMusic on gramophone");
                }
            }

            // Check if quest should auto-complete on music stop
            if (musicQuest.completeOnMusicStop)
            {
                musicQuest.ForceCompletionCheck();
            }
        }
        else
        {
            DebugLog("Cannot complete 'stop music' objective - quest not properly set up", true);
        }
    }

    private void FindMusicQuest()
    {
        DebugLog("Looking for music quest...");

        // Method 1: Find in Resources
        MusicQuest[] quests = Resources.FindObjectsOfTypeAll<MusicQuest>();
        if (quests.Length > 0)
        {
            musicQuest = quests[0];
            DebugLog($"Found music quest in Resources: {musicQuest.questName}");
            return;
        }

        // Method 2: Check active quests
        if (QuestManager.Instance != null)
        {
            foreach (Quest quest in QuestManager.Instance.GetActiveQuests())
            {
                if (quest is MusicQuest)
                {
                    musicQuest = quest as MusicQuest;
                    DebugLog($"Found active music quest: {musicQuest.questName}");
                    return;
                }
            }
        }

        // Method 3: Check all quests with music-related names
        if (QuestManager.Instance != null)
        {
            foreach (Quest quest in Resources.FindObjectsOfTypeAll<Quest>())
            {
                if (quest.questName.Contains("Music") ||
                    quest.questName.Contains("Gramophone") ||
                    quest.questName.Contains("Play"))
                {
                    MusicQuest mq = quest as MusicQuest;
                    if (mq != null)
                    {
                        musicQuest = mq;
                        DebugLog($"Found music quest by name: {musicQuest.questName}");
                        return;
                    }
                }
            }
        }

        DebugLog("No music quest found", true);
    }

    private void FindGramophone()
    {
        // Look for gameobjects with "gramophone" in their name
        GameObject[] potentialGramophones = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in potentialGramophones)
        {
            if (obj.name.ToLower().Contains("gramophone") ||
                obj.name.ToLower().Contains("music") ||
                obj.tag == "Gramophone")
            {
                gramophoneToFind = obj;
                DebugLog($"Found gramophone: {gramophoneToFind.name}");
                return;
            }
        }

        // Look for audio sources
        AudioSource[] audioSources = GameObject.FindObjectsOfType<AudioSource>();
        if (audioSources.Length > 0)
        {
            gramophoneToFind = audioSources[0].gameObject;
            DebugLog($"Found potential gramophone via AudioSource: {gramophoneToFind.name}");
            return;
        }

        DebugLog("No gramophone found in scene", true);
    }

    private void DebugLog(string message, bool isWarning = false)
    {
        if (debugMode)
        {
            if (isWarning)
                Debug.LogWarning($"[MusicQuestActivator] {message}");
            else
                Debug.Log($"[MusicQuestActivator] {message}");
        }
    }
}