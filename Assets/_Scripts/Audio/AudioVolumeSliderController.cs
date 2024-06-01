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
      
    private AudioManager audioManager;
    
    private bool wasInitialized = false;

    #region Unity Callbacks

    private void OnEnable()
    {
        audioManager = ServiceLocator.Instance.GetService<AudioManager>();

        Assert.IsNotNull(audioManager, "AudioVolumeSliderController: AudioManager should be available");

        audioManager.RegisterSlider(this, type, slider);

        if (!wasInitialized)
        {
            wasInitialized = true;
            slider.onValueChanged.AddListener(HandleSliderValueChanged);
        }        

        slider.value = audioManager.GetVolume(type);
    }
        

    #endregion


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


}
