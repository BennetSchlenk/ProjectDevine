using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomGameUIHandler : MonoBehaviour
{
    private GameManager gameManger;
    [SerializeField] private GameObject levelLayoutPanel;
    [SerializeField] private GameObject levelThemePanel;
    [SerializeField] private GameObject levelThemePrefab;

    #region Unity Callbacks

    private void Awake()
    {
    }

    private void Start()
    {
        gameManger = ServiceLocator.Instance.GetService<GameManager>();
    }

    private void OnEnable()
    {
        GenerateLevelThemeUI();
        GenerateLevelLayoutUI();
    }

    private void OnDisable()
    {
        int numLevels = levelLayoutPanel.transform.childCount;
        for (int i = 0; i < numLevels; i++)
        {
            Destroy(levelLayoutPanel.transform.GetChild(i).gameObject);
        }
        
        int numThemes = levelThemePanel.transform.childCount;
        for (int i = 0; i < numThemes; i++)
        {
            Destroy(levelThemePanel.transform.GetChild(i).gameObject);
        }
    }

    #endregion

    private void GenerateLevelThemeUI()
    {
        var levelThemes = Resources.LoadAll<LevelThemeSO>("Themes/");
        ToggleGroup group = levelThemePanel.transform.GetComponent<ToggleGroup>();
        for (int i = 0; i < levelThemes.Length; i++)
        {
            var go = Instantiate(levelThemePrefab, levelThemePanel.transform);
            ScriptableObjectRefHolder holder = go.GetComponent<ScriptableObjectRefHolder>();
            holder.scriptable = levelThemes[i];
            TMP_Text text = go.GetComponentInChildren<TMP_Text>();
            text.text = levelThemes[i].name;
            Toggle toggle = go.GetComponent<Toggle>();
            toggle.group = group;
            if (i == 0)
            {
                toggle.isOn = true;
            }
        }
    }

    private void GenerateLevelLayoutUI()
    {
        var levelData = Resources.LoadAll<LevelDataSO>("Levels/");
        ToggleGroup group = levelLayoutPanel.transform.GetComponent<ToggleGroup>();
        for (int i = 0; i < levelData.Length; i++)
        {
            var go = Instantiate(levelThemePrefab, levelLayoutPanel.transform);
            ScriptableObjectRefHolder holder = go.GetComponent<ScriptableObjectRefHolder>();
            holder.scriptable = levelData[i];
            TMP_Text text = go.GetComponentInChildren<TMP_Text>();
            text.text = levelData[i].name;
            Toggle toggle = go.GetComponent<Toggle>();
            toggle.group = group;
            if (i == 0)
            {
                toggle.isOn = true;
            }
        }
    }

    public void SetCustomLevelAndStart()
    {
       var group = levelLayoutPanel.GetComponent<ToggleGroup>();
        
        var x =  group.GetComponentsInChildren<Toggle>().Where(t => t.isOn).ToList();
        var selectedLevel = x[0].GetComponent<ScriptableObjectRefHolder>().scriptable as LevelDataSO;
        Debug.Log("ACTIVE LEVEL: " + selectedLevel.name);
        
        group = levelThemePanel.GetComponent<ToggleGroup>();
        
        x =  group.GetComponentsInChildren<Toggle>().Where(t => t.isOn).ToList();
        var selectedTheme = x[0].GetComponent<ScriptableObjectRefHolder>().scriptable as LevelThemeSO;
        Debug.Log("ACTIVE Theme: " + selectedTheme.name);
        
        gameManger.SetLevelThemeAndData(selectedLevel,selectedTheme);
        
        gameManger.SetGameState(GameStates.Game);
    }
}