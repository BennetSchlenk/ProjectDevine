using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitBootStrapper : MonoBehaviour
{

    #region Unity Callbacks
        
    private void Awake()
    {
        if(GameObject.Find("GameManager") == null)
        {
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }

        

    }

    private void Start()
    {
        
    }

    #endregion

}
