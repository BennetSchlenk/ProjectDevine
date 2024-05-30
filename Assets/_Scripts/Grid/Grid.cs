using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public bool ShowGizmos;
    
    [HideInInspector]
    public int2 GridSize;

    [Tooltip("uniform size of each grid node")]
    private GridNode[,] grid;

    private LevelDataSO LevelData;
    private LevelThemeSO LevelMeshes;

    private GridNode[,] levelDataGrid;
    //private int[,] levelDataObj;
    private float GridNodeRadius;
    private int gridNodesX;
    private int gridNodesY;

    public int MaxGridSize => gridNodesX * gridNodesY;

    private void Awake()
    {
        GridNodeRadius = GlobalData.GridNodeSize / 2;
    }

    public void SetLevelData(LevelDataSO levelDataSO, LevelThemeSO levelThemesSO)
    {
        LevelData = levelDataSO;
        LevelMeshes = levelThemesSO;
    }

    public void CreateGrid()
    {
        levelDataGrid = GridConversionUtility.ListToGrid(LevelData.Grid, LevelData.GridX, LevelData.GridY);
        GridSize = new int2(levelDataGrid.GetLength(0), levelDataGrid.GetLength(1));
        gridNodesX = GridSize.x;
        gridNodesY = GridSize.y;
        
        grid = new GridNode[gridNodesX, gridNodesY];
        Vector3 bottomLeft =
            transform.position - Vector3.right * GridSize.x - Vector3.forward * GridSize.y;

        for (int x = 0; x < gridNodesX; x++)
        {
            for (int y = 0; y < gridNodesY; y++)
            {
                Vector3 nodePos = bottomLeft + Vector3.right * (x * GlobalData.GridNodeSize + GridNodeRadius) +
                                  Vector3.forward * (y * GlobalData.GridNodeSize + GridNodeRadius);

                grid[x, y] = new GridNode(levelDataGrid[x, y].Walkable, levelDataGrid[x, y].Buildable,
                    LevelMeshes.Meshes[levelDataGrid[x, y].MeshIndex], levelDataGrid[x, y].MeshIndex, levelDataGrid[x, y].MeshYRotation,levelDataGrid[x, y].Spawn,
                    levelDataGrid[x, y].EnemyTarget,
                    levelDataGrid[x, y].Waypoint, nodePos, x, y);

                var go = Instantiate(LevelMeshes.Meshes[levelDataGrid[x, y].MeshIndex], nodePos, Quaternion.Euler(0f,levelDataGrid[x, y].MeshYRotation,0f), this.transform);
                grid[x, y].MeshObj = go;
                
                
                if (levelDataGrid[x, y].Spawn)
                {
                    go.AddComponent<WaypointsContainer>().WaypointsList = LevelData.LevelWaypoints.First(w => (w.NodePos.x == x && w.NodePos.y == y)).Waypoints;
                }
            }
        }
    }


    public GridNode NodeFromWorldPosition(Vector3 worldPos)
    {
        // Check if worldPos is outside the grid boundaries
        if (worldPos.x < -GridSize.x || worldPos.x > GridSize.x || worldPos.z < -GridSize.y || worldPos.z > GridSize.y)
        {
            return null;
        }

        float percentX = Mathf.InverseLerp(-GridSize.x, GridSize.x, worldPos.x);
        float percentY = Mathf.InverseLerp(-GridSize.y, GridSize.y, worldPos.z);

        int x = Mathf.Clamp(Mathf.FloorToInt(percentX * gridNodesX), 0, gridNodesX - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(percentY * gridNodesY), 0, gridNodesY - 1);

        return grid[x, y];
    }

    private void OnDrawGizmos()
    {
        if (!ShowGizmos) return;
        Gizmos.DrawWireCube(transform.position,
            new Vector3(GridSize.x * GlobalData.GridNodeSize, 0.1f, GridSize.y * GlobalData.GridNodeSize));


        if (grid == null) return;
        foreach (GridNode n in grid)
        {
            Gizmos.color = (n.Walkable) ? Color.white : Color.red;

            var pos = (transform.position + n.Position);
            var offset = (0.5f * GlobalData.GridNodeSize) * 0.9f;

            var topLeft = pos;
            topLeft.x -= offset;
            topLeft.z += offset;
            topLeft.y += 0.1f;

            var topRight = pos;
            topRight.x += offset;
            topRight.z += offset;
            topRight.y += 0.1f;

            var bottomLeft = pos;
            bottomLeft.x -= offset;
            bottomLeft.z -= offset;
            bottomLeft.y += 0.1f;

            var bottomRight = pos;
            bottomRight.x += offset;
            bottomRight.z -= offset;
            bottomRight.y += 0.1f;


            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(topLeft, bottomLeft);
            Gizmos.DrawLine(topRight, bottomRight);
        }
    }
}