using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public GameObject Prefab;

    [SerializeField] private GameObject highlightFrame;

    #region Unity Callbacks
        
    private void Awake()
    {
        
    }

    private void Start()
    {
        
    }

    #endregion


    public void Highlight(bool toggle)
    {
        highlightFrame.SetActive(toggle);
    }
}
