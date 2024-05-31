using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Pool;


[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioMixer audioMixer;

    private ObjectPool<SoundEmitter> pool;

    public event UnityAction<SoundEmitter> OnSoundFinishedPlaying;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    #region Pooling related
    
    public void Init()
    {
        // called when when created by Pool
        audioMixer = ServiceLocator.Instance.GetService<AudioManager>().AudioMixer;
    }

    public void OnGet()
    {
        // called when pulled from pool
    }

    public void ReturnToPool()
    {    
        pool.Release(this);
    }

    public void SetPool(ObjectPool<SoundEmitter> pool)
    {
        this.pool = pool;
    }

    #endregion

    /// <summary>
    /// Instructs the AudioSource to play a single clip, with optional looping, in a position in 3D space.
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="volume"></param>
    /// <param name="pitch"></param>
    /// <param name="hasToLoop"></param>
    /// <param name="position"></param>
    public void PlayMusicClip(AudioClip clip, float volume, float pitch, bool hasToLoop, Vector3 position = default)
    {
        audioSource.clip = clip;

        audioSource.transform.position = position;
        audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Music")[0];
        audioSource.panStereo = 0f;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.loop = hasToLoop;
        audioSource.time =
            0f; //Reset in case this AudioSource is being reused for a short SFX after being used for a long music track
        audioSource.Play();

        if (!hasToLoop)
        {
            StartCoroutine(FinishedPlaying(clip.length));
        }
    }

    /// <summary>
    /// Instructs the AudioSource to play a single clip once, in a position in 3D space.
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="volume"></param>
    /// <param name="pitch"></param>
    /// <param name="position"></param>
    public void PlayClipOneShotAtPosition(AudioClip clip, float volume, float pitch, Vector3 position = default)
    {
        audioSource.clip = clip;

        audioSource.transform.position = position;
        audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
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

        //return object to pool
        if (pool != null)
        {
            ReturnToPool();
        }
    }

    private void NotifyBeingDone()
    {
        if (OnSoundFinishedPlaying != null)
        {
            // The AudioManager should pick this up
            OnSoundFinishedPlaying.Invoke(this); 
        }
    }
}