using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "AudioManagerSettings", menuName = "Project Divine/Audio/AudioManagerSettingsSO")]
public class AudioManagerSettingsSO : ScriptableObject
{
    [Header("SoundEmitterPool settings")]
    [SerializeField] private SoundEmitter poolSoundEmitterPrefab;
    [SerializeField] private bool poolCollectionCheck = true;
    [SerializeField] private int poolDefaultCapacity = 30;
    [SerializeField] private int poolMaxCapacity = 60;
    [SerializeField] private bool poolObjectSetActiveOnGet = true;

    [Header("Music player setup")]
    [SerializeField] private bool playMusicOnStart;

    [Header("Audio control")]
    [SerializeField] private float defaultVolumeValue = 0.9f;
    [SerializeField] private float mixerMultiplier = 20f;
    [SerializeField] private AudioMixer audioMixer = default;
    [SerializeField] private List<VolumeTypeItem> volumeExposedParamNames = new List<VolumeTypeItem>();


    // getters
    public SoundEmitter PoolSoundEmitterPrefab => poolSoundEmitterPrefab;
    public bool PoolCollectionCheck => poolCollectionCheck;
    public int PoolDefaultCapacity => poolDefaultCapacity;
    public int PoolMaxCapacity => poolMaxCapacity;
    public bool PoolObjectSetActiveOnGet => poolObjectSetActiveOnGet;

    public bool PlayMusicOnStart => playMusicOnStart;
    public float DefaultVolumeValue => defaultVolumeValue;
    public float MixerMultiplier => mixerMultiplier;
    public AudioMixer AudioMixer => audioMixer;
    public List<VolumeTypeItem> VolumeExposedParamNames => volumeExposedParamNames;


}
