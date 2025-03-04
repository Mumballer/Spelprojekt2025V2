using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float shakeAmount = 0.02f; //Shake amount
    private Vector3 initialPos; // initialposition av shake

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = initialPos + Random.insideUnitSphere * shakeAmount; //random shake
    }
}

