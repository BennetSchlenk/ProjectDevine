using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{

    #region Unity Callbacks
        
    private void Awake()
    {
        
    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    #endregion

}
