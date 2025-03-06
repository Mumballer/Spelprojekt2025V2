using UnityEngine;

public class CameraStick : MonoBehaviour
{
    public Transform Cameraobject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Cameraobject.position; 
    }
}
