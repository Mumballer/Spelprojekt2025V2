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




}