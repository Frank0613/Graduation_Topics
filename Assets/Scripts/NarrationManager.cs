using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class NarrationGroup
{
    public AudioClip[] audioClips;
    public UnityEvent[] unityEvents;
}

public class NarrationManager : MonoBehaviour
{
    [Header("Narration Settings")]
    [SerializeField] private NarrationGroup[] narrationGroups;
    private AudioSource audioSource;
    private Coroutine currentNarrationCoroutine; // Reference to the current narration coroutine



    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        //Narration_Before_SprayGame();
    }

    #region  Every Narrations
    public void Narration_Before_SprayGame()
    {
        PlayNarration(0, 1, 1);
    }

    public void Narration_After_SprayGame()
    {
        PlayNarration(1, 1, 1);

    }

    public void Narration_When_MatchGame()
    {
        PlayNarration(2, 1, 1);
    }

    public void Narration_After_MatchGame()
    {
        PlayNarration(3, 0, 2);
    }
    public void Narration_FrontDoor()
    {
        PlayNarration(4, 0, 0.8f);
    }
    #endregion

    #region  Audio Settings
    public void PlayNarration(int groupIndex, float start_delay, float every_audio_delay)
    {
        if (groupIndex >= 0 && groupIndex < narrationGroups.Length)
        {
            NarrationGroup group = narrationGroups[groupIndex];

            // Stop any currently playing narration
            StopNarration();

            // Start the new narration
            currentNarrationCoroutine = StartCoroutine(PlayAudioClips(group.audioClips, group.unityEvents, start_delay, every_audio_delay));
        }
    }



    IEnumerator PlayAudioClips(AudioClip[] audioClips, UnityEvent[] events, float start_duration, float every_audio_duration)
    {
        int currentClipIndex = 0;
        yield return new WaitForSeconds(start_duration);

        while (currentClipIndex < audioClips.Length)
        {
            audioSource.clip = audioClips[currentClipIndex];
            yield return new WaitForSeconds(every_audio_duration);
            audioSource.Play();

            yield return new WaitForSeconds(audioSource.clip.length);

            if (currentClipIndex < events.Length && events[currentClipIndex] != null)
            {
                events[currentClipIndex].Invoke();
            }

            currentClipIndex++;
        }

        // Reset the coroutine reference when narration is complete
        currentNarrationCoroutine = null;
    }
    public void StopNarration()
    {
        if (currentNarrationCoroutine != null)
        {
            StopCoroutine(currentNarrationCoroutine);
            currentNarrationCoroutine = null;
        }

        // Stop the currently playing audio
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    #endregion
}
