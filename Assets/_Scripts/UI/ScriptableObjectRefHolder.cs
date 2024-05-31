using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectRefHolder : MonoBehaviour
{
    [HideInInspector]
    public ScriptableObject scriptable;
    #region Unity Callbacks
        
    private void Awake()
    {
        
    }

    private void Start()
    {
        
    }

    #endregion

}
