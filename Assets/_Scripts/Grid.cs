using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public bool ShowGizmos;
    
    public LayerMask unwalkableMask;
    [Tooltip("Amout of nodes on the x and z axis")]
    public int2 GridSize;
    [Tooltip("uniform size of each grid node")]
    public float GridNodeSize;
    private GridNode[,] grid;

    private float GridNodeRadius;
    private int gridNodesX;
    private int gridNodesY;

    public int MaxGridSize => gridNodesX * gridNodesY;

    private void Awake()
    {
        GridNodeRadius = GridNodeSize / 2;
        gridNodesX = GridSize.x;
        gridNodesY = GridSize.y;
        Debug.Log("X: " + gridNodesX + " Y: " + gridNodesY);
        CreateGrid();
    }

    private void CreateGrid()
    {
        grid = new GridNode[gridNodesX, gridNodesY];
        Vector3 bottomLeft =
            transform.position - Vector3.right * GridSize.x - Vector3.forward * GridSize.y;

        for (int x = 0; x < gridNodesX; x++)
        {
            for (int y = 0; y < gridNodesY; y++)
            {
                Vector3 nodePos = bottomLeft + Vector3.right * (x * GridNodeSize + GridNodeRadius) +
                                       Vector3.forward * (y * GridNodeSize + GridNodeRadius);
                
                bool walkable =
                    !(Physics.CheckSphere(nodePos, GridNodeRadius,
                        unwalkableMask));

                grid[x, y] = new GridNode(walkable, true,nodePos, x, y);
            }
        }
    }

    public List<GridNode> GetNeighbours(GridNode node)
    {
        List<GridNode> neighbours = new List<GridNode>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.GridX + x;
                int checkY = node.GridY + y;

                if (checkX >= 0 && checkX < gridNodesX && checkY >= 0 && checkY < gridNodesY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    public GridNode NodeFromWorldPosition(Vector3 worldPos)
    {
        float percentX = Mathf.Clamp01((worldPos.x + GridSize.x / 2) / GridSize.x);
        float percentY = Mathf.Clamp01((worldPos.z + GridSize.y / 2) / GridSize.y);

        double totalLengthX = gridNodesX * GridNodeSize;
        double totalLengthY = gridNodesY * GridNodeSize;

        // Position on the axis
        double positionX = percentX * totalLengthX;
        double positionY = percentY * totalLengthY;

        // Index of the square (convert to integer to get grid coordinates)
        int x = (int)(positionX / GridNodeSize);
        int y = (int)(positionY / GridNodeSize);

        // Ensure the index is within grid bounds
        x = Mathf.Clamp(x, 0, gridNodesX - 1);
        y = Mathf.Clamp(y, 0, gridNodesY - 1);

        return grid[x, y];
    }

    private void OnDrawGizmos()
    {
        if (!ShowGizmos) return;
        Gizmos.DrawWireCube(transform.position, new Vector3(GridSize.x * GridNodeSize, 1, GridSize.y * GridNodeSize));


        if (grid == null) return;
        foreach (GridNode n in grid)
        {
            Gizmos.color = (n.Walkable) ? Color.white : Color.red;
            
            var pos = (transform.position + n.Position);
            var offset = (0.5f * GridNodeSize) * 0.75f;

            var topLeft = pos;
            topLeft.x -= offset;
            topLeft.z += offset;
            topLeft.y += 0.5f;

            var topRight = pos;
            topRight.x += offset;
            topRight.z += offset;
            topRight.y += 0.5f;

            var bottomLeft = pos;
            bottomLeft.x -= offset;
            bottomLeft.z -= offset;
            bottomLeft.y += 0.5f;

            var bottomRight = pos;
            bottomRight.x += offset;
            bottomRight.z -= offset;
            bottomRight.y += 0.5f;


            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(topLeft, bottomLeft);
            Gizmos.DrawLine(topRight, bottomRight);
        }
    }
}
