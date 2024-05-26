using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for objects that can be selected.
/// Select is called when the object is selected.
/// DeSelect is called when the object is deselected.
/// </summary>
public interface ISelectable
{
    void Select();
    void Deselect();
}
