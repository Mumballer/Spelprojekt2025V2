using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerScene : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        // Kolla om det är spelaren som kolliderar
        if (collision.gameObject.CompareTag("Door"))
        {
            // Byt scen, exempelvis till scenen med index 1
            SceneManager.LoadScene("FinalScene");  // Byt till den scen du vill
            Debug.Log("Broski");
        }

    }
}
