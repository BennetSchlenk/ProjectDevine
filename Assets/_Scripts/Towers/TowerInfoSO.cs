using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New TowerInfo", menuName = "Project Divine/Towers/Tower Info")]
public class TowerInfoSO : ScriptableObject
{
    [SerializeField] private string towerName;
    public string TowerName => towerName;

    [SerializeField] private string towerDescription;
    public string TowerDescription => towerDescription;

    [SerializeField] private Sprite towerSprite;
    public Sprite TowerSprite => towerSprite;
}
