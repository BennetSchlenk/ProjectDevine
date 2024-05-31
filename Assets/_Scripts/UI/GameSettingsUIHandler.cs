using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameSettingsUIHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject settingsPanel;
    #region Unity Callbacks
        
    private void Awake()
    {
        settingsPanel.SetActive(false);
    }

    private void Start()
    {
        
    }

    #endregion

    private void Update()
    {
        if (Keyboard.current[Key.Escape].wasPressedThisFrame)
        {
           settingsPanel.SetActive(!settingsPanel.activeSelf); 
        }
    }
}
