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

[RequireComponent(typeof(SoundEmitterPool), typeof(DontDestroyOnLoad))]
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioManagerSettingsSO audioSettings;
    [SerializeField] private AudioStoreSO audioStore;

	private SoundEmitterPool soundEmitterPool; // needs to be added via awake
    private SoundEmitter musicEmitter; // add a musicEmitter -> add a music emitter
    
	public AudioMixer AudioMixer => audioSettings.AudioMixer;
    public AudioStoreSO AudioStore => audioStore;
    public float DefaultVolumeValue => audioSettings.DefaultVolumeValue;

    private int currentMusicFileIndex;

    private void Awake()
    {
        Debug.Log("audioManager Awake()");
	    ServiceLocator.Instance.RegisterService(this);
       
    }

    private void Start()
    {
        Debug.Log("audioManager Start()");

        musicEmitter.Init();

        SetupMixerVolumes();

		if (audioSettings.PlayMusicOnStart)
		{
			StartPlayingMusic();
		}
    }

    public void Init(AudioManagerSettingsSO audioSettings, AudioStoreSO audioStore)
    {
        this.audioSettings = audioSettings;
        this.audioStore = audioStore;

        soundEmitterPool = gameObject.GetComponent<SoundEmitterPool>();
        soundEmitterPool.InitPoolWithValues(
            gameObject.transform,
            audioSettings.PoolCollectionCheck,
            audioSettings.PoolSoundEmitterPrefab,
            audioSettings.PoolDefaultCapacity,
            audioSettings.PoolMaxCapacity,
            audioSettings.PoolObjectSetActiveOnGet);


        var musicEmitterGO = new GameObject("MusicPlayer");
        musicEmitterGO.transform.SetParent(transform);
        musicEmitter = musicEmitterGO.AddComponent<SoundEmitter>();

        musicEmitter.OnSoundFinishedPlaying += HandleIfMusicFinishedPlaying;
    }


    private void OnEnable()
    {
        Debug.Log("audioManager OnEnable()");
        
    }


    private void OnDisable()
    {
        musicEmitter.OnSoundFinishedPlaying -= HandleIfMusicFinishedPlaying;
    }

    private void OnDestroy()
    {
		foreach (var item in audioSettings.VolumeExposedParamNames)
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
        }

        PlayMusicTrack(AudioStore.Musics[currentMusicFileIndex]);
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
		VolumeTypeItem item = audioSettings.VolumeExposedParamNames.Find(x => (x.volumeType == volumeType));
		if (item != null)
		{
			item.slider = slider;
		}
	}

    private void SetupMixerVolumes()
    {
		foreach (var item in audioSettings.VolumeExposedParamNames)
		{
			float initVolumeValue = PlayerPrefs.GetFloat(item.mixerExposedParamName, audioSettings.DefaultVolumeValue);
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
        if (AudioMixer.GetFloat(parameterName, out float rawVolume))
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
		bool volumeSet = AudioMixer.SetFloat(parameterName, NormalizedToMixerValue(normalizedVolume));
		if (!volumeSet)
			Debug.LogError("The AudioMixer parameter was not found");
	}

	public void SetGroupVolumeFromMixer(string parameterName, float mixerVolume)
	{
		bool volumeSet = AudioMixer.SetFloat(parameterName, mixerVolume);
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
        float sliderValue = Mathf.Pow(10.0f, mixerValue / audioSettings.MixerMultiplier);

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
        float dB = audioSettings.MixerMultiplier * Mathf.Log10(sliderValue);

        // Optional: Limit dB to a specific range (adjust as needed)
        dB = Mathf.Clamp(dB, -80.0f, 0.0f); // Clamp to -80 dB (inaudible) to 0 dB (full volume)

        return dB;
    }


    public string GetVolumeTypeMixerName(VolumeType vt)
    {
        VolumeTypeItem desiredVolumeTypeItem = audioSettings.VolumeExposedParamNames.Find(x => x.volumeType == vt);
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
