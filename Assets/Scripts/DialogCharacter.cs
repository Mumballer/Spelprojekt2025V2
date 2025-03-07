using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogCharacter", menuName = "Dialog/Character")]
public class DialogCharacter : ScriptableObject
{
    // karaktärens namn
    public string characterName;
    // karaktärens bild
    public Sprite portraitSprite;

    [Header("Portrait Settings")]
    // storlek på porträttbilden
    public float portraitSize = 100f;
    // position för porträtt
    public Vector2 portraitOffset = Vector2.zero;
}