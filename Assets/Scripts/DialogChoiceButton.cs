using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class DialogChoiceButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private float textPadding = 10f;
    [SerializeField] private float minWidth = 160f;
    [SerializeField] private float minHeight = 50f;

    private RectTransform rectTransform;
    private LayoutElement layoutElement;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = gameObject.AddComponent<LayoutElement>();
        }
        layoutElement.minWidth = minWidth;
        layoutElement.minHeight = minHeight;

        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
        }

        ContentSizeFitter fitter = GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    public void SetText(string text)
    {
        if (buttonText == null) return;

        buttonText.text = text;

        RectTransform textRectTransform = buttonText.GetComponent<RectTransform>();
        if (textRectTransform != null)
        {
            textRectTransform.anchorMin = new Vector2(0, 0);
            textRectTransform.anchorMax = new Vector2(1, 1);
            textRectTransform.pivot = new Vector2(0.5f, 0.5f);
            textRectTransform.offsetMin = new Vector2(textPadding, textPadding);
            textRectTransform.offsetMax = new Vector2(-textPadding, -textPadding);
        }

        buttonText.alignment = TextAlignmentOptions.Center;

#pragma warning disable CS0618
        buttonText.enableWordWrapping = true;
#pragma warning restore CS0618

        buttonText.overflowMode = TextOverflowModes.Overflow;

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }
}