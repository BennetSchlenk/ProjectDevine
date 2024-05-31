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

[RequireComponent(typeof(AudioStore),typeof(SoundEmitterPool))]
public class AudioManager : MonoBehaviour
{
	[Header("SoundEmitter setup")]
	[SerializeField] private SoundEmitterPool soundEmitterPool;

	[Header("Music player setup")]
	[SerializeField] private SoundEmitter musicEmitter;

	[Header("Audio control")]	
	[SerializeField] private float mixerMultiplier = 20f;
    [SerializeField] private AudioMixer audioMixer = default;
	[SerializeField] private AudioVolumeUIManager volumeUIManager;
    [SerializeField] private List<VolumeTypeItem> volumeExposedParamNames = new List<VolumeTypeItem>();
    
	public AudioMixer AudioMixer => audioMixer;
	public AudioStore AudioStore { get; private set; }

    private int currentMusicFileIndex;
	//private AudioSourcePool soundEmitterPool;

    private void Awake()
    {
	    ServiceLocator.Instance.RegisterService(this);
        soundEmitterPool = GetComponent<SoundEmitterPool>();

        //soundEmitterPool = new AudioSourcePool(soundEmitterPrefab, this.transform, prewarmSize);
        
		AudioStore = GetComponent<AudioStore>();
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

		musicEmitter.Init();
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

 //   #region Play Clip Functions

	////
 //   public void PlayerLooseHPSound(Vector3 position)
 //   {
	//	// TODO this should be moved out of here
	//    PlaySFXOneShotAtPosition("looseHPClip", position);
 //   }
 //   #endregion 


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
}
