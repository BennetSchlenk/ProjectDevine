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
    public int MeshIndex;
    public int MeshYRotation;
    public GameObject MeshObj;
    public GameObject TowerObj;
    public HighlightPlaceholder HighlightCell;

    public GridNode(bool walkable, bool buildable,GameObject meshObj, int meshIndex,int meshYRotation,bool spawn, bool enemyTarget,bool waypoint,Vector3 localPosition, int gridX, int gridY)
    {
        Walkable = walkable;
        Buildable = buildable;
        MeshObj = meshObj;
        MeshIndex = meshIndex;
        MeshYRotation = meshYRotation;
        Spawn = spawn;
        EnemyTarget = enemyTarget;
        Waypoint = waypoint;
        Position = localPosition;
        GridX = gridX;
        GridY = gridY;
    }
}
