using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainMenuUIHandler : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform parent;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject customGamePanel;
    
    [SerializeField] private List<Sprite> buttonsNormalSprites;
    [SerializeField] private List<Sprite> buttonsHoverSprites;

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
            if(i == 4) continue;
#endif
            var go = Instantiate(prefab, parent);
            go.name = names[i] + "_Button";
            Button button = go.GetComponent<Button>();
            var state = button.spriteState;
            state.highlightedSprite = buttonsHoverSprites[i];
            state.disabledSprite = buttonsNormalSprites[i];
            state.pressedSprite = buttonsNormalSprites[i];
            state.selectedSprite = buttonsNormalSprites[i];
            button.spriteState = state;
            Image image = go.GetComponent<Image>();
            image.sprite = buttonsNormalSprites[i];
            
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
            customGamePanel.SetActive(!customGamePanel.activeSelf);
        }

        if (button == names[2])
        {
            gameManager.SetGameState(GameStates.LevelEditor);
        }

        if (button == names[3])
        {
            settingsPanel.SetActive(true);
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
        if (settingsPanel.activeSelf)
            settingsPanel.SetActive(false);
    }

    public void CloseCustomGame()
    {
        if (customGamePanel.activeSelf)
            customGamePanel.SetActive(false);
    }
}