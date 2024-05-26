using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GridNode
{
    public bool Walkable;
    public bool Buildable;
    public bool Spawn;
    public bool EnemyTarget;
    public bool Waypoint;
    public Vector3 Position;
    public int GridX;
    public int GridY;
    public GameObject MeshObj;

    public GridNode(bool walkable, bool buildable,GameObject meshObj,bool spawn, bool enemyTarget,bool waypoint,Vector3 localPosition, int gridX, int gridY)
    {
        Walkable = walkable;
        Buildable = buildable;
        MeshObj = meshObj;
        Spawn = spawn;
        EnemyTarget = enemyTarget;
        Waypoint = waypoint;
        Position = localPosition;
        GridX = gridX;
        GridY = gridY;
    }
}
