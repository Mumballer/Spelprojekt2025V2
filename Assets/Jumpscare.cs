using UnityEngine;
using UnityEngine.UI;

public class Jumpscare : MonoBehaviour
{

    public Image JumpscareImage;
    public AudioSource Scare;
    bool isJumpscaring = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject go = GameObject.Find("Jumpscare"); // Make sure this is the correct name in your hierarchy!
        if (go != null)
        {
            JumpscareImage = go.GetComponent<Image>();
            JumpscareImage.enabled = false;
        }
        else
        {
            Debug.LogError("JumpscareImage not found! Check your UI hierarchy.");
        }
    }


    public void StartJumpscare()
    {
        isJumpscaring = true;
        JumpscareImage.enabled = true;
        Scare.Play();

        //Chasemusic.Stop();
        //BackgroundMusic.Stop();

        // Reload the scene after a short delay
        Invoke("ReloadScene", 1f); // 2 second delay for effect (adjust as needed)
        //Scare.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            StartJumpscare();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
