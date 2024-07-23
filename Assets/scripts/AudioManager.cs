using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public List<AudioClip> musicClips;  // Public list of music files
    public AudioSource audioSource;     // AudioSource component
    public float fadeDuration = 1.0f;   // Duration for fading in and out

    private int currentClipIndex = 0;

    void Start()
    {
        if (musicClips.Count > 0)
        {
            audioSource.clip = musicClips[currentClipIndex];
            StartCoroutine(PlayNextClip());
        }
        else
        {
            Debug.LogWarning("No music clips assigned to AudioManager.");
        }
    }

    IEnumerator PlayNextClip()
    {
        while (true)
        {
            // Play the current clip
            audioSource.Play();
            // Fade in
            yield return StartCoroutine(FadeIn(audioSource, fadeDuration));

            // Wait until the clip is about to end
            yield return new WaitForSeconds(audioSource.clip.length - fadeDuration * 2);

            // Fade out
            yield return StartCoroutine(FadeOut(audioSource, fadeDuration));

            // Switch to the next clip
            currentClipIndex = (currentClipIndex + 1) % musicClips.Count;
            audioSource.clip = musicClips[currentClipIndex];
        }
    }

    IEnumerator FadeIn(AudioSource audioSource, float duration)
    {
        float startVolume = 0.3f;
        audioSource.volume = startVolume;

        while (audioSource.volume < 1.0f)
        {
            audioSource.volume += (1.0f - startVolume) * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.volume = 1.0f;
    }

    IEnumerator FadeOut(AudioSource audioSource, float duration)
    {
        float startVolume = 1.0f;

        while (audioSource.volume > 0.3f)
        {
            audioSource.volume -= (startVolume - 0.3f) * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.volume = 0.3f;
    }
}
