using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverUIHandler : MonoBehaviour
{
    private GameManager gameManager;
    #region Unity Callbacks
        
    private void Awake()
    {
        
    }

    private void Start()
    {
        gameManager = ServiceLocator.Instance.GetService<GameManager>();
    }

    #endregion

    public void RetryLevel()
    {
        gameManager.SetGameState(GameStates.Game);
    }

    public void GoToMenu()
    {
        gameManager.SetGameState(GameStates.Menu);
    }
}
