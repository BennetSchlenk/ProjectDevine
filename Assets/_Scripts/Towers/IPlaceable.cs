using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for objects that can be placed on the map.
/// OnPlacing is called when the object is being placed.
/// OnPlaced is called when the object is placed.
/// </summary>
public interface IPlaceable
{
    void OnPlacing();
    void OnPlaced();
}
