using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum RotationDirections
{
    Up,
    Right,
    Down,
    Left
}

public class LevelEditorManager : MonoBehaviour
{
    private RotationDirections buildDir;
    private Camera camMain;
    private bool gridGenerated;
    private GameObject previewObj;
    private bool validated;
    private GameObject[,] gridObj;
    private int[,] gridObjIndex;
    private List<WaypointData> levelWaypoints;
    private GameManager gameManager;


    public Button generateButton;
    public Button validateButton;
    public Button saveButton;
    public Button backButton;
    public Button backButton2;
    [HideInInspector]
    public GameObject SelectedObj;
    public int SelectedObjIndex;
    public GridGenerator gridGen;
    public TMP_InputField xInput;
    public TMP_InputField zInput;
    public TMP_InputField levelNameInput;
    public GameObject GenerateGridUIPanel;
    public GameObject MeshSelectionUIPanel;
    public GameObject ValidateSaveUIPanel;
    public GameObject ControlUIPanel;
    public GameObject BackUIPanel;
    public GameObject ButtonPrefab;
    public LevelThemeSO ThemeMeshes;
    public List<Sprite> ButtonIcons;


    private void Awake()
    {
        levelWaypoints = new List<WaypointData>();
        buildDir = RotationDirections.Up;
        SelectedObj = null;
        gridGenerated = false;
        validated = false;
        validateButton.GetComponent<Image>().color = Color.red;
        camMain = Camera.main;
        GenerateGridUIPanel.SetActive(true);
        MeshSelectionUIPanel.SetActive(false);
        ValidateSaveUIPanel.SetActive(false);
        ControlUIPanel.SetActive(false);
        BackUIPanel.SetActive(false);
        backButton.onClick.AddListener(ReturnToMenu);
        backButton2.onClick.AddListener(ReturnToMenu);
        generateButton.onClick.AddListener(GenerateOnClick);
        validateButton.onClick.AddListener(ValidateOnClick);
        saveButton.onClick.AddListener(SaveOnClick);

        for (int i = 0; i < ThemeMeshes.Meshes.Count; i++)
        {
            if (i == 2 || i == 3) continue;

            var go = Instantiate(ButtonPrefab, MeshSelectionUIPanel.transform);
            Button button = go.GetComponent<Button>();
            Image image = go.GetComponent<Image>();
            image.sprite = ButtonIcons[i];

            var i1 = i;
            button.onClick.AddListener(() => SetSelectedMesh(ThemeMeshes.Meshes[i1]));
        }
    }



    private void Start()
    {
        gameManager = ServiceLocator.Instance.GetService<GameManager>();
    }

    private void Update()
    {
        ChangeBuildingDirection();
        DisplayPreviewObj();
        RemovePreviewAndSelectedObj();
        PlaceNodeMeshObj();
    }
    

    private void SaveOnClick()
    {
        if (!validated) return;
        string path = $"Assets/Resources/Levels/{levelNameInput.text}.asset";
        
        var obj = new LevelSaveData();

        obj.Grid = GridConversionUtility.GridToList(gridGen.grid);
        obj.GridObj = GridConversionUtility.GridToList(gridObjIndex);
        obj.GridX = gridGen.grid.GetLength(0);
        obj.GridY = gridGen.grid.GetLength(1);
        obj.LevelWaypoints = levelWaypoints;
        obj.LevelName = levelNameInput.text;

        var levelData =JsonUtility.ToJson(obj);

        
         #if UNITY_EDITOR
         var folderPath =  Path.Combine(Application.dataPath, "Resources/LevelSaves");
        
         Debug.Log("Path: " + folderPath);
        
         if (Directory.Exists(folderPath))
         {
             File.WriteAllText(folderPath + $"/{levelNameInput.text}.json", levelData);
         }
         else
         {
             Directory.CreateDirectory(folderPath);
             File.WriteAllText(folderPath + $"/{levelNameInput.text}.json", levelData);
         }
         AssetDatabase.Refresh();
        #else
        var folderPath =  Path.Combine(Application.persistentDataPath, "LevelSaves");

        if (Directory.Exists(folderPath))
        {
            File.WriteAllText(folderPath + $"/{levelNameInput.text}.json", levelData);
        }
        else
        {
            Directory.CreateDirectory(folderPath);
            File.WriteAllText(folderPath + $"/{levelNameInput.text}.json", levelData);
        }
        #endif
        gameManager.SetGameState(GameStates.Menu);
    }

