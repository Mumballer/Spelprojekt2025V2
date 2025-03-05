using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool lookAtCamera = true; 
    [SerializeField] private bool freezeYAxis = true; 
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Vector3 offset = Vector3.zero; 

    private Transform playerTransform;
    private Transform cameraTransform;

    private void Start()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }

        cameraTransform = Camera.main?.transform;

        if (playerTransform == null && cameraTransform == null)
        {
            Debug.LogWarning("LookAtPlayer: Could not find player or camera", this);
            enabled = false; 
        }
    }

    private void LateUpdate()
    {
        Transform target = lookAtCamera ? cameraTransform : playerTransform;
        if (target == null) return;

        Vector3 direction = target.position - transform.position;


        if (freezeYAxis)
        {
            direction.y = 0;
        }
        if (direction.sqrMagnitude < 0.001f) return;


        Quaternion lookRotation = Quaternion.LookRotation(direction);

        if (offset != Vector3.zero)
        {
            lookRotation *= Quaternion.Euler(offset);
        }
        if (rotationSpeed >= 10f)
        {
            transform.rotation = lookRotation;
        }
        else
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }
}