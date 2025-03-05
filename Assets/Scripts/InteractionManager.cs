using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    private List<DialogTrigger> activeTriggers = new List<DialogTrigger>();
    private DialogTrigger currentClosestTrigger;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.OnShowDialog += HideAllPrompts;
            DialogManager.Instance.OnHideDialog += UpdatePrompts;
        }
    }

    private void OnDestroy()
    {
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.OnShowDialog -= HideAllPrompts;
            DialogManager.Instance.OnHideDialog -= UpdatePrompts;
        }
    }

    public void RegisterTrigger(DialogTrigger trigger)
    {
        if (!activeTriggers.Contains(trigger))
        {
            activeTriggers.Add(trigger);
        }
    }

    public void UnregisterTrigger(DialogTrigger trigger)
    {
        activeTriggers.Remove(trigger);
    }

    public void HideAllPrompts()
    {
        foreach (var trigger in activeTriggers)
        {
            if (trigger != null && trigger.interactionPrompt != null)
            {
                trigger.interactionPrompt.SetActive(false);
            }
        }
    }

    public void UpdatePrompts()
    {
        if (DialogManager.Instance != null && DialogManager.Instance.IsDialogActive)
        {
            return;
        }

        // Find closest trigger and show only that prompt
        FindClosestTrigger();
    }

    private void FindClosestTrigger()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        float closestDistance = float.MaxValue;
        DialogTrigger closestTrigger = null;

        foreach (var trigger in activeTriggers)
        {
            if (trigger == null) continue;

            float distance = Vector3.Distance(player.transform.position, trigger.transform.position);
            if (distance < closestDistance && distance <= trigger.triggerDistance)
            {
                closestDistance = distance;
                closestTrigger = trigger;
            }
        }

        // Update prompts
        foreach (var trigger in activeTriggers)
        {
            if (trigger != null && trigger.interactionPrompt != null)
            {
                trigger.interactionPrompt.SetActive(trigger == closestTrigger);
            }
        }

        currentClosestTrigger = closestTrigger;
    }

    public DialogTrigger GetClosestTrigger()
    {
        return currentClosestTrigger;
    }
}