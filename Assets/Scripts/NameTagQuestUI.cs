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
    [SerializeField] private TextMeshProUGUI placedNamesText;
    [SerializeField] private Image progressBar;
    [SerializeField] private GameObject hintPanel;

    [Header("Settings")]
    [SerializeField] private float hintDisplayTime = 3f;
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color incorrectColor = Color.red;
    [SerializeField] private Color neutralColor = Color.white;
    [SerializeField] private Color placedNameColor = Color.green;

    private float hintTimer = 0f;
    private Dictionary<string, bool> placedNames = new Dictionary<string, bool>();

    private void Start()
    {
        if (NameTagManager.Instance != null)
        {
            NameTagManager.Instance.OnProgressUpdated += UpdateProgress;
            NameTagManager.Instance.OnNameTagPlaced += OnNameTagPlaced;
            NameTagManager.Instance.OnNameTagPickup += OnNameTagPickup;
        }
        else
        {
            Debug.LogWarning("NameTagManager instance not found. UI won't update.");
        }

        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }

        if (currentNameTagText != null)
        {
            currentNameTagText.text = "";
        }

        UpdateProgress(0, 0);
    }

    private void OnDestroy()
    {
        if (NameTagManager.Instance != null)
        {
            NameTagManager.Instance.OnProgressUpdated -= UpdateProgress;
            NameTagManager.Instance.OnNameTagPlaced -= OnNameTagPlaced;
            NameTagManager.Instance.OnNameTagPickup -= OnNameTagPickup;
        }
    }

    private void Update()
    {
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

        ShowHint($"Place {nameTag.GuestName}'s nametag on the table.", neutralColor);
    }

    private void OnNameTagPlaced(NameTag nameTag)
    {
        if (nameTag == null) return;

        if (currentNameTagText != null)
        {
            currentNameTagText.text = "";
        }

        placedNames[nameTag.GuestName] = true;
        UpdatePlacedNamesList();

        ShowHint($"{nameTag.GuestName}'s nametag placed on the table.", correctColor);
    }

    private void UpdatePlacedNamesList()
    {
        if (placedNamesText == null) return;

        string displayText = "<b>Placed Nametags:</b>\n";

        if (placedNames.Count == 0)
        {
            displayText += "None yet";
        }
        else
        {
            foreach (string name in placedNames.Keys)
            {
                string colorHex = ColorUtility.ToHtmlStringRGB(placedNameColor);
                displayText += $"<color=#{colorHex}>{name}</color>\n";
            }
        }

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