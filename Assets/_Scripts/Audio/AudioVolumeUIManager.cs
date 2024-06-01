using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioVolumeUIManager : MonoBehaviour
{ 

    private AudioManager audioManager;
    private List<AudioVolumeSliderController> volumeControllers;

    #region Unity Callbacks

    private void Awake()
    {
        
    }


    #endregion

    
    //public void InitVolumeControllers()
    //{
    //    volumeControllers = GetComponentsInChildren<AudioVolumeSliderController>().ToList();
    //    audioManager = ServiceLocator.Instance.GetService<AudioManager>();;
    //    volumeControllers.ForEach(x => x.Init(this, audioManager));
    //}


    //public float GetVolume(VolumeType vt)
    //{
    //    AudioVolumeSliderController desiredVolumeController = volumeControllers.Find(x => x.GetVolumeType() == vt);
    //    if (desiredVolumeController != null)
    //    {
    //        return desiredVolumeController.GetVolume();
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"The {desiredVolumeController.GetVolumeType()} volumeController was not found!");
    //        return 0;
    //    }
    //}

}
