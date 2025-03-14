using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneSwitch : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad;
    [SerializeField] private bool useNextSceneInBuild = false;
    [SerializeField] private float delayBeforeLoading = 0f;
    [SerializeField] private bool fadeOut = true;
    [SerializeField] private float fadeOutDuration = 1f;
    
    [Header("Tags")]
    [SerializeField] private string targetTag = "SceneChanger";
    
    private bool isLoading = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading) return;
        
        // kollar vid kollision
        if (other.CompareTag(targetTag))
        {
            Debug.Log($"<color=cyan>SceneSwitch: Player touched object with {targetTag} tag</color>");
            isLoading = true;
            StartCoroutine(LoadNextScene());
        }
    }
    
    private System.Collections.IEnumerator LoadNextScene()
    {
        if (delayBeforeLoading > 0)
        {
            yield return new WaitForSeconds(delayBeforeLoading);
        }
        
        if (fadeOut)
        {
            yield return StartCoroutine(FadeToBlack());
        }
        
        // laddar nästa scen
        if (useNextSceneInBuild)
        {
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            int nextIndex = (currentIndex + 1) % SceneManager.sceneCountInBuildSettings;
            Debug.Log($"<color=cyan>SceneSwitch: Loading next scene in build (index: {nextIndex})</color>");
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.Log($"<color=cyan>SceneSwitch: Loading scene: {sceneToLoad}</color>");
            SceneManager.LoadScene(sceneToLoad);
        }
    }
    
    private System.Collections.IEnumerator FadeToBlack()
    {
        GameObject fadePanel = new GameObject("FadePanel");
        Canvas canvas = fadePanel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        
        RectTransform rectTransform = fadePanel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        
        UnityEngine.UI.Image fadeImage = fadePanel.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        
        // tonar in svart
        float startTime = Time.time;
        while (Time.time < startTime + fadeOutDuration)
        {
            float t = (Time.time - startTime) / fadeOutDuration;
            fadeImage.color = new Color(0, 0, 0, t);
            yield return null;
        }
        
        fadeImage.color = new Color(0, 0, 0, 1);
        yield return new WaitForSeconds(0.2f);
    }
}
