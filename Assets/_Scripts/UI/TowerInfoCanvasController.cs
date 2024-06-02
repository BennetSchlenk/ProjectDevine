using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerInfoCanvasController : MonoBehaviour
{
    private UIFollowGameObject uiFollowGameObject;
    private TowerInfoPanel towerInfoPanel;
    private float refreshRate = 1f;
    private float currentTime = 0f;

    #region Unity Callbacks
        
    private void Awake()
    {
        uiFollowGameObject = GetComponent<UIFollowGameObject>();
    }

    private void Start()
    {
        towerInfoPanel = GetComponentInChildren<TowerInfoPanel>(true);
        towerInfoPanel.gameObject.SetActive(false);

        GlobalData.OnTowerSelected += OnTowerSelected;
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime < refreshRate) return;

        if (towerInfoPanel.gameObject.activeSelf)
            towerInfoPanel.Refresh();
        currentTime -= refreshRate;
    }

    private void OnDestroy()
    {
        GlobalData.OnTowerSelected -= OnTowerSelected;
    }

    #endregion

    private void OnTowerSelected(Tower tower)
    {
        if (tower == null)
        {
           if (Time.time - towerInfoPanel.LastTimeButtonPressed < 0.5f)
            {
                towerInfoPanel.gameObject.SetActive(true);
                towerInfoPanel.Refresh();
                return;
            } else
            {
                //uiFollowGameObject.gameObject.SetActive(false);
                towerInfoPanel.gameObject.SetActive(false);
                return;
            }

        }

        //uiFollowGameObject.gameObject.SetActive(true);
        uiFollowGameObject.target = tower.gameObject;
        towerInfoPanel.gameObject.SetActive(true);
        towerInfoPanel.SetTower(tower);        
    }

}
