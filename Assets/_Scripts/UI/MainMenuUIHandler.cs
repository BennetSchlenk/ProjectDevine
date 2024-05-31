using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuUIHandler : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform parent;
    [SerializeField] private GameObject SettingsPanel;
    [SerializeField] private GameObject CustomGamePanel;

    private GameManager gameManager;
    private string[] names;


    #region Unity Callbacks

    private void Awake()
    {
    }

    private void Start()
    {
        CloseSettings();
        names = new[] { "Start Game", "Custom Game", "Level Editor", "Options", "Exit" };
        gameManager = ServiceLocator.Instance.GetService<GameManager>();

        for (int i = 0; i < 5; i++)
        {
#if UNITY_WEBGL   
            if(i == 1) continue;
#endif
            var go = Instantiate(prefab, parent);
            go.name = names[i] + "_Button";
            TMP_Text textComp = go.GetComponentInChildren<TMP_Text>();
            textComp.gameObject.name = names[0] + "_Text";
            textComp.text = names[i];
            textComp.fontSize = 35;
            textComp.fontStyle = FontStyles.Bold;
            Button button = go.GetComponent<Button>();
            var i1 = i;
            button.onClick.AddListener(() => { PerformAction(names[i1]); });
        }
    }

    #endregion

    private void PerformAction(string button)
    {
        CloseSettings();
        CloseCustomGame();
        
        if (button == names[0])
        {
            gameManager.SetGameState(GameStates.Game);
        }

        if (button == names[1])
        {
            CustomGamePanel.SetActive(!CustomGamePanel.activeSelf);
        }

        if (button == names[2])
        {
            gameManager.SetGameState(GameStates.LevelEditor);
        }

        if (button == names[3])
        {
            SettingsPanel.SetActive(true);
        }

        if (button == names[4])
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    public void CloseSettings()
    {
        if (SettingsPanel.activeSelf)
            SettingsPanel.SetActive(false);
    }

    public void CloseCustomGame()
    {
        if (CustomGamePanel.activeSelf)
            CustomGamePanel.SetActive(false);
    }
}