using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

    
public class AudioManager : MonoBehaviour
{
	[Header("SoundEmitter setup")]
	[SerializeField] AudioSource soundEmitterPrefab;
	[SerializeField] int prewarmSize = 10;

	[Header("Music player setup")]
	[SerializeField] SoundEmitter musicEmitter;
	[SerializeField] List<AudioFileSO> musicFiles;

    [Header("Audio control")]
    [SerializeField] AudioMixer audioMixer = default;
    [Range(0f, 1f)]
    [SerializeField] float masterVolume = .8f;
    [Range(0f, 1f)]
    [SerializeField] float musicVolume = .8f;
    [Range(0f, 1f)]
    [SerializeField] float sfxVolume = .8f;

    public static AudioManager Instance;

    const string MASTER_VOLUME_PARAM_NAME = "MasterVolume";
	const string MUSIC_VOLUME_PARAM_NAME = "MusicVolume";
	const string SFX_VOLUME_PARAM_NAME = "SFXVolume";

	private int currentMusicFileIndex;
	private AudioSourcePool soundEmitterPool;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        soundEmitterPool = new AudioSourcePool(soundEmitterPrefab, this.transform, prewarmSize);
    }

    private void OnEnable()
    {
		musicEmitter.OnSoundFinishedPlaying += HandleIfMusicFinishedPlaying;
    }


    private void OnDisable()
    {
        musicEmitter.OnSoundFinishedPlaying -= HandleIfMusicFinishedPlaying;
    }

	#region Music player

	private void HandleIfMusicFinishedPlaying(SoundEmitter soundEmitter)
    {
        throw new NotImplementedException();
    }

	public void StartPlayingMusic()
	{
		currentMusicFileIndex = UnityEngine.Random.Range(0, musicFiles.Count - 1);
		PlayMusicTrack(musicFiles[currentMusicFileIndex]);
	}

    public void PlayMusicTrack(AudioFileSO audioFile)
    {
		musicEmitter.PlayAudioClip(audioFile.Clip, audioFile.Settings, audioFile.IsLooping);
    }

	public void StopMusicTrack()
	{
		if (musicEmitter != null && musicEmitter.IsPlaying())
		{
			musicEmitter.Stop();
		}
	}

    #endregion

    public void PlaySFX(AudioFileSO audioFile)
    {
		var soundEmitter = soundEmitterPool.Request();
		soundEmitter.PlayAudioClip(audioFile.Clip, audioFile.Settings, audioFile.IsLooping);
	}

    /// <summary>
    /// This is only used in the Editor, to debug volumes.
    /// It is called when any of the variables is changed, and will directly change the value of the volumes on the AudioMixer.
    /// </summary>
    void OnValidate()
	{
		if (Application.isPlaying)
		{
			SetGroupVolume(MASTER_VOLUME_PARAM_NAME, masterVolume);
			SetGroupVolume(MUSIC_VOLUME_PARAM_NAME, musicVolume);
			SetGroupVolume(SFX_VOLUME_PARAM_NAME, sfxVolume);
		}
	}

    #region Volume handling

    public void ChangeMasterVolume(float newVolume)
	{
		masterVolume = newVolume;
		SetGroupVolume(MASTER_VOLUME_PARAM_NAME, masterVolume);
	}

    public void ChangeMusicVolume(float newVolume)
	{
		musicVolume = newVolume;
		SetGroupVolume(MUSIC_VOLUME_PARAM_NAME, musicVolume);
	}

    public void ChangeSFXVolume(float newVolume)
	{
		sfxVolume = newVolume;
		SetGroupVolume(SFX_VOLUME_PARAM_NAME, sfxVolume);
	}

	public void SetGroupVolume(string parameterName, float normalizedVolume)
	{
		bool volumeSet = audioMixer.SetFloat(parameterName, NormalizedToMixerValue(normalizedVolume));
		if (!volumeSet)
			Debug.LogError("The AudioMixer parameter was not found");
	}

	public float GetGroupVolume(string parameterName)
	{
		if (audioMixer.GetFloat(parameterName, out float rawVolume))
		{
			return MixerValueToNormalized(rawVolume);
		}
		else
		{
			Debug.LogError("The AudioMixer parameter was not found");
			return 0f;
		}
	}

	#endregion

	// Both MixerValueNormalized and NormalizedToMixerValue functions are used for easier transformations
	/// when using UI sliders normalized format
	private float MixerValueToNormalized(float mixerValue)
	{
		// We're assuming the range [-80dB to 0dB] becomes [0 to 1]
		return 1f + (mixerValue / 80f);
	}

	private float NormalizedToMixerValue(float normalizedValue)
	{
		// We're assuming the range [0 to 1] becomes [-80dB to 0dB]
		// This doesn't allow values over 0dB
		return (normalizedValue - 1f) * 80f;
	}

}
