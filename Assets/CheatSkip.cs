using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatSkip : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    private void Update()
    {
        // Check if the L key was pressed this frame
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadScene();
        }
    }

    private void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("Scene name is empty! Please specify a scene to load in the Inspector.");
        }
    }
}
