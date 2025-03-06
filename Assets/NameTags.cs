using UnityEngine;

public class NameTags : MonoBehaviour
{
    public string name; // Name of the person

    private NameTagPlacingManager gameManager;

    private void Start()
    {
        gameManager = NameTagPlacingManager.Instance;
    }

    private void OnMouseDown()
    {
        gameManager.PickUpNametag(this);
    }
}
