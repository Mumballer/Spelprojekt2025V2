using UnityEngine;
using System.Collections;

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

    private AudioSource audioSource;
    private bool isPlaying = false;
    private Coroutine fadeCoroutine;
    private bool playerInRange = false;
    private bool objectiveCompleted = false;

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
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            UpdatePrompt();
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
        // Handle player interaction
        if (requireButtonPress && playerInRange && Input.GetKeyDown(interactKey))
        {
            // Check if interaction is allowed based on quest state
            if (CanInteract())
            {
                ToggleMusic();
                UpdatePrompt();
            }
            else
            {
                Debug.Log("Gramophone is locked or quest objective already completed");
            }
        }
    }
    
    private bool CanInteract()
    {
        // If no quest restrictions, always allow interaction
        if (associatedQuest == null)
            return true;
            
        // If we don't require an active quest, or the quest is active, allow interaction
        if (!requireActiveQuest || associatedQuest.IsActive)
        {
            // If the objective is already completed and we're not allowing replay
            if (objectiveCompleted && !completeObjectiveOnStop)
                return true; // Allow toggling off but not on again
                
            return true;
        }
        
        return false;
    }
    
    private void UpdatePrompt()
    {
        if (interactionPrompt != null && playerInRange)
        {
            interactionPrompt.SetActive(true);
            
            TMPro.TextMeshProUGUI promptTextComponent = 
                interactionPrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                
            if (promptTextComponent != null)
            {
                // Show different prompt based on quest and play status
                if (!CanInteract())
                {
                    promptTextComponent.text = lockedPromptText;
                }
                else
                {
                    promptTextComponent.text = isPlaying ? stopPromptText : playPromptText;
                }
            }
        }
    }

    // Public method for toggling music state
    public void ToggleMusic()
    {
        isPlaying = !isPlaying;

        if (isPlaying)
        {
            StartMusic();
            Debug.Log($"Gramophone started playing: {name}");
            
            // Complete quest objective when started playing
            if (completeObjectiveOnPlay && !objectiveCompleted)
            {
                CompleteQuestObjective();
            }
        }
        else
        {
            StopMusic();
            Debug.Log($"Gramophone stopped playing: {name}");
            
            // Complete quest objective when stopped playing
            if (completeObjectiveOnStop && !objectiveCompleted)
            {
                CompleteQuestObjective();
            }
        }
    }
    
    private void CompleteQuestObjective()
    {
        if (associatedQuest != null && QuestManager.Instance != null)
        {
            // Make sure the quest is active
            if (!associatedQuest.IsActive)
            {
                Debug.Log("Cannot complete objective - quest is not active");
                return;
            }
            
            // Make sure the objective exists
            if (objectiveIndex >= 0 && objectiveIndex < associatedQuest.objectives.Count)
            {
                QuestObjective objective = associatedQuest.objectives[objectiveIndex];
                
                if (!objective.isCompleted)
                {
                    Debug.Log($"Completing objective {objectiveIndex} for quest {associatedQuest.questName}");
                    QuestManager.Instance.CompleteObjective(associatedQuest, objectiveIndex);
                    objectiveCompleted = true;
                }
            }
        }
    }

    private void StartMusic()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeAudio(0, volumeLevel, fadeTime));
    }

    private void StopMusic()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

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
}