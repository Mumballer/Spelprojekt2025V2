using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{

    public void PlayGame()
    {
        SceneManager.LoadScene(0);
    }

    public void Options()
    {
        SceneManager.LoadScene("Options");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Back()
    {
        SceneManager.LoadScene("Menu");
    }


}
