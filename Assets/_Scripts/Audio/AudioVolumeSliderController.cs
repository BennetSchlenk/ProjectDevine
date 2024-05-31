using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class AudioVolumeSliderController : MonoBehaviour
{
    [SerializeField] VolumeType type;

    [SerializeField] Slider slider;
    [SerializeField] float defaultValue = 0.8f;    
        
    private string volumeParameter;
    private AudioManager audioManager;
    private AudioVolumeUIManager volumeManager;


    #region Unity Callbacks

    private void OnDestroy()
    {
        //save sound levels
        PlayerPrefs.SetFloat(volumeParameter, slider.value);
    }

    

    #endregion

    public void Init(AudioVolumeUIManager volumeManager, AudioManager audioManager)
    {
        this.volumeManager = volumeManager;
        this.audioManager = audioManager;

        volumeParameter = audioManager.GetVolumeTypeMixerName(type);

        slider.onValueChanged.AddListener(HandleSliderValueChanged);        

        // load and set volume
        float initVolumeValue = PlayerPrefs.GetFloat(volumeParameter, defaultValue);

        //mixer.SetFloat(volumeParameter, Mathf.Log10(initVolumeValue) * multiplier);
        audioManager.ChangeVolume(type, initVolumeValue);
                        
        slider.value = initVolumeValue;
    }

    private void HandleSliderValueChanged(float value)
    {
        //mixer.SetFloat(volumeParameter, Mathf.Log10(value) * multiplier);
        audioManager.ChangeVolume(type, value);
    }

    public float GetVolume()
    {
        return slider.value;
    }

    public VolumeType GetVolumeType()
    {
        return type;
    }

    public float GetDefaultVolume()
    {
        return defaultValue;
    }

}
