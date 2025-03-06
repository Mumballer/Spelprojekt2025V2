using UnityEngine;

public class SeatTag : MonoBehaviour
{
    public string expectedName; // Name that should go here

    private NameTagPlacingManager manager;

    private void Start()
    {
        manager = NameTagPlacingManager.Instance;
    }

    private void Update()
    {
        // Check for 'E' key press when in range
        if (Input.GetKeyDown(KeyCode.E) && IsPlayerInRange())
        {
            manager.TryPlaceNametag(this);
        }
    }

    private bool IsPlayerInRange()
    {
        // Raycast to check if the player is close enough to interact
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f))
        {
            if (hit.transform.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Show UI hint when player is near
            manager.HoverSeat(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            manager.StopHovering();
        }
    }
}