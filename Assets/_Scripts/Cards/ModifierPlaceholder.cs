using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifierPlaceholder : MonoBehaviour, IPlaceable
{
    [Tooltip("Model to show when the card is being dragged. It will have the size of the grid cell.")]
    [SerializeField] private GameObject placeholderModel;

    public void OnPlaced()
    {
        
    }

    public void OnPlacing()
    {
        // TODO: Scale the model to the size of the grid cell
    }

}
