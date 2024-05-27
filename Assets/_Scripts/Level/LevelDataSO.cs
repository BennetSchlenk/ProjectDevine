using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Level", menuName = "Project Divine/Level")]
public class LevelDataSO : ScriptableObject
{
    public string LevelName;
    public List<int> GridObj;
    public List<GridNode> Grid;
    public int GridX;
    public int GridY;
    public List<WaypointData> LevelWaypoints;
}
