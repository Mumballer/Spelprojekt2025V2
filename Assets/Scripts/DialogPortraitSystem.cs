using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogPortraitSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject portraitContainer;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private RectTransform portraitFrame;

    [Header("Default Portrait Settings")]
    [SerializeField] private float defaultPortraitSize = 100f;
    [SerializeField] private Vector2 defaultPortraitOffset = Vector2.zero;

    private void Start()
    {
        if (portraitContainer != null)
        {
            portraitContainer.SetActive(false);
        }
    }

    public void UpdatePortrait(DialogCharacter character)
    {
        if (character == null)
        {
            HidePortrait();
            return;
        }

        if (portraitContainer != null && portraitImage != null && characterNameText != null)
        {
            portraitContainer.SetActive(true);
            portraitImage.sprite = character.portraitSprite;
            characterNameText.text = character.characterName;

            if (portraitFrame != null)
            {
                // Use character-specific settings if available, otherwise use defaults
                float size = character.portraitSize > 0 ? character.portraitSize : defaultPortraitSize;
                Vector2 offset = character.portraitOffset != Vector2.zero ? character.portraitOffset : defaultPortraitOffset;

                // Set the portrait image size
                portraitFrame.sizeDelta = new Vector2(size, size);
                portraitFrame.anchoredPosition = offset;

                // Ensure the portrait image is centered within its frame
                RectTransform imageRect = portraitImage.GetComponent<RectTransform>();
                if (imageRect != null && imageRect != portraitFrame)
                {
                    imageRect.anchorMin = new Vector2(0.5f, 0.5f);
                    imageRect.anchorMax = new Vector2(0.5f, 0.5f);
                    imageRect.pivot = new Vector2(0.5f, 0.5f);
                    imageRect.anchoredPosition = Vector2.zero;
                    imageRect.sizeDelta = new Vector2(size, size);
                }

                // Log for debugging
                Debug.Log($"Setting portrait for {character.characterName}: Size={size}, Offset={offset}");
            }
        }
    }

    private void HidePortrait()
    {
        if (portraitContainer != null)
        {
            portraitContainer.SetActive(false);
        }
    }
}