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
	public void PlayAudioClip(AudioClip clip, AudioSourceConfigurationSO settings, bool hasToLoop, Vector3 position = default)
	{
		audioSource.clip = clip;
		settings.ApplyTo(audioSource);
		audioSource.transform.position = position;
		audioSource.loop = hasToLoop;
		audioSource.time = 0f; //Reset in case this AudioSource is being reused for a short SFX after being used for a long music track
		audioSource.Play();

		if (!hasToLoop)
		{
			StartCoroutine(FinishedPlaying(clip.length));
		}
	}

	//public void FadeMusicIn(AudioClip musicClip, AudioSourceConfigurationSO settings, float duration, float startTime = 0f)
	//{
	//	PlayAudioClip(musicClip, settings, true);
	//	_audioSource.volume = 0f;

	//	//Start the clip at the same time the previous one left, if length allows
	//	//TODO: find a better way to sync fading songs
	//	if (startTime <= _audioSource.clip.length)
	//		_audioSource.time = startTime;

	//	_audioSource.DOFade(settings.Volume, duration);
	//}

	//public float FadeMusicOut(float duration)
	//{
	//	_audioSource.DOFade(0f, duration).onComplete += OnFadeOutComplete;

	//	return _audioSource.time;
	//}

	//private void OnFadeOutComplete()
	//{
	//	NotifyBeingDone();
	//}

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
		//OnSoundFinishedPlaying.Invoke(this); // The AudioManager will pick this up
	}
}

