using UnityEngine;

public class NameTagPlacementTrigger : MonoBehaviour
{
    private NameTagPlacingManager manager;

    private void Start()
    {
        manager = NameTagPlacingManager.Instance;
        Debug.Log("NameTagPlacementTrigger initialized.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            manager.currentPlayerPosition = other.transform.position;
            Debug.Log($"Player entered placement trigger. Current position: {manager.currentPlayerPosition}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            manager.currentPlayerPosition = Vector3.zero;
            Debug.Log("Player exited placement trigger. Resetting current position.");
        }
    }
}