using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSaveData
{
    public string LevelName;
    public List<int> GridObj;
    public List<GridNode> Grid;
    public int GridX;
    public int GridY;
    public List<WaypointData> LevelWaypoints;
}
