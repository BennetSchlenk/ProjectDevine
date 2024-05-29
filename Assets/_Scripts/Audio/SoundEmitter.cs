using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour
{
    private AudioSource audioSource;

    public event UnityAction<SoundEmitter> OnSoundFinishedPlaying;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// Instructs the AudioSource to play a single clip, with optional looping, in a position in 3D space.
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="settings"></param>
    /// <param name="hasToLoop"></param>
    /// <param name="position"></param>
    public void PlayAudioClip(AudioClip clip, AudioSourceConfigurationSO settings, bool hasToLoop,
        Vector3 position = default)
    {
        audioSource.clip = clip;
        settings.ApplyTo(audioSource);
        audioSource.transform.position = position;
        audioSource.loop = hasToLoop;
        audioSource.time =
            0f; //Reset in case this AudioSource is being reused for a short SFX after being used for a long music track
        audioSource.Play();

        if (!hasToLoop)
        {
            StartCoroutine(FinishedPlaying(clip.length));
        }
    }

    public void PlayClipOnShotAtPosition(AudioClip clip, float volume, float pitch, Vector3 position = default)
    {
        audioSource.clip = clip;

        audioSource.transform.position = position;
        audioSource.panStereo = 0f;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.loop = false;
        audioSource.time =
            0f; //Reset in case this AudioSource is being reused for a short SFX after being used for a long music track
        audioSource.Play();
        
        StartCoroutine(FinishedPlaying(clip.length));
    }

    /// <summary>
    /// Used to check which music track is being played.
    /// </summary>
    public AudioClip GetClip()
    {
        return audioSource.clip;
    }


    /// <summary>
    /// Used when the game is unpaused, to pick up SFX from where they left.
    /// </summary>
    public void Resume()
    {
        audioSource.Play();
    }

    /// <summary>
    /// Used when the game is paused.
    /// </summary>
    public void Pause()
    {
        audioSource.Pause();
    }

    public void Stop()
    {
        audioSource.Stop();
    }

    public void Finish()
    {
        if (audioSource.loop)
        {
            audioSource.loop = false;
            float timeRemaining = audioSource.clip.length - audioSource.time;
            StartCoroutine(FinishedPlaying(timeRemaining));
        }
    }

    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }

    public bool IsLooping()
    {
        return audioSource.loop;
    }

    IEnumerator FinishedPlaying(float clipLength)
    {
        yield return new WaitForSeconds(clipLength);

        NotifyBeingDone();
    }

    private void NotifyBeingDone()
    {
        OnSoundFinishedPlaying?.Invoke(this); // The AudioManager should pick this up
    }
}