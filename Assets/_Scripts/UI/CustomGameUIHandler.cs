using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomGameUIHandler : MonoBehaviour
{
    private GameManager gameManger;
    [SerializeField] private GameObject levelLayoutPanel;
    //[SerializeField] private GameObject levelThemePanel;
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
        //GenerateLevelThemeUI();
        GenerateLevelLayoutUI();
    }

    private void OnDisable()
    {
        int numLevels = levelLayoutPanel.transform.childCount;
        for (int i = 0; i < numLevels; i++)
        {
            Destroy(levelLayoutPanel.transform.GetChild(i).gameObject);
        }

        // int numThemes = levelThemePanel.transform.childCount;
        // for (int i = 0; i < numThemes; i++)
        // {
        //     Destroy(levelThemePanel.transform.GetChild(i).gameObject);
        // }
    }

    #endregion

    // private void GenerateLevelThemeUI()
    // {
    //     var levelThemes = Resources.LoadAll<LevelThemeSO>("Themes/");
    //     ToggleGroup group = levelThemePanel.transform.GetComponent<ToggleGroup>();
    //     for (int i = 0; i < levelThemes.Length; i++)
    //     {
    //         var go = Instantiate(levelThemePrefab, levelThemePanel.transform);
    //         RefHolder holder = go.GetComponent<RefHolder>();
    //         holder.scriptableData = levelThemes[i];
    //         TMP_Text text = go.GetComponentInChildren<TMP_Text>();
    //         text.text = levelThemes[i].name;
    //         Toggle toggle = go.GetComponent<Toggle>();
    //         toggle.group = group;
    //         if (i == 0)
    //         {
    //             toggle.isOn = true;
    //         }
    //     }
    // }

    private void GenerateLevelLayoutUI()
    {
        var folderPath = Path.Combine(Application.persistentDataPath, "LevelSaves");
        List<string> paths = GetJsonFilePaths(folderPath).ToList();
        
        var localFolderPath = Path.Combine(Application.dataPath, "Resources/LevelSaves");
        var resourcesLevels = Resources.LoadAll<TextAsset>("LevelSaves/");
        for (int i = 0; i < resourcesLevels.Length; i++)
        {
            paths.Add(localFolderPath +"/"+ resourcesLevels[i].name +".json");
        }
        
        //paths.Add(Path.Combine(Application.dataPath, $"Resources/LevelSaves/{GlobalData.DefaultLevel}.json"));

        ToggleGroup group = levelLayoutPanel.transform.GetComponent<ToggleGroup>();
        for (int i = 0; i < paths.Count; i++)
        {
            var go = Instantiate(levelThemePrefab, levelLayoutPanel.transform);
            RefHolder holder = go.GetComponent<RefHolder>();
            holder.Path = paths[i];
            TMP_Text text = go.GetComponentInChildren<TMP_Text>();
            text.text = Path.GetFileName(paths[i]);
            Toggle toggle = go.GetComponent<Toggle>();
            toggle.group = group;
            if (i == 0)
            {
                toggle.isOn = true;
            }
        }
    }

    string[] GetJsonFilePaths(string folderPath)
    {
        // Check if the folder exists
        if (Directory.Exists(folderPath))
        {
            // Get all .json files in the folder and its subfolders
            string[] jsonFiles = Directory.GetFiles(folderPath, "*.json", SearchOption.AllDirectories);
            return jsonFiles;
        }
        else
        {
            Debug.LogError("Folder path does not exist: " + folderPath);
            return new string[0];
        }
    }


    public void SetCustomLevelAndStart()
    {
        var group = levelLayoutPanel.GetComponent<ToggleGroup>();

        var x = group.GetComponentsInChildren<Toggle>().Where(t => t.isOn).ToList();
        var levelPath = x[0].GetComponent<RefHolder>().Path;
        Debug.Log("ACTIVE LEVEL PATH: " + levelPath);
        Debug.Log("ACTIVE LEVEL: " + Path.GetFileName(levelPath));
        
        var folderPath = Path.GetDirectoryName(levelPath); //Path.Combine(Application.persistentDataPath, "LevelSaves");

        LevelSaveData selectedLevel = new LevelSaveData();
        
        if (Directory.Exists(folderPath))
        {
            string jsonFile = File.ReadAllText(levelPath);
            selectedLevel = JsonUtility.FromJson<LevelSaveData>(jsonFile);
        }
        else
        {
            Debug.LogError("Folder path does not exist: " + folderPath);
        }

        // group = levelThemePanel.GetComponent<ToggleGroup>();
        //
        // x = group.GetComponentsInChildren<Toggle>().Where(t => t.isOn).ToList();
        // var selectedTheme = x[0].GetComponent<RefHolder>().scriptableData as LevelThemeSO;
        // Debug.Log("ACTIVE Theme: " + selectedTheme.name);
        var selectedTheme = Resources.Load("Themes/" + GlobalData.DefaultTheme) as LevelThemeSO;

        gameManger.SetLevelThemeAndData(selectedLevel, selectedTheme);

        gameManger.SetGameState(GameStates.Game);
    }
}