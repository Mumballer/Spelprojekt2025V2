using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangerDoor : MonoBehaviour
{
    public string Scene;
    private void OnTriggerEnter(Collider other)
    {
        // Kolla om det är spelaren som kolliderar
        if (other.gameObject.CompareTag("Player"))
        {
            // Byt scen, exempelvis till scenen med index 1
            SceneManager.LoadScene(Scene);  // Byt till den scen du vill
        }

    }


}