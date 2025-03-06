using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ContentSizeFitter))]
public class AutoSize : MonoBehaviour
{
    private ContentSizeFitter fitter;
    private RectTransform rect;
    private Text text;

    void Awake()
    {
        fitter = GetComponent<ContentSizeFitter>();
        rect = GetComponent<RectTransform>();
        text = GetComponentInChildren<Text>();

        if (fitter == null)
            fitter = gameObject.AddComponent<ContentSizeFitter>();

        // Configure the ContentSizeFitter
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    void Update()
    {
        // Only update layout when text changes
        if (text != null && text.cachedTextGenerator.characterCountVisible > 0)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
    }
}