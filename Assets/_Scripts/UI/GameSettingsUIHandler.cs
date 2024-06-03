using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameSettingsUIHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject settingsPanel;
    [SerializeField]
    private Button menuButton;

    private GameManager gameManager;
    #region Unity Callbacks
        
    private void Awake()
    {
        settingsPanel.SetActive(false);
        menuButton.onClick.AddListener(ReturnToMenu);
    }

    private void Start()
    {
        gameManager = ServiceLocator.Instance.GetService<GameManager>();
    }

    #endregion

    private void Update()
    {
        if (Keyboard.current[Key.Escape].wasPressedThisFrame)
        {
           settingsPanel.SetActive(!settingsPanel.activeSelf); 
        }
    }

    private void ReturnToMenu()
    {
        gameManager.SetGameState(GameStates.Menu);
    }
}
