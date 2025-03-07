using UnityEngine;
using System.Collections;

public class Gramophone : MonoBehaviour
{
    [Header("Audio Settings")]
    // musikfil att spela
    [SerializeField] private AudioClip musicClip;
    // volym för musiken
    [SerializeField] private float volumeLevel = 0.5f;
    // tid för volymändring
    [SerializeField] private float fadeTime = 1.0f;

    private AudioSource audioSource;
    private bool isPlaying = false;
    private Coroutine fadeCoroutine;

    // kolla om spelas
    public bool IsPlaying => isPlaying;

    private void Start()
    {
        // konfigurera ljudkälla
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

    // växla musikstatus
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

        // öka volymen gradvis
        fadeCoroutine = StartCoroutine(FadeAudio(0, volumeLevel, fadeTime));
    }

    private void StopMusic()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        // sänk volymen gradvis
        fadeCoroutine = StartCoroutine(FadeAudio(audioSource.volume, 0, fadeTime));
    }

    private IEnumerator FadeAudio(float startVolume, float targetVolume, float duration)
    {
        float timeElapsed = 0;
        audioSource.volume = startVolume;

        // mjuk volymförändring
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

    // för testning
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