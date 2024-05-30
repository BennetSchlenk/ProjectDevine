using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    [SerializeField]
    private int MaxHP;
    public int currHP;

    [SerializeField]
    private int startAmountEssence;
    public int currEssence;
    
    #region Unity Callbacks
        
    private void Awake()
    {
        currHP = MaxHP;
        currEssence = startAmountEssence;
    }

    private void Start()
    {
        ServiceLocator.Instance.RegisterService(this);
    }

    #endregion

    public void AddEssence(int amount)
    {
        currEssence += amount;
    }
    
    public bool RemoveEssence(int amount)
    {
        if (currEssence < amount) return false;
        currEssence -= amount;
        return true;
    }

    public void RemoveHP(int amount)
    {
        currHP -= amount;
        if (currHP <= 0)
        {
            TriggerGameOver();
        }
    }

    public void AddHP(int amount)
    {
        currHP += amount;
    }
    
    private void TriggerGameOver()
    {
        Debug.Log("<color=#00FF00><b>---=== GAME OVER, YOU ARE DONE :D ===---</b></color>");
    }
}
