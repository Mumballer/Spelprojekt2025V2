using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NameTagQuestUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private TextMeshProUGUI currentNameTagText;
    [SerializeField] private Image progressBar;
    [SerializeField] private float hintDisplayTime = 3f;
    [SerializeField] private GameObject hintPanel;

    private float hintTimer = 0f;

    private void Start()
    {
        if (NameTagManager.Instance != null)
        {
            NameTagManager.Instance.OnProgressUpdated += UpdateProgress;
            NameTagManager.Instance.OnNameTagPlaced += OnNameTagPlaced;
            NameTagManager.Instance.OnNameTagPickup += OnNameTagPickup;
        }

        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }

        if (currentNameTagText != null)
        {
            currentNameTagText.text = "";
        }
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
            progressText.text = $"Nametags Placed: {current}/{total}";
        }

        if (progressBar != null)
        {
            progressBar.fillAmount = (float)current / total;
        }
    }

    private void OnNameTagPickup(NameTag nameTag)
    {
        if (currentNameTagText != null)
        {
            currentNameTagText.text = $"Holding: {nameTag.GuestName}'s nametag";
        }

        ShowHint($"Find {nameTag.GuestName}'s place at the table.", Color.white);
    }

    private void OnNameTagPlaced(ChairNameTagSpot chairSpot, NameTag nameTag, bool isCorrect)
    {
        if (currentNameTagText != null)
        {
            currentNameTagText.text = "";
        }

        if (isCorrect)
        {
            ShowHint($"Correct! {nameTag.GuestName}'s nametag placed.", Color.green);
        }
        else
        {
            ShowHint($"That doesn't seem right. Try another spot for {nameTag.GuestName}.", Color.red);
        }
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