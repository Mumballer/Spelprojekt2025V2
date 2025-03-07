using UnityEngine;

public class FinalScipt : MonoBehaviour
{

    public Animation anim;
    public AudioSource FinalBeep;
    public AudioSource RegularBeep;
    public AudioSource Song;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = gameObject.GetComponent<Animation>();
        anim.Play();
    }

    public void Beep()
    {
        
        Song.Stop();
    }
    public void TrueBeep()
    {
        FinalBeep.Play();
        RegularBeep.Stop();
    }
    public void Quit()
    {
        Debug.Log("Game is quitting..."); // Useful for testing in the editor
        Application.Quit();

        // If running in the editor, stop play mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
