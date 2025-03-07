using UnityEngine;
using UnityEngine.SceneManagement;

public class BedInteraction : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);
        LoadNextScene();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered by: " + other.gameObject.name);
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        Debug.Log("Loading next scene...");
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.LogError("No next scene in build settings!");
        }
    }
}