using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{

    public void PlayGame()
    {
        //lodscene
    }

    public void Options()
    {

    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Back()
    {
        int loadPrevious = PlayerPrefs.GetInt("Menu");
        Application.LoadLevel(loadPrevious);
    }


}
