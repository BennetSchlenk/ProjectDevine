using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitBootStrapper : MonoBehaviour
{
    [SerializeField] private AudioManagerSettingsSO audioSettings;
    [SerializeField] private AudioStoreSO audioStore;

    #region Unity Callbacks

    private void Awake()
    {
        if(GameObject.Find("GameManager") == null)
        {
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }

        if (GameObject.Find("AudioManager") == null)
        {
            var go = new GameObject("AudioManager");
            AudioManager am = go.AddComponent<AudioManager>();
            am.Init(audioSettings, audioStore);
        }


    }

    private void Start()
    {
        
    }

    #endregion

}
