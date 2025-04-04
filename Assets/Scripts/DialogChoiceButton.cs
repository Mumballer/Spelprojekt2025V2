using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(LayoutElement))] // behöver denna för storlek
public class DialogChoiceButton : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI buttonText; // texten på knappen

    [Header("Sizing & Padding")]
    [SerializeField] private float horizontalPadding = 30f; // luft på sidorna
    [SerializeField] private float verticalPadding = 15f; // luft uppe/nere
    [SerializeField] private float minWidth = 150f; // minsta bredd
    [SerializeField] private float minHeight = 45f; // minsta höjd
    [SerializeField] private float maxWidth = 500f; // största bredd

    private LayoutElement layoutElement; // komponent för storlek
    private Button button; // själva knappen
    private RectTransform textRectTransform; // textens rektangel

    private void Awake()
    {
        // hämta komponenterna
        layoutElement = GetComponent<LayoutElement>();
        button = GetComponent<Button>();
        if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null) textRectTransform = buttonText.GetComponent<RectTransform>();

        // kolla att allt finns
        if (layoutElement == null || button == null || buttonText == null || textRectTransform == null)
        {
            Debug.LogError($"knapp {gameObject.name} saknar delar!", gameObject);
            enabled = false; // stäng av scriptet
            return;
        }

        // grundinställningar för layout
        layoutElement.minWidth = minWidth;
        layoutElement.minHeight = minHeight;
        layoutElement.flexibleWidth = 0; // ska inte sträckas ut
        layoutElement.flexibleHeight = 0;

        // grundinställningar för text
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.enableWordWrapping = true; // slå på radbrytning
        buttonText.overflowMode = TextOverflowModes.Overflow; // låt text växa nedåt
    }

    // ställ in knappen med text och klick-händelse
    public void Setup(string text, System.Action onClickAction)
    {
        if (!enabled) return; // kör inte om trasig

        // sätt texten
        buttonText.text = text;

        // räkna ut hur stor texten vill vara
        float maxTextWidth = maxWidth - horizontalPadding;
        Vector2 preferredTextSize = buttonText.GetPreferredValues(text, maxTextWidth, Mathf.Infinity);

        // räkna ut knappens storlek
        float desiredWidth = preferredTextSize.x + horizontalPadding;
        float desiredHeight = preferredTextSize.y + verticalPadding;

        // begränsa storleken
        float finalWidth = Mathf.Clamp(desiredWidth, minWidth, maxWidth);
        float finalHeight = Mathf.Max(desiredHeight, minHeight);

        // säg åt layouten hur stor knappen ska vara
        layoutElement.preferredWidth = finalWidth;
        layoutElement.preferredHeight = finalHeight;
        layoutElement.minWidth = minWidth; // upprepa min-värden
        layoutElement.minHeight = minHeight;

        // justera textens område inuti knappen
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.offsetMin = new Vector2(horizontalPadding / 2f, verticalPadding / 2f);
        textRectTransform.offsetMax = new Vector2(-horizontalPadding / 2f, -verticalPadding / 2f);

        // sätt vad som händer vid klick
        button.onClick.RemoveAllListeners(); // ta bort gamla först
        button.onClick.AddListener(() => onClickAction?.Invoke()); // lägg till nya
    }
}