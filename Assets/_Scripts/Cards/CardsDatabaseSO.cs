using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardsDatabase", menuName = "Project Divine/Cards/CardsDatabase", order = 1)]
public class CardsDatabaseSO : ScriptableObject
{

    [SerializeField] private List<CardDataSO> database;
    public List<CardDataSO> Database => database;

}