    private void ValidateOnClick()
    {
        levelWaypoints.Clear();
        int targets = 0;
        int spawns = 0;
        List<GridNode> spawnNodes = new List<GridNode>();
        gridObj = new GameObject[gridGen.grid.GetLength(0), gridGen.grid.GetLength(1)];
        gridObjIndex = new int[gridGen.grid.GetLength(0), gridGen.grid.GetLength(1)];

        for (int x = 0; x < gridGen.grid.GetLength(0); x++)
        {
            for (int y = 0; y < gridGen.grid.GetLength(1); y++)
            {
                gridObj[x, y] = gridGen.grid[x, y].MeshObj;
                gridObjIndex[x, y] = gridGen.grid[x, y].MeshIndex;
                if (gridGen.grid[x, y].Spawn)
                {
                    spawns++;
                    spawnNodes.Add(gridGen.grid[x, y]);
                }
                else if (gridGen.grid[x, y].EnemyTarget)
                {
                    targets++;
                }
            }
        }
        
        if (targets == 1 && spawns >= 1)
        {
            List<Vector3> waypoints = new List<Vector3>();
            List<GridNode> nodePath = new List<GridNode>();
            for (int n = 0; n < spawnNodes.Count; n++)
            {
                bool targetFound = false;
                GridNode last = spawnNodes[n];
                waypoints.Clear();
                nodePath.Clear();
                nodePath.Add(spawnNodes[n]);
                waypoints.Add(new Vector3(spawnNodes[n].Position.x, -0.5f, spawnNodes[n].Position.z));

                while (!targetFound)
                {
                    var neighbours = GetNonDiagonalNeighbours(last);

                    bool progressMade = false;
                    for (int i = 0; i < neighbours.Count; i++)
                    {
                        if (!nodePath.Contains(neighbours[i]) && neighbours[i].Walkable)
                        {
                            last = neighbours[i];
                            nodePath.Add(neighbours[i]);
                            progressMade = true; // Mark that progress is made
                            if (neighbours[i].Waypoint)
                            {
                                waypoints.Add(new Vector3(neighbours[i].Position.x, -0.5f, neighbours[i].Position.z));
                            }

                            if (neighbours[i].EnemyTarget)
                            {
                                targetFound = true;
                                //Debug.Log("Path from Node: " + spawnNodes[n].GridX + " / " + spawnNodes[n].GridY +" was found!");
                            }
                        }
                    }

                    if (progressMade) continue; // If no progress is made, break out of the loop
                    Debug.LogWarning("No progress made, breaking out to avoid infinite loop.");
                    return;
                }

                levelWaypoints.Add(new WaypointData()
                {
                    NodePos = new int2(nodePath[0].GridX, nodePath[0].GridY),
                    Waypoints = waypoints.ToList()
                });
                if (spawnNodes[n].MeshObj.TryGetComponent<WaypointsContainer>(out var waypointsComponent))
                {
                    waypointsComponent.WaypointsList = waypoints;
                }
                else
                {
                    spawnNodes[n].MeshObj.AddComponent<WaypointsContainer>().WaypointsList = waypoints;
                }
            }

            validated = true;
            validateButton.GetComponent<Image>().color = Color.green;
        }
    }

    private int MeshObjToInt(GameObject obj)
    {
        if (obj == ThemeMeshes.Meshes[0])
        {
            return 0;
        }

        if (obj == ThemeMeshes.Meshes[1])
        {
            return 1;
        }

        if (obj == ThemeMeshes.Meshes[2])
        {
            return 2;
        }

        if (obj == ThemeMeshes.Meshes[3])
        {
            return 3;
        }

        if (obj == ThemeMeshes.Meshes[4])
        {
            return 4;
        }

        if (obj == ThemeMeshes.Meshes[5])
        {
            return 5;
        }

        if (obj == ThemeMeshes.Meshes[6])
        {
            return 6;
        }

        if (obj == ThemeMeshes.Meshes[7])
        {
            return 7;
        }

        if (obj == ThemeMeshes.Meshes[8])
        {
            return 8;
        }

        if (obj == ThemeMeshes.Meshes[9])
        {
            return 9;
        }

        return 2000;
    }

