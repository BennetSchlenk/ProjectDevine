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
    [SerializeField]
    private Button backButton;
    [SerializeField]
    private Button settingsButton;

    private GameManager gameManager;
    #region Unity Callbacks
        
    private void Awake()
    {
        settingsPanel.SetActive(false);
        menuButton.onClick.AddListener(ReturnToMenu);
        settingsButton.onClick.AddListener(ToggleSettings);
        backButton.onClick.AddListener(ToggleSettings);
    }


    private void Start()
    {
        gameManager = ServiceLocator.Instance.GetService<GameManager>();
    }

    #endregion

    private void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
        if (settingsPanel.activeSelf)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    private void ReturnToMenu()
    {
        Time.timeScale = 1f;
        gameManager.SetGameState(GameStates.Menu);
    }
}
