using UnityEngine;

public class FinalScipt : MonoBehaviour
{

    public Animation anim;
    public AudioSource AudioSource;
    public AudioSource Beep;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = gameObject.GetComponent<Animation>();
        anim.Play();
    }

    public void PlayFinalBeep()
    {
        AudioSource.Play();
        Beep.Stop();
    }

    public void End()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
