using UnityEngine;
using Unity.UI;

public class test : MonoBehaviour
{

    public GameObject Blah;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!Blah.activeSelf)
        {
            Blah.SetActive(true);
        }


    }

    /*private void SetAlpha(float alpha)
    {
        if (nametagUIText != null)
        {
            Color currentColor = nametagUIText.color;
            currentColor.a = alpha;
            nametagUIText.color = currentColor;
        }
    }*/

    // Update is called once per frame
    void Update()
    {
        
    }
}
