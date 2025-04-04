using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
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
    bool hasPlayed = false; // Tracks Boom sound
    public Transform player;
    public GameObject childObject;

    // New variables for speed ramping
    public float initialSpeed = 1.5f;
    public float maxSpeed = 5.0f;
    public float speedIncreaseRate = 0.1f;
    public float speedIncreaseDelay = 5.0f;
    private float timeElapsed = 0.0f;
    private bool speedIncreaseStarted = false;

    // Optional: audio pitch adjustment with speed
    public bool adjustAudioWithSpeed = true;
    public float minPitch = 1.0f;
    public float maxPitch = 1.5f;

    // New: X Position Threshold for Detection
    public float crossXThreshold = 5.0f;
    private bool playerHasCrossed = false;

    void Start()
    {
        stalkerAgent = GetComponent<NavMeshAgent>();
        stalkerAgent.speed = initialSpeed;

        stalkerAgent.isStopped = true;
        enemyAnimation = theEnemy.GetComponent<Animation>();
        footstepAudio = theEnemy.GetComponent<AudioSource>();
        Boom = childObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        // Check if player has crossed the X threshold
        if (!playerHasCrossed && player.position.x > crossXThreshold)
        {
            playerHasCrossed = true;
            StartChase();
        }

        if (playerHasCrossed)
        {
            // Gradually increase speed
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= speedIncreaseDelay && !speedIncreaseStarted)
            {
                speedIncreaseStarted = true;
                timeElapsed = 0;
            }

            if (speedIncreaseStarted)
            {
                stalkerAgent.speed = Mathf.Min(initialSpeed + (speedIncreaseRate * timeElapsed), maxSpeed);
                animSpeed = Mathf.Lerp(1.0f, 1.5f, (stalkerAgent.speed - initialSpeed) / (maxSpeed - initialSpeed));

                if (adjustAudioWithSpeed && footstepAudio != null)
                {
                    footstepAudio.pitch = Mathf.Lerp(minPitch, maxPitch,
                        (stalkerAgent.speed - initialSpeed) / (maxSpeed - initialSpeed));
                }
            }

            // Set enemy destination to follow player
            stalkerAgent.SetDestination(player.position);
            stalkerAgent.isStopped = false;
        }
    }

    void StartChase()
    {
        // Start playing Boom sound if it hasn't been played yet
        if (!hasPlayed)
        {
            Boom.Play();
            hasPlayed = true;
        }

        // Start footstep sound
        if (!footstepAudio.isPlaying)
        {
            footstepAudio.Play();
        }
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
