using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

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

        // Starta coroutines för att byta scen efter 2 sekunder
        StartCoroutine(SwitchSceneAfterDelay(2f));
    }

    private IEnumerator SwitchSceneAfterDelay(float delay)
    {
        // Vänta i 'delay' sekunder
        yield return new WaitForSeconds(delay);

        // Byt scen här, exempelvis till en scen som heter "JumpscareScene"
        SceneManager.LoadScene("Stage 5");
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
