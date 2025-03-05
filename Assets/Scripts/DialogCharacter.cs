using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogCharacter", menuName = "Dialog/Character")]
public class DialogCharacter : ScriptableObject
{
    public string characterName;
    public Sprite portraitSprite;

    [Header("Portrait Settings")]
    [Tooltip("Size of the portrait in UI units")]
    public float portraitSize = 100f;
    [Tooltip("Offset position of the portrait within the container")]
    public Vector2 portraitOffset = Vector2.zero;
}