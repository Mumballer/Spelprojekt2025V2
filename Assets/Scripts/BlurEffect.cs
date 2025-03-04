
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

public class BlurEffect : MonoBehaviour
{
    public Volume globalVolume;
    private DepthOfField dof;

    public CanvasGroup canvasGroup;
    public GameObject image;
    public GameObject Breathe;

    public float minFocusDistance; // More blur (closer focus)
    public float maxFocusDistance; // Less blur (farther focus)

    public float minInterval; // Minimum time before changing
    public float maxInterval; // Maximum time before changing
    public float FademinInterval;
    public float FadeMaxInterval;

    public float fadeDuration = 1f;  // Fixed duration for fading in/out
    public float FadeMax = 1f; // Max alpha (fully visible)
    public float FadeMin = 0f; // Min alpha (fully transparent)

    private void Start()
    {
        image.SetActive(true); // Ensure the RawImage is active
        Breathe.SetActive(true); // play breathe

        // Check if we have the DepthOfField effect in the Volume Profile
        if (globalVolume.profile.TryGet<DepthOfField>(out dof))
        {
            // Ensure Depth of Field is enabled in the global volume
            dof.active = true;
            // Start the blur effect coroutine
            StartCoroutine(RandomBlurEffectCoroutine());
            // Start the fade effect coroutine
            StartCoroutine(RandomFadeEffectCoroutine());
        }
        else
        {
            Debug.LogError("Depth of Field not found in the global volume profile.");
        }
    }

    private IEnumerator RandomBlurEffectCoroutine()
    {
        while (true) // Infinite loop to keep changing blur effect
        {
            float randomFocus = Random.Range(minFocusDistance, maxFocusDistance);
            float randomTime = Random.Range(minInterval, maxInterval);

            // Ensure the Depth of Field component is active and can be overridden
            if (dof.focusDistance.overrideState)
            {
                dof.focusDistance.overrideState = true; // Ensure override is enabled
                dof.focusDistance.value = randomFocus;  // Apply random focus distance
            }

            yield return new WaitForSeconds(randomTime); // Wait before changing again
        }
    }

    private IEnumerator RandomFadeEffectCoroutine()
    {
        while (true) // Infinite loop to keep changing fade effect
        {
            // Apply random fade value within the range
            float randomFade = Random.Range(FadeMin, FadeMax);

            // Fade the image in or out over a fixed duration
            float startAlpha = canvasGroup.alpha;
            float timeElapsed = 0f;

            while (timeElapsed < fadeDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, randomFade, timeElapsed / fadeDuration);
                timeElapsed += Time.deltaTime;
                yield return null; // Wait until the next frame
            }

            canvasGroup.alpha = randomFade; // Ensure it finishes exactly at the target value

            // Wait for a random interval before changing again
            float randomWaitTime = Random.Range(FademinInterval, FadeMaxInterval);
            yield return new WaitForSeconds(randomWaitTime);
        }
    }
}
