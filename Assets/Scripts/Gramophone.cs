using UnityEngine;
using System.Collections;

public class Gramophone : MonoBehaviour
{
    [Header("Audio Settings")]
    // Referens till ljudkällan istället för ljudklippet
    [SerializeField] private GameObject soundSource;
    // volym för musiken
    [SerializeField] private float volumeLevel = 0.5f;
    // tid för volymändring
    [SerializeField] private float fadeTime = 1.0f;

    private AudioSource targetAudioSource;
    private bool isPlaying = false;
    private Coroutine fadeCoroutine;

    // kolla om spelas
    public bool IsPlaying => isPlaying;

    private void Start()
    {
        // Hämta ljudkällan från det refererade objektet
        if (soundSource != null)
        {
            targetAudioSource = soundSource.GetComponent<AudioSource>();
            if (targetAudioSource == null)
            {
                Debug.LogError("Sound source object does not have an AudioSource component!");
            }
            else
            {
                // Konfigurera ljudkällan
                targetAudioSource.volume = 0;
                targetAudioSource.loop = true;
                targetAudioSource.playOnAwake = false;
                targetAudioSource.spatialBlend = 1f; // 3D sound
            }
        }
        else
        {
            Debug.LogError("Sound source reference is missing!");
        }
    }

    // växla musikstatus
    public void ToggleMusic()
    {
        if (targetAudioSource == null) return;

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
        if (targetAudioSource == null) return;

        if (!targetAudioSource.isPlaying)
        {
            targetAudioSource.Play();
        }

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        // öka volymen gradvis
        fadeCoroutine = StartCoroutine(FadeAudio(0, volumeLevel, fadeTime));
    }

    private void StopMusic()
    {
        if (targetAudioSource == null) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        // sänk volymen gradvis
        fadeCoroutine = StartCoroutine(FadeAudio(targetAudioSource.volume, 0, fadeTime));
    }

    private IEnumerator FadeAudio(float startVolume, float targetVolume, float duration)
    {
        if (targetAudioSource == null) yield break;

        float timeElapsed = 0;
        targetAudioSource.volume = startVolume;

        // mjuk volymförändring
        while (timeElapsed < duration)
        {
            targetAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        targetAudioSource.volume = targetVolume;

        if (targetVolume <= 0.01f && targetAudioSource.isPlaying)
        {
            targetAudioSource.Stop();
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