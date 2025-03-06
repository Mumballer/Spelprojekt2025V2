using UnityEngine;
using System.Collections;

public class Gramophone : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private float volumeLevel = 0.5f;
    [SerializeField] private float fadeTime = 1.0f;

    private AudioSource audioSource;
    private bool isPlaying = false;
    private Coroutine fadeCoroutine;

    // Public property to check if music is playing
    public bool IsPlaying => isPlaying;

    private void Start()
    {
        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = musicClip;
        audioSource.volume = 0;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
    }

    // Public method for toggling music state
    public void ToggleMusic()
    {
        isPlaying = !isPlaying;

        if (isPlaying)
        {
            StartMusic();
            Debug.Log($"Gramophone started playing: {name}");
        }
        else
        {
            StopMusic();
            Debug.Log($"Gramophone stopped playing: {name}");
        }
    }

    private void StartMusic()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeAudio(0, volumeLevel, fadeTime));
    }

    private void StopMusic()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeAudio(audioSource.volume, 0, fadeTime));
    }

    private IEnumerator FadeAudio(float startVolume, float targetVolume, float duration)
    {
        float timeElapsed = 0;
        audioSource.volume = startVolume;

        while (timeElapsed < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = targetVolume;

        if (targetVolume <= 0.01f && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        fadeCoroutine = null;
    }

    // For editor testing
    public void ForcePlayMusic()
    {
        isPlaying = true;
        StartMusic();
    }

    public void ForceStopMusic()
    {
        isPlaying = false;
        StopMusic();
    }
}