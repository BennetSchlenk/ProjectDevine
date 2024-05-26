using UnityEngine;

[CreateAssetMenu(fileName = "newAudioFile", menuName = "Project Divine/Audio/Audio File")]
public class AudioFileSO : ScriptableObject
{
	[SerializeField] AudioClip audioClip = default;
	[SerializeField] AudioSourceConfigurationSO settings = default;
	[SerializeField] bool looping = false;
	//[SerializeField][Range(0f, 1f)] float volume = 1f;

	public AudioClip Clip => audioClip;
	//public float Volume => volume;
	public bool IsLooping => looping;
	public AudioSourceConfigurationSO Settings => settings;
}
