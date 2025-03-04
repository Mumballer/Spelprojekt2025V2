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

    [Header("Portrait Settings")]
    [SerializeField] private float portraitSize = 100f;
    [SerializeField] private Vector2 portraitOffset = new Vector2(20f, -20f);

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
                portraitFrame.sizeDelta = new Vector2(portraitSize, portraitSize);
                portraitFrame.anchoredPosition = portraitOffset;
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