using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [SerializeField] private CanvasGroup fadeCanvasGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void FadeToScene(string sceneName, float duration)
    {
        StartCoroutine(FadeOutAndLoadScene(sceneName, duration));
    }

    private System.Collections.IEnumerator FadeOutAndLoadScene(string sceneName, float duration)
    {
        // Make fade panel visible
        fadeCanvasGroup.gameObject.SetActive(true);

        // Fade to black
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            fadeCanvasGroup.alpha = t;
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;

        // Load new scene
        SceneManager.LoadScene(sceneName);

        // Fade back in
        startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            fadeCanvasGroup.alpha = 1f - t;
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;

        // Hide fade panel
        fadeCanvasGroup.gameObject.SetActive(false);
    }
}