using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogPortraitSystem : MonoBehaviour
{
    [Header("UI References")]
    // container för porträttet
    [SerializeField] private GameObject portraitContainer;
    // bildkomponent för porträttet
    [SerializeField] private Image portraitImage;
    // text för karaktärsnamn
    [SerializeField] private TextMeshProUGUI characterNameText;
    // ram för porträtt
    [SerializeField] private RectTransform portraitFrame;

    [Header("Default Portrait Settings")]
    // standardstorlek för porträtt
    [SerializeField] private float defaultPortraitSize = 100f;
    // standardposition för porträtt
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
            // visar porträttet
            portraitContainer.SetActive(true);
            portraitImage.sprite = character.portraitSprite;
            characterNameText.text = character.characterName;

            if (portraitFrame != null)
            {
                // använd karaktärens inställningar
                float size = character.portraitSize > 0 ? character.portraitSize : defaultPortraitSize;
                Vector2 offset = character.portraitOffset != Vector2.zero ? character.portraitOffset : defaultPortraitOffset;

                // ställ in bildstorlek
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