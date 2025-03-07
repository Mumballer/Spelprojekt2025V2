using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
public class Jumpscare : MonoBehaviour
{
    public RawImage JumpscareImage; // Changed to RawImage if that's what you're using
    public AudioSource Scare;
    bool isJumpscaring = false;

    void Start()
    {
        // Make sure the image is disabled at start
        if (JumpscareImage != null)
        {
            JumpscareImage.enabled = false;
        }
        else
        {
            Debug.LogError("JumpscareImage not assigned in inspector!");
        }
    }

    public void StartJumpscare()
    {
        isJumpscaring = true;

        // Check if JumpscareImage is assigned
        if (JumpscareImage != null)
        {
            JumpscareImage.gameObject.SetActive(true);
            JumpscareImage.enabled = true;
        }
        else
        {
            Debug.LogError("JumpscareImage is null in StartJumpscare!");
        }

        if (Scare != null)
        {
            Scare.Play();
        }

        // Reload the scene after a short delay
        Invoke("ReloadScene", 1f); // 1 second delay
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            StartJumpscare();
            StartCoroutine(WaitAndChangeScene());

        }

    }
    private IEnumerator WaitAndChangeScene()
    {
        // Vänta i 2 sekunder
        yield return new WaitForSeconds(2);

        // Byt scen, exempelvis till scenen med index 1
        SceneManager.LoadScene("Stage 5");  // Byt till den scen du vill
    }

    // Make sure to add this method since you're invoking it
    /*private void ReloadScene()
    {
        // Add code to reload your scene here
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }*/
}