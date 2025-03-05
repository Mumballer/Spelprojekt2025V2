using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    private Transform player;

    void Start()
    {
       
        player = GameObject.FindGameObjectWithTag("Player").transform;

    }

    void Update()
    {
        if (player != null)
        {
            Vector3 directionToPlayer = player.position - transform.position;
            directionToPlayer.y = 0; 
            if (directionToPlayer != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(directionToPlayer);
            }
        }
    }
}