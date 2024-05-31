using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameStates
{
    Menu,
    LevelEditor,
    Game,
    GameOver
}

[RequireComponent(typeof(DontDestroyOnLoad))]
public class GameManager : MonoBehaviour
{
    private Grid grid;
    private LevelDataSO LevelData;
    private LevelThemeSO LevelTheme;

    private GameStates gameState;

    #region Unity Callbacks

    private void Awake()
    {
        ServiceLocator.Instance.RegisterService(this);
        if (LevelData == null || LevelTheme == null)
        {
            LevelData = Resources.Load(GlobalData.DefaultLevel) as LevelDataSO;
            LevelTheme = Resources.Load(GlobalData.DefaultTheme) as LevelThemeSO;
        }
    }

    private void Start()
    {
    }

    #endregion

    private void InitializeGame()
    {
        grid = FindObjectOfType<Grid>();
        grid.SetLevelData(LevelData, LevelTheme);
        grid.CreateGrid();
    }

    public void SetLevelThemeAndData(LevelDataSO levelData,LevelThemeSO levelTheme)
    {
        LevelData = levelData;
        LevelTheme = levelTheme;
    }

    public void SetGameState(GameStates state)
    {
        Debug.Log($"<color=#FFFFFF>GAMESTATE: </color><color=#449E48> {state.ToString().ToUpper()}</color>");
        switch (state)
        {
            case GameStates.Menu:
                StartCoroutine(LoadYourAsyncScene("MainMenu"));
                gameState = state;
                break;
            case GameStates.LevelEditor:
                StartCoroutine(LoadYourAsyncScene("LevelEditor"));
                gameState = state;
                break;
            case GameStates.Game:
                StartCoroutine(LoadYourAsyncScene("GameScene"));
                gameState = state;
                break;
            case GameStates.GameOver:
                StartCoroutine(LoadYourAsyncScene("GameOver"));
                gameState = state;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private IEnumerator LoadYourAsyncScene(string sceneName)
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName,LoadSceneMode.Single);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            
            yield return null;
        }

        GameStateInitialization();
    }

    private void GameStateInitialization()
    {
        switch (gameState)
        {
            case GameStates.Menu:
                break;
            case GameStates.LevelEditor:
                break;
            case GameStates.Game:
                InitializeGame();
                break;
            case GameStates.GameOver:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #region ContextMenu

    [ContextMenu("GOTO Menu")]
    private void GameStateMainMenu()
    {
        SetGameState(GameStates.Menu);
    }

    [ContextMenu("GOTO Level Editor")]
    private void GameStateLevelEditor()
    {
        SetGameState(GameStates.LevelEditor);
    }

    [ContextMenu("GOTO Game")]
    private void GameStateGame()
    {
        SetGameState(GameStates.Game);
    }

    [ContextMenu("GOTO Game Over")]
    private void GameStateGameOver()
    {
        SetGameState(GameStates.GameOver);
    }

    #endregion
}