using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorUI : MonoBehaviour
{

    public Button generateButton;
    public Button validateButton;
    public Button saveButton;
    public Button backButton;
    public Button backButton2;
    public TMP_InputField xInput;
    public TMP_InputField zInput;
    public TMP_InputField levelNameInput;
    public GameObject GenerateGridUIPanel;
    public GameObject MeshSelectionUIPanel;
    public GameObject ValidateSaveUIPanel;
    public GameObject ControlUIPanel;
    public GameObject BackUIPanel;
    public GameObject ButtonPrefab;
    public List<Sprite> ButtonIcons;
    public LevelEditorManager levelEditorManager;
    public GridGenerator gridGen;
    
    private GameManager gameManager;
    #region Unity Callbacks
        
    private void Awake()
    {
        validateButton.GetComponent<Image>().color = Color.red;
        GenerateGridUIPanel.SetActive(true);
        MeshSelectionUIPanel.SetActive(false);
        ValidateSaveUIPanel.SetActive(false);
        ControlUIPanel.SetActive(false);
        BackUIPanel.SetActive(false);
        backButton.onClick.AddListener(ReturnToMenu);
        backButton2.onClick.AddListener(ReturnToMenu);
        generateButton.onClick.AddListener(GenerateOnClick);
        validateButton.onClick.AddListener(levelEditorManager.ValidateOnClick);
        saveButton.onClick.AddListener(TriggerLevelSave);
        
        for (int i = 0; i < ButtonIcons.Count; i++)
        {
            if (i == 2 || i == 3) continue;

            var go = Instantiate(ButtonPrefab, MeshSelectionUIPanel.transform);
            Button button = go.GetComponent<Button>();
            Image image = go.GetComponent<Image>();
            image.sprite = ButtonIcons[i];

            var i1 = i;
            button.onClick.AddListener(() => levelEditorManager.SetSelectedMesh(levelEditorManager.ThemeMeshes.Meshes[i1]));
        }
    }

    private void Start()
    {
        gameManager = ServiceLocator.Instance.GetService<GameManager>();
    }

    #endregion

    private void GenerateOnClick()
    {
        gridGen.CreateGrid(int.Parse(xInput.text), int.Parse(zInput.text));
        GenerateGridUIPanel.SetActive(false);
        MeshSelectionUIPanel.SetActive(true);
        ValidateSaveUIPanel.SetActive(true);
        ControlUIPanel.SetActive(true);
        BackUIPanel.SetActive(true);
        levelEditorManager.SetGridGenerated();
    }
    public void ReturnToMenu()
    {
        gameManager.SetGameState(GameStates.Menu);
    }

    public void SetValidateButtonSate(bool validated)
    {
        if (validated)
        {
            validateButton.GetComponent<Image>().color = Color.green;
        }
        else
        {
            validateButton.GetComponent<Image>().color = Color.red;
        }
    }

    private void TriggerLevelSave()
    {
        levelEditorManager.SaveOnClick(levelNameInput.text);
    }
}
