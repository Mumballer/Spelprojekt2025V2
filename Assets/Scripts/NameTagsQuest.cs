using UnityEngine;

public class NameTagsQuest : Quest
{
    [SerializeField] private GameObject nameTagSource; // The visible one on kitchen sink
    [SerializeField] private GameObject[] nameTagPlaceholders; // Array of invisible ones on table
    [SerializeField] private int requiredNameTags = 4; // How many are needed
    [SerializeField] private BoxCollider tableArea; // Area where player needs to be to place tags

    private int placedNameTags = 0;
    private bool isHoldingNameTag = false;

    public override void OnActivate()
    {
        base.OnActivate();
        // Make sure source is visible
        SpriteRenderer sourceRenderer = nameTagSource.GetComponent<SpriteRenderer>();
        Color sourceColor = sourceRenderer.color;
        sourceColor.a = 1f;
        sourceRenderer.color = sourceColor;

        // Make placeholders invisible
        foreach (GameObject placeholder in nameTagPlaceholders)
        {
            SpriteRenderer placeholderRenderer = placeholder.GetComponent<SpriteRenderer>();
            Color placeholderColor = placeholderRenderer.color;
            placeholderColor.a = 0f;
            placeholderRenderer.color = placeholderColor;
        }

        // Reset counter
        placedNameTags = 0;
        isHoldingNameTag = false;

        // Update UI
        UIManager.Instance.UpdateNameTagCounter(placedNameTags, requiredNameTags);
    }

    private void Start()
    {
        nameTagSource.GetComponent<Interactable>().onInteract.AddListener(PickUpNameTag);

        foreach (GameObject placeholder in nameTagPlaceholders)
        {
            placeholder.GetComponent<Interactable>().onInteract.AddListener(() => PlaceNameTag(placeholder));
        }

        // Make sure table area collider is a trigger
        if (tableArea != null)
        {
            tableArea.isTrigger = true;
        }
    }

    private void PickUpNameTag()
    {
        // Can only pick up if quest is active and not already holding
        if (!QuestManager.Instance.IsQuestActive(id) || isHoldingNameTag) return;

        isHoldingNameTag = true;

        // Optional: change alpha of the source to indicate it's been picked up
        SpriteRenderer sourceRenderer = nameTagSource.GetComponent<SpriteRenderer>();
        Color sourceColor = sourceRenderer.color;
        sourceColor.a = 0.5f; // Semi-transparent to indicate picked up
        sourceRenderer.color = sourceColor;

        // Update UI to show player is holding a name tag
        UIManager.Instance.UpdateQuestText("You picked up a name tag. Place it on the table.");
    }

    private void PlaceNameTag(GameObject placeholder)
    {
        // Check if quest is active, player is holding a tag, and in the table area
        if (!QuestManager.Instance.IsQuestActive(id) || !isHoldingNameTag) return;

        // Check if this placeholder is not already filled
        SpriteRenderer renderer = placeholder.GetComponent<SpriteRenderer>();
        if (renderer.color.a > 0.1f) return; // Already placed

        // Check if player is in the table area
        if (tableArea != null)
        {
            Collider playerCollider = GameObject.FindGameObjectWithTag("Player").GetComponent<Collider>();
            if (!tableArea.bounds.Intersects(playerCollider.bounds))
            {
                UIManager.Instance.UpdateQuestText("You need to be closer to the table.");
                return;
            }
        }

        // Make the placeholder visible
        Color color = renderer.color;
        color.a = 1f;
        renderer.color = color;

        // Reset holding state
        isHoldingNameTag = false;

        // Reset source to be visible again for the next tag
        SpriteRenderer sourceRenderer = nameTagSource.GetComponent<SpriteRenderer>();
        Color sourceColor = sourceRenderer.color;
        sourceColor.a = 1f;
        sourceRenderer.color = sourceColor;

        // Increment counter
        placedNameTags++;

        // Update UI to show progress
        UIManager.Instance.UpdateNameTagCounter(placedNameTags, requiredNameTags);
        UIManager.Instance.UpdateQuestText($"Name tag placed! {placedNameTags}/{requiredNameTags}");

        // Check if all name tags are placed
        if (placedNameTags >= requiredNameTags)
        {
            UIManager.Instance.UpdateQuestText("All name tags have been placed!");
            QuestManager.Instance.CompleteQuest(id);
        }
    }
}