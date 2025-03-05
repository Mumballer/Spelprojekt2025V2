using UnityEngine;

public class FinalScipt : MonoBehaviour
{

    public Animation anim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = gameObject.GetComponent<Animation>();
        anim.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
