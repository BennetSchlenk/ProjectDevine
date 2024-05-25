using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "newAudioConfig", menuName = "Project Divine/Audio/Audio Configuration")]
public class AudioSourceConfigurationSO : ScriptableObject
{
	public AudioMixerGroup MixerGroup = null;

	[Header("Sound properties")]
	public bool Mute = false;
	[Range(0f, 1f)] public float Volume = 1f;
	[Range(-3f, 3f)] public float Pitch = 1f;
	[Range(-1f, 1f)] public float PanStereo = 0f;

	public void ApplyTo(AudioSource audioSource)
	{
		audioSource.outputAudioMixerGroup = MixerGroup;
		audioSource.mute = Mute;			
		audioSource.volume = Volume;
		audioSource.pitch = Pitch;
		audioSource.panStereo = PanStereo;
	}
}
