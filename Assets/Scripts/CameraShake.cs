using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float shakeAmount; // Shake intensity
    private Transform playerCamera; // Reference to the player's camera
    private Vector3 initialLocalPos; // Initial local position of the camera

    void Start()
    {
        playerCamera = transform; // Assuming this script is on the camera
        initialLocalPos = playerCamera.localPosition; // Store initial local position
    }

    void Update()
    {
        // Add shake as an offset instead of overriding position
        playerCamera.localPosition = initialLocalPos + (Random.insideUnitSphere * shakeAmount);
    }
}
