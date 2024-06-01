using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerInfoCanvasController : MonoBehaviour
{
    private UIFollowGameObject uiFollowGameObject;
    private TowerInfoPanel towerInfoPanel;

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

    #endregion

    private void OnTowerSelected(Tower tower)
    {
        if (tower == null)
        {
            //uiFollowGameObject.gameObject.SetActive(false);
            towerInfoPanel.gameObject.SetActive(false);
            return;
        }

        //uiFollowGameObject.gameObject.SetActive(true);
        uiFollowGameObject.target = tower.gameObject;
        towerInfoPanel.gameObject.SetActive(true);
        towerInfoPanel.SetTower(tower);
    }

}
