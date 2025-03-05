using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement; // Add this for scene management

public class EnemyAI : MonoBehaviour
{
    // Existing variables...
    public GameObject stalkerDes;
    NavMeshAgent stalkerAgent;
    public GameObject theEnemy;
    public float normalSpeed = 0.01f;
    public float chaseSpeed = 0.05f;
    private float enemySpeed;
    public float detectionRange = 10f;
    public LayerMask playerLayer;
    public float animSpeed = 1.0f;
    public float footstepDistance = 5f;
    public float maxVolume = 50f;
    Animation enemyAnimation;
    AudioSource footstepAudio;
    public string walkAnim = "Walk";
    public string runAnim = "Run";
    public Transform player;
    private bool isJumpscaring = false;

    // New variables for chase music and background music
    //public AudioSource Chasemusic;
    //public AudioSource BackgroundMusic;

    void Start()
    {
        stalkerAgent = GetComponent<NavMeshAgent>();
        enemySpeed = normalSpeed;
        enemyAnimation = theEnemy.GetComponent<Animation>();
        footstepAudio = theEnemy.GetComponent<AudioSource>();
        footstepAudio.spatialBlend = 1f;
        // Ensure chase music is not playing at the start
        //Chasemusic.Stop();
    }

    void Update()
    {
        if (isJumpscaring) return;

        stalkerAgent.SetDestination(player.position);
        stalkerAgent.SetDestination(stalkerDes.transform.position);

        float distanceToPlayer = Vector3.Distance(transform.position, stalkerDes.transform.position);
        //HandleFootstepAudio(distanceToPlayer);

        if (IsPlayerInSight())
        {
            enemySpeed = chaseSpeed;
            enemyAnimation[runAnim].speed = animSpeed;
            if (!enemyAnimation.IsPlaying(runAnim))
            {
                enemyAnimation.Play(runAnim);


            }
        }
        else
        {
            enemySpeed = normalSpeed;
            enemyAnimation[walkAnim].speed = animSpeed;
            /*if (!enemyAnimation.IsPlaying(walkAnim))
            {
                enemyAnimation.Play(walkAnim);

                if (Chasemusic.isPlaying)
                {
                    Chasemusic.Stop();
                    if (!BackgroundMusic.isPlaying)
                    {
                        BackgroundMusic.Play();
                    }
                }
            }*/
        }

        transform.position = Vector3.MoveTowards(transform.position, stalkerDes.transform.position, enemySpeed);

        if (stalkerAgent.remainingDistance > stalkerAgent.stoppingDistance)
        {
            stalkerAgent.isStopped = false;
        }
        else
        {
            stalkerAgent.isStopped = true;
            Wander();
        }
    }

    bool IsPlayerInSight()
    {
        Vector3 directionToPlayer = stalkerDes.transform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= detectionRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer.normalized, out hit, detectionRange, playerLayer))
            {
                if (hit.transform == stalkerDes.transform)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /*public void HandleFootstepAudio(float distanceToPlayer)
    {
        if (distanceToPlayer <= footstepDistance)
        {
            // Calculate volume based on distance.
            float calculatedVolume = Mathf.Lerp(0, maxVolume, (footstepDistance - distanceToPlayer) / footstepDistance);

            // Ensure the volume is being updated correctly.
            footstepAudio.volume = Mathf.Clamp(calculatedVolume, 0, maxVolume);  // Clamp the volume between 0 and maxVolume

            // Play footstep audio if it's not playing and the volume is above a threshold.
            if (!footstepAudio.isPlaying && footstepAudio.volume > 0.01f)
            {
                footstepAudio.Play();
            }
        }
        else
        {
            // Stop the sound if the player is far away.
            footstepAudio.volume = 0;
            if (footstepAudio.isPlaying)
            {
                footstepAudio.Stop();
            }
        }
    }*/



    public void StartJumpscare()
    {
        isJumpscaring = true;
        footstepAudio.Stop();
        //Chasemusic.Stop();
        //BackgroundMusic.Stop();

        // Reload the scene after a short delay
        Invoke("ReloadScene", 1f); // 2 second delay for effect (adjust as needed)
    }

    void ReloadScene()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Wander()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 5f;
        randomDirection += transform.position;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, 5f, 1))
        {
            stalkerAgent.SetDestination(hit.position);
        }
    }
}
