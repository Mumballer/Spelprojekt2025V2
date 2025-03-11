using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    // Existing variables
    public GameObject stalkerDes;
    NavMeshAgent stalkerAgent;
    public GameObject theEnemy;
    public float normalSpeed = 0.01f;
    public float chaseSpeed = 0.05f;
    private float enemySpeed;
    public float detectionRange = 10f;
    public LayerMask playerLayer;
    public float animSpeed = 1.0f;
    public float footstepDistance;
    public float maxVolume;
    Animation enemyAnimation;
    AudioSource footstepAudio;
    AudioSource Boom;
    bool hasPlayed = false;
    public Transform player;
    public GameObject childObject;

    // New variables for speed ramping
    public float initialSpeed = 1.5f;     // Starting speed for NavMeshAgent
    public float maxSpeed = 5.0f;         // Maximum speed the agent can reach
    public float speedIncreaseRate = 0.1f; // How much to increase speed per second
    public float speedIncreaseDelay = 5.0f; // Time in seconds before speed starts increasing
    private float timeElapsed = 0.0f;      // Track elapsed time
    private bool speedIncreaseStarted = false;

    // Optional: audio pitch adjustment with speed
    public bool adjustAudioWithSpeed = true;
    public float minPitch = 1.0f;
    public float maxPitch = 1.5f;

    void Start()
    {
        stalkerAgent = GetComponent<NavMeshAgent>();
        stalkerAgent.speed = initialSpeed; // Set initial speed

        stalkerAgent.Stop();
        enemyAnimation = theEnemy.GetComponent<Animation>();
        footstepAudio = theEnemy.GetComponent<AudioSource>();
        Boom = childObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        // Update elapsed time
        timeElapsed += Time.deltaTime;

        // Start increasing speed after delay
        if (timeElapsed >= speedIncreaseDelay && !speedIncreaseStarted)
        {
            speedIncreaseStarted = true;
            timeElapsed = 0;
        }

        // Gradually increase speed if started
        if (speedIncreaseStarted)
        {
            // Increase speed gradually up to max speed
            stalkerAgent.speed = Mathf.Min(initialSpeed + (speedIncreaseRate * timeElapsed), maxSpeed);

            // Optional: Adjust animation speed with agent speed
            animSpeed = Mathf.Lerp(1.0f, 1.5f, (stalkerAgent.speed - initialSpeed) / (maxSpeed - initialSpeed));

            // Optional: Adjust audio pitch with speed
            if (adjustAudioWithSpeed && footstepAudio != null)
            {
                footstepAudio.pitch = Mathf.Lerp(minPitch, maxPitch,
                    (stalkerAgent.speed - initialSpeed) / (maxSpeed - initialSpeed));
            }
        }

        // Set destination (enemy follows player)
        stalkerAgent.SetDestination(player.position);
        stalkerAgent.SetDestination(stalkerDes.transform.position);

        // Handle footstep audio based on distance
        float distanceToPlayer = Vector3.Distance(transform.position, stalkerDes.transform.position);

        if (distanceToPlayer <= footstepDistance)
        {
            if (!hasPlayed)
            {
                Boom.Play();
                hasPlayed = true;
            }

            footstepAudio.volume = Mathf.Lerp(0, maxVolume, (footstepDistance - distanceToPlayer) / footstepDistance);

            if (!footstepAudio.isPlaying && footstepAudio.volume > 0.01f)
            {
                footstepAudio.Play();
            }
        }
        else
        {
            if (footstepAudio.isPlaying)
            {
                footstepAudio.Stop();
            }
        }

        // Handle player detection
        if (IsPlayerInSight() == true)
        {
            stalkerAgent.Resume();
        }
        else
        {
            stalkerAgent.Stop();
        }

        // Check if agent has reached destination
        if (stalkerAgent.remainingDistance > stalkerAgent.stoppingDistance)
        {
            stalkerAgent.isStopped = false;
        }
        else
        {
            stalkerAgent.isStopped = true;
        }
    }

    bool IsPlayerInSight()
    {
        Vector3 directionToPlayer = stalkerDes.transform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= detectionRange)
        {
            Debug.Log("Less than detection range");
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartJumpscare();
        }
    }

    public void StartJumpscare()
    {
        footstepAudio.Stop();
    }

    void ReloadScene()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}