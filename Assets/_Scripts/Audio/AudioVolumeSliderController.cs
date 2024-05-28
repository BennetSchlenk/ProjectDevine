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
    //[SerializeField] float multiplier = 30f;
    [SerializeField] float defaultValue = 0.8f;
    [SerializeField] Toggle toggle;

    private bool disableToggleEvent = false;
    private string volumeParameter;
    private AudioManager audioManager;
    private AudioVolumeUIManager volumeManager;


    #region Unity Callbacks

    private void OnDestroy()
    {
        //save sound levels
        PlayerPrefs.SetFloat(volumeParameter, slider.value);
        PlayerPrefs.SetInt(volumeParameter + "Toggle", toggle.isOn ? 1 : 0);
    }

    

    #endregion

    public void Init(AudioVolumeUIManager volumeManager, AudioManager audioManager)
    {
        this.volumeManager = volumeManager;
        this.audioManager = audioManager;

        volumeParameter = audioManager.GetVolumeTypeMixerName(type);

        slider.onValueChanged.AddListener(HandleSliderValueChanged);
        toggle.onValueChanged.AddListener(HandleToggleValueChanged);

        // load and set volume
        float initVolumeValue = PlayerPrefs.GetFloat(volumeParameter, defaultValue);
        bool initToggleValue = PlayerPrefs.GetInt(volumeParameter + "Toggle", 1) == 1;

        //mixer.SetFloat(volumeParameter, Mathf.Log10(initVolumeValue) * multiplier);
        audioManager.ChangeVolume(type, initVolumeValue);

        //toggle.isOn = initVolumeValue > slider.minValue;
        toggle.isOn = initToggleValue;
        slider.value = initVolumeValue;
    }

    private void HandleToggleValueChanged(bool enableSound)
    {
        audioManager.ToggleVolumeOnOff(type, enableSound);

        if (disableToggleEvent) return;

        if (enableSound)
        {
            slider.value = defaultValue;
        }
        else
        {
            slider.value = slider.minValue;
        }
    }

    private void HandleSliderValueChanged(float value)
    {
        //mixer.SetFloat(volumeParameter, Mathf.Log10(value) * multiplier);
        audioManager.ChangeVolume(type, value);

        disableToggleEvent = true;
        toggle.isOn = slider.value > slider.minValue;
        disableToggleEvent = false;
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
