using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class WaypointData
{
    public int2 NodePos;
    public List<Vector3> Waypoints;
}
