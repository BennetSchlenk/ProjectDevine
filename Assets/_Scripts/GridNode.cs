using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNode
{
    public bool Walkable;
    public bool Buildable;
    public Vector3 Position;
    public int GridX;
    public int GridY;

    //Distance based on vertical/horizontal move = 10, diagonal move = 14
    //Distance from StartNode
    public int gCost;
    //Distance from TargetNode
    public int hCost;
    public int fCost => gCost + hCost;

    public GridNode ParentNode;
    private int heapIndex;

    public GridNode(bool walkable, bool buildable,Vector3 localPosition, int gridX, int gridY)
    {
        Walkable = walkable;
        Buildable = buildable;
        Position = localPosition;
        GridX = gridX;
        GridY = gridY;
    }

    public int CompareTo(GridNode other)
    {
        int compare = fCost.CompareTo(other.fCost);
        //If fcost is the same use hcost as decider
        if (compare == 0)
        {
            compare = hCost.CompareTo(other.hCost);
        }

        return -compare;
    }

    public int HeapIndex
    {
        get => heapIndex;
        set => heapIndex = value;
    }
}
