using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogPortraitSystem : MonoBehaviour
{
    [Header("UI References")]
    // container f�r portr�ttet
    [SerializeField] private GameObject portraitContainer;
    // bildkomponent f�r portr�ttet
    [SerializeField] private Image portraitImage;
    // text f�r karakt�rsnamn
    [SerializeField] private TextMeshProUGUI characterNameText;
    // ram f�r portr�tt
    [SerializeField] private RectTransform portraitFrame;

    [Header("Default Portrait Settings")]
    // standardstorlek f�r portr�tt
    [SerializeField] private float defaultPortraitSize = 100f;
    // standardposition f�r portr�tt
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
            // visar portr�ttet
            portraitContainer.SetActive(true);
            portraitImage.sprite = character.portraitSprite;
            characterNameText.text = character.characterName;

            if (portraitFrame != null)
            {
                // anv�nd karakt�rens inst�llningar
                float size = character.portraitSize > 0 ? character.portraitSize : defaultPortraitSize;
                Vector2 offset = character.portraitOffset != Vector2.zero ? character.portraitOffset : defaultPortraitOffset;

                // st�ll in bildstorlek
                portraitFrame.sizeDelta = new Vector2(size, size);
                portraitFrame.anchoredPosition = offset;

                // centrera bilden i ramen
                RectTransform imageRect = portraitImage.GetComponent<RectTransform>();
                if (imageRect != null && imageRect != portraitFrame)
                {
                    imageRect.anchorMin = new Vector2(0.5f, 0.5f);
                    imageRect.anchorMax = new Vector2(0.5f, 0.5f);
                    imageRect.pivot = new Vector2(0.5f, 0.5f);
                    imageRect.anchoredPosition = Vector2.zero;
                    imageRect.sizeDelta = new Vector2(size, size);
                }

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