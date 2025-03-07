using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogCharacter", menuName = "Dialog/Character")]
public class DialogCharacter : ScriptableObject
{
    // karakt�rens namn
    public string characterName;
    // karakt�rens bild
    public Sprite portraitSprite;

    [Header("Portrait Settings")]
    // storlek p� portr�ttbilden
    public float portraitSize = 100f;
    // position f�r portr�tt
    public Vector2 portraitOffset = Vector2.zero;
}