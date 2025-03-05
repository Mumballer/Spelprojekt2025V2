using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class NameTagQuestUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private TextMeshProUGUI currentNameTagText;
    [SerializeField] private TextMeshProUGUI placedNamesText; // NEW: List of placed names
    [SerializeField] private Image progressBar;
    [SerializeField] private GameObject hintPanel;

    [Header("Settings")]
    [SerializeField] private float hintDisplayTime = 3f;
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color incorrectColor = Color.red;
    [SerializeField] private Color neutralColor = Color.white;
    [SerializeField] private Color placedNameColor = Color.green; // NEW: Color for placed names

    private float hintTimer = 0f;
    private Dictionary<string, bool> placedNames = new Dictionary<string, bool>(); // NEW: Track placed nametags

    private void Start()
    {
        if (NameTagManager.Instance != null)
        {
            // Subscribe to events from the NameTagManager
            NameTagManager.Instance.OnProgressUpdated += UpdateProgress;
            NameTagManager.Instance.OnNameTagPlaced += OnNameTagPlaced;
            NameTagManager.Instance.OnNameTagPickup += OnNameTagPickup;
        }
        else
        {
            Debug.LogWarning("NameTagManager instance not found. UI won't update.");
        }

        // Initialize UI
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }

        if (currentNameTagText != null)
        {
            currentNameTagText.text = "";
        }

        // Initialize progress display
        UpdateProgress(0, 0);
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (NameTagManager.Instance != null)
        {
            NameTagManager.Instance.OnProgressUpdated -= UpdateProgress;
            NameTagManager.Instance.OnNameTagPlaced -= OnNameTagPlaced;
            NameTagManager.Instance.OnNameTagPickup -= OnNameTagPickup;
        }
    }

    private void Update()
    {
        // Handle hint timer
        if (hintTimer > 0)
        {
            hintTimer -= Time.deltaTime;
            if (hintTimer <= 0 && hintPanel != null)
            {
                hintPanel.SetActive(false);
            }
        }
    }

    private void UpdateProgress(int current, int total)
    {
        if (progressText != null)
        {
            if (total > 0)
            {
                progressText.text = $"Nametags Placed: {current}/{total}";
            }
            else
            {
                progressText.text = "Find and place all nametags";
            }
        }

        if (progressBar != null && total > 0)
        {
            progressBar.fillAmount = (float)current / total;
        }
    }

    private void OnNameTagPickup(NameTag nameTag)
    {
        if (nameTag == null) return;

        if (currentNameTagText != null)
        {
            currentNameTagText.text = $"Holding: {nameTag.GuestName}'s nametag";
        }

        ShowHint($"Find {nameTag.GuestName}'s place at the table.", neutralColor);
    }

    private void OnNameTagPlaced(ChairNameTagSpot chairSpot, NameTag nameTag, bool isCorrect)
    {
        if (nameTag == null) return;

        if (currentNameTagText != null)
        {
            currentNameTagText.text = "";
        }

        // NEW: Track correctly placed names
        if (isCorrect)
        {
            // Add to dictionary of placed names
            placedNames[nameTag.GuestName] = true;
            UpdatePlacedNamesList();

            ShowHint($"Correct! {nameTag.GuestName}'s nametag placed.", correctColor);
        }
        else
        {
            ShowHint($"That doesn't seem right. Try another spot for {nameTag.GuestName}.", incorrectColor);
        }
    }

    // NEW: Method to update the placed names list display
    private void UpdatePlacedNamesList()
    {
        if (placedNamesText == null) return;

        // Start with rich text format
        string displayText = "<b>Correctly Placed:</b>\n";

        // No names placed yet
        if (placedNames.Count == 0)
        {
            displayText += "None yet";
        }
        else
        {
            // Add each name in green
            foreach (string name in placedNames.Keys)
            {
                string colorHex = ColorUtility.ToHtmlStringRGB(placedNameColor);
                displayText += $"<color=#{colorHex}>{name}</color>\n";
            }
        }

        // Update the UI
        placedNamesText.text = displayText;
    }

    private void ShowHint(string message, Color color)
    {
        if (hintText != null)
        {
            hintText.text = message;
            hintText.color = color;

            if (hintPanel != null)
            {
                hintPanel.SetActive(true);
            }

            hintTimer = hintDisplayTime;
        }
    }
}