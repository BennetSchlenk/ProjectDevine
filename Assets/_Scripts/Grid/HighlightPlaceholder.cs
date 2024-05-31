using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightPlaceholder : MonoBehaviour
{
    
    private Material material;

    private void Awake()
    {
        material = GetComponent<Renderer>().material;
    }

    public void SetColor(Color color)
    {
        material.color = color;
    }

}
