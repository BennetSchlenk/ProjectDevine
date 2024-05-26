using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public bool ShowGizmos;
    public GameObject DefaultNodeObj;

    private int2 GridSize;
    [HideInInspector] public GridNode[,] grid;

    private float GridNodeRadius;
    private int gridNodesX;
    private int gridNodesY;

    public int MaxGridSize => gridNodesX * gridNodesY;

    private void Awake()
    {
        GridNodeRadius = GlobalData.GridNodeSize / 2;
    }

    public void CreateGrid(int xSize, int ySize)
    {
        GridSize = new int2(xSize, ySize);
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

                bool walkable = false;
                var go = Instantiate(DefaultNodeObj, nodePos, quaternion.identity, this.transform);
                grid[x, y] = new GridNode(walkable, true, go, 0, 0,false ,false, false, nodePos, x, y);
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