using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Audio;
using UnityEngine.UI;


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
	public Slider slider;
}

[RequireComponent(typeof(AudioStore),typeof(SoundEmitterPool))]
public class AudioManager : MonoBehaviour
{
	[Header("SoundEmitter setup")]
	[SerializeField] private SoundEmitterPool soundEmitterPool;

	[Header("Music player setup")]
	[SerializeField] private SoundEmitter musicEmitter;
	[SerializeField] private bool playMusicOnStart;

	[Header("Audio control")]
	[SerializeField] private float defaultVolumeValue = 0.9f;
    [SerializeField] private float mixerMultiplier = 20f;
    [SerializeField] private AudioMixer audioMixer = default;
    [SerializeField] private List<VolumeTypeItem> volumeExposedParamNames = new List<VolumeTypeItem>();
    
	public AudioMixer AudioMixer => audioMixer;
	public AudioStore AudioStore { get; private set; }
	public float DefaultVolumeValue => defaultVolumeValue;

    private int currentMusicFileIndex;

    private void Awake()
    {
	    ServiceLocator.Instance.RegisterService(this);
        soundEmitterPool = GetComponent<SoundEmitterPool>();
		        
		AudioStore = GetComponent<AudioStore>();
    }

    private void Start()
    {
		musicEmitter.Init();

		if (playMusicOnStart)
		{
			StartPlayingMusic();
		}

        SetupMixerVolumes();
    }



    private void OnEnable()
    {
		musicEmitter.OnSoundFinishedPlaying += HandleIfMusicFinishedPlaying;
    }


    private void OnDisable()
    {
        musicEmitter.OnSoundFinishedPlaying -= HandleIfMusicFinishedPlaying;
    }

    private void OnDestroy()
    {
		foreach (var item in volumeExposedParamNames)
		{
			PlayerPrefs.SetFloat(item.mixerExposedParamName, GetVolume(item.volumeType));
		}
    }


    #region Music player

    private void HandleIfMusicFinishedPlaying(SoundEmitter soundEmitter)
    {
		if (AudioStore.Musics.Count > 1)
		{
            currentMusicFileIndex = GetRandomIndexDifferentFromPrevious(currentMusicFileIndex, AudioStore.Musics.Count);

            PlayMusicTrack(AudioStore.Musics[currentMusicFileIndex]);
        }
    }

	public void StartPlayingMusic()
	{
		currentMusicFileIndex = UnityEngine.Random.Range(0, AudioStore.Musics.Count - 1);
		PlayMusicTrack(AudioStore.Musics[currentMusicFileIndex]);
	}

    public void PlayMusicTrack(AudioObject audioObject)
    {
		musicEmitter.PlayMusicClip(audioObject.Clip, audioObject.Volume, audioObject.Pitch, false);
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

	#region Play Effect One Shot Functions

	private bool FindEffectAudioObjectInStore(string name, out AudioObject audioObject)
	{
		audioObject = null;
		audioObject = AudioStore.Effects.Find(item => item.Name.ToUpper() == name.ToUpper());
		return audioObject != null;
	}

    public void PlaySFXOneShotAtPosition(string name, Vector3 position)
    {
		if (FindEffectAudioObjectInStore(name, out AudioObject audioObject)) 
		{
            PlaySFXOneShotAtPosition(audioObject.Clip, audioObject.Volume, audioObject.Pitch, position);
		}
		else
		{
			Debug.LogWarning($"Did not found {name} in AudioStore.");
		}
    }

    public void PlaySFXOneShotAtPosition(AudioObject audioObject, Vector3 position)
    {
        PlaySFXOneShotAtPosition(audioObject.Clip, audioObject.Volume, audioObject.Pitch, position);
	}
    
    public void PlaySFXOneShotAtPosition(AudioClip clip, float volume, float pitch, Vector3 position)
    {
		var soundEmitter = soundEmitterPool.Pool.Get();
		soundEmitter.PlayClipOneShotAtPosition(clip,volume, pitch, position);            
    }

    #endregion


    #region Volume handling

	public void RegisterSlider(AudioVolumeSliderController volumeController, VolumeType volumeType, Slider slider)
	{
		VolumeTypeItem item = volumeExposedParamNames.Find(x => (x.volumeType == volumeType));
		if (item != null)
		{
			item.slider = slider;
		}
	}

    private void SetupMixerVolumes()
    {
		foreach (var item in volumeExposedParamNames)
		{
			float initVolumeValue = PlayerPrefs.GetFloat(item.mixerExposedParamName, defaultVolumeValue);
			ChangeVolume(item.volumeType, initVolumeValue);
        }

    }

    public void ChangeVolume(VolumeType volumeType, float newVolume, bool isVolumeNormalized = true)
	{
		if (isVolumeNormalized)
		{
			SetGroupVolumeFromNormalized(GetVolumeTypeMixerName(volumeType), newVolume);
		}
		else
		{
            SetGroupVolumeFromMixer(GetVolumeTypeMixerName(volumeType), newVolume);
        }
	}

	/// <summary>
	/// Returns the slide normalized float between 0f and 1f
	/// </summary>
	/// <param name="volumeType"></param>
	/// <returns></returns>
	public float GetVolume(VolumeType volumeType)
	{		
		return GetGroupVolume(GetVolumeTypeMixerName(volumeType));
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

    public void SetGroupVolumeFromNormalized(string parameterName, float normalizedVolume)
	{
		bool volumeSet = audioMixer.SetFloat(parameterName, NormalizedToMixerValue(normalizedVolume));
		if (!volumeSet)
			Debug.LogError("The AudioMixer parameter was not found");
	}

	public void SetGroupVolumeFromMixer(string parameterName, float mixerVolume)
	{
		bool volumeSet = audioMixer.SetFloat(parameterName, mixerVolume);
		if (!volumeSet)
			Debug.LogError("The AudioMixer parameter was not found");
	}

	#endregion

	// Both MixerValueNormalized and NormalizedToMixerValue functions are used for easier transformations
	/// when using UI sliders normalized format
	public float MixerValueToNormalized(float mixerValue)
	{
        // Clamp dB value to avoid issues with calculations at extremes
        mixerValue = Mathf.Clamp(mixerValue, - 80.0f, 0.0f); // Clamp to -80 dB to 0 dB

        // Convert from dB to linear scale using anti-logarithm (10 raised to the power of dB/20)
        float sliderValue = Mathf.Pow(10.0f, mixerValue / mixerMultiplier);

        // Clamp slider value to valid range (0.0 to 1.0)
        sliderValue = Mathf.Clamp01(sliderValue);

        return sliderValue;
    }


    private float NormalizedToMixerValue(float sliderValue)
	{
        
		// Clamp slider value to avoid issues at 0 or 1
        sliderValue = Mathf.Clamp01(sliderValue);

        // Convert to decibel (dB) using log10 with a factor of 20
        // This factor converts linear scale (0-1) to logarithmic dB scale (-80 to 0 dB)
        float dB = mixerMultiplier * Mathf.Log10(sliderValue);

        // Optional: Limit dB to a specific range (adjust as needed)
        dB = Mathf.Clamp(dB, -80.0f, 0.0f); // Clamp to -80 dB (inaudible) to 0 dB (full volume)

        return dB;
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
}
