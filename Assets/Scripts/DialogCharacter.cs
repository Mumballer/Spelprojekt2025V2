using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogCharacter", menuName = "Dialog/Character")]
public class DialogCharacter : ScriptableObject
{
    public string characterName;
    public Sprite portraitSprite;
}