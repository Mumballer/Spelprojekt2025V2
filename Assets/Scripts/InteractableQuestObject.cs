using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractableQuestObject : MonoBehaviour
{
    [Header("Quest Settings")]
    [SerializeField] private Quest relatedQuest;
    [SerializeField] private int objectiveIndex;
    [SerializeField] private bool destroyOnInteract = true;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 5f; // Increased default distance
    [SerializeField] public string interactionPrompt = "Press E to interact";
    [SerializeField] private bool debugMode = true; // Enable for troubleshooting

    [Header("Optional Settings")]
    [SerializeField] private GameObject visualEffect;
    [SerializeField] private AudioClip interactSound;
    [SerializeField] private float promptHeight = 2f; // Adjust this to position prompt
    [SerializeField] private float promptScale = 0.01f; // Adjust scale if needed

    private bool isPlayerNearby = false;
    private GameObject promptObject;
    private TextMeshProUGUI promptText;
    private AudioSource audioSource;
    private bool hasBeenInteracted = false;
    private bool promptCreated = false;

    private void Start()
    {
        // Set up components
        if (interactSound != null && audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Create the prompt
        CreateInteractionPrompt();

        if (debugMode)
        {
            Debug.Log($"[{gameObject.name}] InteractableQuestObject initialized. Prompt created: {promptCreated}");
            Debug.Log($"[{gameObject.name}] Interaction distance: {interactionDistance}");
            if (relatedQuest != null)
                Debug.Log($"[{gameObject.name}] Related quest: {relatedQuest.questName}, Objective: {objectiveIndex}");
            else
                Debug.LogError($"[{gameObject.name}] No quest assigned to InteractableQuestObject!");
        }
    }

    private void Update()
    {
        // Find the player using modern Unity API
        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player == null)
        {
            if (debugMode) Debug.LogWarning($"[{gameObject.name}] Cannot find PlayerController in scene!");
            return;
        }

        // Check distance to player
        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool playerInRange = distance <= interactionDistance;

        if (debugMode && Input.GetKeyDown(KeyCode.P)) // Debug key to check distance
        {
            Debug.Log($"[{gameObject.name}] Distance to player: {distance}, Interaction range: {interactionDistance}");
            Debug.Log($"[{gameObject.name}] Player in range: {playerInRange}");
        }

        // Show/hide prompt based on distance
        if (playerInRange != isPlayerNearby)
        {
            isPlayerNearby = playerInRange;
            if (promptObject != null)
            {
                promptObject.SetActive(isPlayerNearby && !hasBeenInteracted);

                if (debugMode)
                {
                    Debug.Log($"[{gameObject.name}] Setting prompt active: {isPlayerNearby && !hasBeenInteracted}");
                }
            }
            else if (debugMode && isPlayerNearby)
            {
                Debug.LogError($"[{gameObject.name}] Prompt object is null but player is in range!");
            }
        }

        // Process interaction
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E) && !hasBeenInteracted)
        {
            if (debugMode) Debug.Log($"[{gameObject.name}] Player pressed E to interact");
            InteractWithObject();
        }
    }

    private void CreateInteractionPrompt()
    {
        try
        {
            // Create a new prompt object
            promptObject = new GameObject($"{gameObject.name}_InteractionPrompt");
            promptObject.transform.SetParent(transform);
            promptObject.transform.localPosition = new Vector3(0, promptHeight, 0);

            // Add canvas
            Canvas canvas = promptObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            // Add canvas scaler
            CanvasScaler scaler = promptObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100;

            // Configure canvas transform
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(300, 100);
            canvasRect.localScale = new Vector3(promptScale, promptScale, promptScale);

            // Create background panel
            GameObject panel = new GameObject("PromptPanel");
            panel.transform.SetParent(canvas.transform);

            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.localPosition = Vector3.zero;
            panelRect.sizeDelta = Vector2.zero;

            // Create text
            GameObject textObj = new GameObject("PromptText");
            textObj.transform.SetParent(panel.transform);

            promptText = textObj.AddComponent<TextMeshProUGUI>();
            promptText.text = interactionPrompt;
            promptText.fontSize = 36;
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.color = Color.white;

            RectTransform textRect = promptText.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.localPosition = Vector3.zero;
            textRect.sizeDelta = new Vector2(-20, -20);

            // Add billboard script (or just use LookAt camera in Update)
            promptObject.AddComponent<LookAtCamera>();

            // Initially hidden until player gets close
            promptObject.SetActive(false);

            promptCreated = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[{gameObject.name}] Failed to create interaction prompt: {e.Message}");
            promptCreated = false;
        }
    }

    private void InteractWithObject()
    {
        // Prevent multiple interactions
        hasBeenInteracted = true;

        // Update quest objective
        if (relatedQuest != null && QuestManager.Instance != null)
        {
            // Check if quest is active
            if (QuestManager.Instance.IsQuestActive(relatedQuest))
            {
                // Complete the objective
                QuestManager.Instance.CompleteObjective(relatedQuest, objectiveIndex);

                // Play effects
                if (visualEffect != null)
                {
                    Instantiate(visualEffect, transform.position, Quaternion.identity);
                }

                if (interactSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(interactSound);
                }

                // Hide prompt
                if (promptObject != null)
                {
                    promptObject.SetActive(false);
                }

                // Destroy or deactivate after a short delay
                if (destroyOnInteract)
                {
                    // If we have a sound, wait for it to finish
                    float delay = (interactSound != null) ? interactSound.length : 0.1f;
                    Destroy(gameObject, delay);
                }
                else
                {
                    // Just make it non-interactive
                    Collider[] colliders = GetComponents<Collider>();
                    foreach (var col in colliders)
                    {
                        col.enabled = false;
                    }

                    // Maybe make it semi-transparent
                    Renderer[] renderers = GetComponentsInChildren<Renderer>();
                    foreach (var rend in renderers)
                    {
                        Color color = rend.material.color;
                        color.a = 0.5f;
                        rend.material.color = color;
                    }
                }
            }
            else if (debugMode)
            {
                Debug.LogWarning($"[{gameObject.name}] Quest {relatedQuest.questName} is not active!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw interaction sphere in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        // Draw where the prompt will appear
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * promptHeight);
        Gizmos.DrawSphere(transform.position + Vector3.up * promptHeight, 0.2f);
    }
}

// Simple component to make UI face camera
public class LookAtCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward);
        }
    }
}