    public List<GridNode> GetNonDiagonalNeighbours(GridNode node)
    {
        List<GridNode> neighbours = new List<GridNode>();

        // Define the relative positions of the four neighbors (up, down, left, right)
        int[,] directions = new int[,] { { 0, -1 }, { 0, 1 }, { -1, 0 }, { 1, 0 } };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int checkX = node.GridX + directions[i, 0];
            int checkY = node.GridY + directions[i, 1];

            if (checkX >= 0 && checkX < gridGen.grid.GetLength(0) && checkY >= 0 && checkY < gridGen.grid.GetLength(1))
            {
                neighbours.Add(gridGen.grid[checkX, checkY]);
            }
        }

        return neighbours;
    }

    public List<GridNode> GetAllNeighbours(GridNode node)
    {
        List<GridNode> neighbours = new List<GridNode>();

        // Define the relative positions of the four neighbors (up, down, left, right)
        int[,] directions = new int[,]
            { { 0, -1 }, { 1, -1 }, { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 }, { -1, 0 }, { -1, -1 } };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int checkX = node.GridX + directions[i, 0];
            int checkY = node.GridY + directions[i, 1];

            if (checkX >= 0 && checkX < gridGen.grid.GetLength(0) && checkY >= 0 && checkY < gridGen.grid.GetLength(1))
            {
                neighbours.Add(gridGen.grid[checkX, checkY]);
            }
        }

        return neighbours;
    }


    private void ChangeBuildingDirection()
    {
        if (!Keyboard.current[Key.R].wasPressedThisFrame) return;
        if ((int)buildDir >= 3)
        {
            buildDir = 0;
        }
        else
        {
            buildDir++;
        }

        if (previewObj != null)
        {
            previewObj.transform.rotation = Quaternion.Euler(0f, GetYRotationFromBuildDir(), 0f);
        }
    }

    private void RemovePreviewAndSelectedObj()
    {
        if (!Mouse.current.rightButton.wasPressedThisFrame) return;
        SelectedObj = null;
        Destroy(previewObj);
        previewObj = null;
    }

    private float GetYRotationFromBuildDir()
    {
        switch (buildDir)
        {
            case RotationDirections.Up:
                return 0f;
            case RotationDirections.Right:
                return 90f;
            case RotationDirections.Down:
                return 180f;
            case RotationDirections.Left:
                return 270f;
            default:
                return 0f;
        }
    }


    private void DisplayPreviewObj()
    {
        if (GetRaycastHitPos(out var hitPos) && previewObj != null)
        {
            hitPos.y = 0.6f;
            previewObj.transform.position = hitPos;
        }
    }

    private void PlaceNodeMeshObj()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame || !gridGenerated) return;
        if (GetRaycastHitPos(out var hitPos) && !EventSystem.current.IsPointerOverGameObject())
        {
            var node = gridGen.NodeFromWorldPosition(hitPos);

            
            if (node != null && node.MeshObj != SelectedObj && SelectedObj != null)
            {
                // Debug.Log("Mesh Index: " + SelectedObjIndex);
                // //Check if allowed to place (nor more then 2 neighbouring path tiles)
                // if (SelectedObjIndex is >= 1 and <= 5)
                // {
                //     var neighbours = GetAllNeighbours(node);
                //     int pathNeighbours = 0;
                //
                //     for (int i = 0; i < neighbours.Count; i++)
                //     {
                //         if (neighbours[i].MeshIndex is >= 1 and <= 5)
                //         {
                //             Debug.Log("Neighbour: " + neighbours[i].MeshIndex);
                //             pathNeighbours++;
                //         }
                //     }
                //     Debug.Log("path Neighbours: " + pathNeighbours);
                //     if(pathNeighbours > 2) return;
                // }
                
                
                
                //Reset target 9 tiles area of unbuildable to buildable
                if (node.MeshIndex is >= 6 and <= 9)
                {
                    SetNeighboursBuildableState(node, true);
                }

                //Remove validation
                if (validated)
                {
                    validated = false;
                    validateButton.GetComponent<Image>().color = Color.red;
                }
                //Clean slate node
                node.EnemyTarget = false;
                node.Spawn = false;
                node.Waypoint = false;
                node.Buildable = true;
                node.Walkable = false;
                node.MeshIndex = 2000;
                Destroy(node.MeshObj);
                node.MeshObj = null;
                node.MeshYRotation = 0;


                //Instantiate new tile and set tile state
                var go = Instantiate(SelectedObj, node.Position, Quaternion.identity, gridGen.transform);
                go.transform.rotation = Quaternion.Euler(0f, GetYRotationFromBuildDir(), 0f);
                node.MeshYRotation = (int)GetYRotationFromBuildDir();
                node.MeshObj = go;
                node.MeshIndex = SelectedObjIndex;

                if (SelectedObj == ThemeMeshes.Meshes[6] || SelectedObj == ThemeMeshes.Meshes[7] ||
                    SelectedObj == ThemeMeshes.Meshes[8] || SelectedObj == ThemeMeshes.Meshes[9])
                {
                    //Set target 9 tiles area to unbuildable
                    SetNeighboursBuildableState(node, false);

                    node.EnemyTarget = true;
                    node.Walkable = true;
                    node.Buildable = false;
                    node.Waypoint = true;
                }
                else if (SelectedObj == ThemeMeshes.Meshes[5])
                {
                    node.Spawn = true;
                    node.Walkable = true;
                    node.Buildable = false;
                    node.Waypoint = true;
                }
                else if (SelectedObj != ThemeMeshes.Meshes[0])
                {
                    node.Walkable = true;
                    node.Buildable = false;
                    if (SelectedObj == ThemeMeshes.Meshes[4])
                    {
                        node.Waypoint = true;
                    }
                }

                if (SelectedObj == ThemeMeshes.Meshes[0])
                {
                    node.Walkable = false;
                    node.Buildable = true;
                    node.Waypoint = false;
                }
            }
        }
    }

    private void SetNeighboursBuildableState(GridNode node, bool state)
    {
        var neighbours = GetAllNeighbours(node);

        for (int i = 0; i < neighbours.Count; i++)
        {
            neighbours[i].Buildable = state;
        }
    }

    private void ResetBuildDirection()
    {
        buildDir = RotationDirections.Up;
        if (previewObj != null)
        {
            previewObj.transform.rotation = Quaternion.Euler(0f, GetYRotationFromBuildDir(), 0f);
        }
    }

    private bool GetRaycastHitPos(out Vector3 hitPos)
    {
        hitPos = Vector3.zero;
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = camMain.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (plane.Raycast(ray, out float distance))
        {
            hitPos = ray.GetPoint(distance);
            return true;
        }

        return false;
    }

    private void SetSelectedMesh(GameObject obj)
    {
        SelectedObj = obj;
        SelectedObjIndex = MeshObjToInt(obj);
        ResetBuildDirection();

        if (previewObj != null)
        {
            Destroy(previewObj);
        }

        if (GetRaycastHitPos(out var hitPos))
        {
            previewObj = Instantiate(obj, hitPos, quaternion.identity);
        }
    }

    private void GenerateOnClick()
    {
        gridGen.CreateGrid(int.Parse(xInput.text), int.Parse(zInput.text));
        GenerateGridUIPanel.SetActive(false);
        MeshSelectionUIPanel.SetActive(true);
        ValidateSaveUIPanel.SetActive(true);
        ControlUIPanel.SetActive(true);
        BackUIPanel.SetActive(true);
        gridGenerated = true;
    }
    
    private void ReturnToMenu()
    {
        gameManager.SetGameState(GameStates.Menu);
    }
}