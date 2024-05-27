using System;
using System.Collections;
using System.Collections.Generic;
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


    public Button generateButton;
    public Button validateButton;
    public Button saveButton;
    public GameObject SelectedObj;
    public int SelectedObjIndex;
    public GridGenerator gridGen;
    public TMP_InputField xInput;
    public TMP_InputField zInput;
    public TMP_InputField levelNameInput;
    public GameObject GenerateGridUIPanel;
    public GameObject MeshSelectionUIPanel;
    public GameObject ValidateSaveUIPanel;
    public GameObject ButtonPrefab;
    public List<GameObject> PlaceableMeshes;


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
        generateButton.onClick.AddListener(GenerateOnClick);
        validateButton.onClick.AddListener(ValidateOnClick);
        saveButton.onClick.AddListener(SaveOnClick);

        for (int i = 0; i < PlaceableMeshes.Count; i++)
        {
            var go = Instantiate(ButtonPrefab, MeshSelectionUIPanel.transform);
            Button button = go.GetComponent<Button>();
            Image image = go.GetComponent<Image>();
            var tex = AssetPreview.GetAssetPreview(PlaceableMeshes[i]);
            Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, 100f, 100f), new Vector2(0.5f, 0.5f), 100.0f);
            image.sprite = sprite;
            var i1 = i;
            button.onClick.AddListener(() => SetSelectedMesh(PlaceableMeshes[i1]));
        }
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
        string path = $"Assets/Data/Levels/{levelNameInput.text}.asset";

#if UNITY_EDITOR
        var obj = ScriptableObject.CreateInstance<LevelDataSO>();

        obj.Grid = GridConversionUtility.GridToList(gridGen.grid);
        obj.GridObj = GridConversionUtility.GridToList(gridObjIndex);
        obj.GridX = gridGen.grid.GetLength(0);
        obj.GridY = gridGen.grid.GetLength(1);
        obj.LevelWaypoints = levelWaypoints;
        obj.LevelName = levelNameInput.text;

        UnityEditor.AssetDatabase.CreateAsset(obj, path);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
#endif
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
                    var neighbours = GetNeighbours(last);

                    for (int i = 0; i < neighbours.Count; i++)
                    {
                        if (neighbours[i].Walkable && !nodePath.Contains(neighbours[i]))
                        {
                            last = neighbours[i];
                            nodePath.Add(neighbours[i]);
                            if (neighbours[i].Waypoint)
                            {
                                waypoints.Add(new Vector3(neighbours[i].Position.x, -0.5f, neighbours[i].Position.z));
                            }

                            if (neighbours[i].EnemyTarget)
                            {
                                targetFound = true;
                            }

                            break;
                        }
                    }
                }
                levelWaypoints.Add(new WaypointData()
                {
                    NodePos = new int2(nodePath[0].GridX,nodePath[0].GridY),
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
        if (obj == PlaceableMeshes[0])
        {
            return 0;
        }

        if (obj == PlaceableMeshes[1])
        {
            return 1;
        }

        if (obj == PlaceableMeshes[2])
        {
            return 2;
        }

        if (obj == PlaceableMeshes[3])
        {
            return 3;
        }

        if (obj == PlaceableMeshes[4])
        {
            return 4;
        }

        if (obj == PlaceableMeshes[5])
        {
            return 5;
        }

        if (obj == PlaceableMeshes[6])
        {
            return 6;
        }
        if (obj == PlaceableMeshes[7])
        {
            return 7;
        }
        if (obj == PlaceableMeshes[8])
        {
            return 8;
        }
        if (obj == PlaceableMeshes[9])
        {
            return 9;
        }

        return 2000;
    }

    public List<GridNode> GetNeighbours(GridNode node)
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
                break;
            case RotationDirections.Right:
                return 90f;
                break;
            case RotationDirections.Down:
                return 180f;
                break;
            case RotationDirections.Left:
                return 270f;
                break;
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
                Destroy(node.MeshObj);
                node.MeshObj = null;
                node.MeshIndex = 2000;
                var go = Instantiate(SelectedObj, node.Position, Quaternion.identity, gridGen.transform);
                go.transform.rotation = Quaternion.Euler(0f, GetYRotationFromBuildDir(), 0f);
                node.MeshYRotation = (int)GetYRotationFromBuildDir();
                node.MeshObj = go;
                node.MeshIndex = SelectedObjIndex;
                validated = false;
                if (SelectedObj == PlaceableMeshes[6] || SelectedObj == PlaceableMeshes[7] || SelectedObj == PlaceableMeshes[8] ||SelectedObj == PlaceableMeshes[9])
                {
                    node.EnemyTarget = true;
                    node.Walkable = true;
                    node.Buildable = false;
                    node.Waypoint = true;
                }
                else if (SelectedObj == PlaceableMeshes[5])
                {
                    node.Spawn = true;
                    node.Walkable = true;
                    node.Buildable = false;
                    node.Waypoint = true;
                }
                else if (SelectedObj != PlaceableMeshes[0])
                {
                    node.Walkable = true;
                    node.Buildable = false;
                    if (SelectedObj == PlaceableMeshes[4])
                    {
                        node.Waypoint = true;
                    }
                }

                if (SelectedObj == PlaceableMeshes[0])
                {
                    node.Walkable = false;
                    node.Buildable = true;
                    node.Waypoint = false;
                }
            }
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
        gridGenerated = true;
    }
}