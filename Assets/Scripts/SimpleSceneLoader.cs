using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneLoader : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            // get current scene index
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

            // load next scene
            SceneManager.LoadScene("Stage 6");
        }
    }
}