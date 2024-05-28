using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Audio;


public enum VolumeType
{
    Master,
    Music,
    Effect
}

[System.Serializable]
public class VolumeTypeItem
{
    public VolumeType volumeType;
    public string mixerExposedParamName;
}

public class AudioManager : MonoBehaviour
{
	[Header("SoundEmitter setup")]
	[SerializeField] private AudioSource soundEmitterPrefab;
	[SerializeField] private int prewarmSize = 10;

	[Header("Music player setup")]
	[SerializeField] private SoundEmitter musicEmitter;
	[SerializeField] private List<AudioFileSO> musicFiles;

	[Header("Audio control")]
	[Tooltip("We're assuming the range [-mixerToSlider to 0dB] becomes [0 to 1]")]
	[SerializeField] private float mixerMultiplier = 20f;
    [SerializeField] private AudioMixer audioMixer = default;
	[SerializeField] private AudioVolumeUIManager volumeUIManager;
    [SerializeField] private List<VolumeTypeItem> volumeExposedParamNames = new List<VolumeTypeItem>();

    public static AudioManager Instance;

	public AudioMixer AudioMixer => audioMixer;
	public bool IsMasterEnabled { get; private set; } = true;
	public bool IsMusicEnabled { get; private set; } = true;
    public bool IsEffectEnabled { get; private set; } = true;


    //   const string MASTER_VOLUME_PARAM_NAME = "MasterVolume";
    //const string MUSIC_VOLUME_PARAM_NAME = "MusicVolume";
    //const string SFX_VOLUME_PARAM_NAME = "SFXVolume";

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

    private void Start()
    {
        if (volumeUIManager != null)
        {
			volumeUIManager.InitVolumeControllers();
		}
		else
		{
			Debug.LogWarning("Add Volume UI Manager link!");
		}
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
		if (musicFiles.Count > 1)
		{
            currentMusicFileIndex = GetRandomIndexDifferentFromPrevious(currentMusicFileIndex, musicFiles.Count);

            PlayMusicTrack(musicFiles[currentMusicFileIndex]);
        }
    }

	public void StartPlayingMusic()
	{
		currentMusicFileIndex = UnityEngine.Random.Range(0, musicFiles.Count - 1);
		PlayMusicTrack(musicFiles[currentMusicFileIndex]);
	}

    public void PlayMusicTrack(AudioFileSO audioFile)
    {
        if (IsMusicEnabled)
        {
			musicEmitter.PlayAudioClip(audioFile.Clip, audioFile.Settings, audioFile.IsLooping);
        }
    }

	public void StopMusicTrack()
	{
		if (musicEmitter != null && musicEmitter.IsPlaying())
		{
			musicEmitter.Stop();
		}
	}

    private int GetRandomIndexDifferentFromPrevious(int prevInt, int max)
    {
        Assert.IsTrue(prevInt < 0 || prevInt >= max || max >= 1);

        // Create the list using range
        List<int> intList = new List<int>(Enumerable.Range(0, max));

        // Remove element at the specified index
        intList.RemoveAt(prevInt);

        return intList[UnityEngine.Random.Range(0, max - 1)];
    }

    #endregion

    public void PlaySFX(AudioFileSO audioFile)
    {
        if (IsEffectEnabled)
        {
			var soundEmitter = soundEmitterPool.Request();
			soundEmitter.PlayAudioClip(audioFile.Clip, audioFile.Settings, audioFile.IsLooping);            
        }
	}


    #region Volume handling

    public void ChangeVolume(VolumeType volumeType, float newVolume)
	{
		SetGroupVolume(GetVolumeTypeMixerName(volumeType), newVolume);
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
		return Mathf.Log10(mixerValue * mixerMultiplier);
	}

	private float NormalizedToMixerValue(float normalizedValue)
	{
		return Mathf.Log10(normalizedValue) * mixerMultiplier;
	}


    public string GetVolumeTypeMixerName(VolumeType vt)
    {
        VolumeTypeItem desiredVolumeTypeItem = volumeExposedParamNames.Find(x => x.volumeType == vt);
        if (desiredVolumeTypeItem != null)
        {
            return desiredVolumeTypeItem.mixerExposedParamName;
        }
        else
        {
            Debug.LogWarning($"The {desiredVolumeTypeItem.volumeType} volumeExposedParamNames entry was not found!");
            return "NotFound";
        }
    }

	public void ToggleVolumeOnOff(VolumeType volumeType, bool isOn)
	{
		switch (volumeType)
		{
			case VolumeType.Master:                
                IsMasterEnabled = isOn; 
				break;

            case VolumeType.Music:
				IsMusicEnabled = isOn;
				if (isOn)
				{
					StartPlayingMusic();
				} 
				else 
				{ 
					StopMusicTrack(); 
				}
				break;

			case VolumeType.Effect:
				IsEffectEnabled = isOn;
                break;

			default:
				break;
		}
	}

}
