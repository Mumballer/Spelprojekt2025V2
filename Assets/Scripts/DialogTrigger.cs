using UnityEngine;

public class DialogTrigger : MonoBehaviour
{
    [SerializeField] private Dialog dialog;
    [SerializeField] private bool autoTrigger = false;
    [SerializeField] private float triggerDistance = 3f;
    [SerializeField] private GameObject interactionPrompt;

    private Transform playerTransform;
    private bool hasTriggered = false;

    private void Start()
    {
        PlayerController player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerTransform == null || DialogManager.Instance == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= triggerDistance)
        {
            if (interactionPrompt != null && !DialogManager.Instance.IsDialogActive)
            {
                interactionPrompt.SetActive(true);
            }

            if (autoTrigger && !hasTriggered && DialogManager.Instance.CanStartDialog())
            {
                TriggerDialog();
                hasTriggered = true;
            }
        }
        else
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            if (distance > triggerDistance * 1.5f)
            {
                hasTriggered = false;
            }
        }
    }

    public void TriggerDialog()
    {
        if (dialog != null && DialogManager.Instance != null && DialogManager.Instance.CanStartDialog())
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog));

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}