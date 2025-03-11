using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private TextMeshProUGUI questText;
    [SerializeField] private TextMeshProUGUI nameTagCounter;
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float notificationDuration = 3f;

    private Coroutine notificationCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Hide notification panel at start
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    public void ShowInteractPrompt(string text)
    {
        interactionPrompt.SetActive(true);
        interactionText.text = text;
    }

    public void HideInteractPrompt()
    {
        interactionPrompt.SetActive(false);
    }

    public void UpdateQuestText(string text)
    {
        if (questText != null)
        {
            questText.gameObject.SetActive(true); // Make sure it's always visible
            questText.text = text;
        }
    }

    public void UpdateNameTagCounter(int current, int max)
    {
        if (nameTagCounter != null)
        {
            nameTagCounter.gameObject.SetActive(true);
            nameTagCounter.text = $"Name Tags: {current}/{max}";
        }
    }

    public void ShowNotification(string message)
    {
        if (notificationPanel != null && notificationText != null)
        {
            // Stop any existing notification
            if (notificationCoroutine != null)
            {
                StopCoroutine(notificationCoroutine);
            }

            // Start new notification
            notificationCoroutine = StartCoroutine(ShowNotificationForDuration(message, notificationDuration));
        }
    }

    private IEnumerator ShowNotificationForDuration(string message, float duration)
    {
        notificationPanel.SetActive(true);
        notificationText.text = message;

        yield return new WaitForSeconds(duration);

        notificationPanel.SetActive(false);
        notificationCoroutine = null;
    }
}