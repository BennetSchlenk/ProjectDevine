using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerDataManager : MonoBehaviour
{
    [SerializeField]
    private int maxHP;
    public int currHP;

    [SerializeField]
    private int startAmountEssence;
    public int currEssence;
    
    public delegate void HPChangedDelegate(int newHP, int maxHP);
    public event HPChangedDelegate OnHPChanged;
    
    public delegate void EssenceChangedDelegate(int newEssence);
    public event EssenceChangedDelegate OnEssenceChanged;
    
    #region Unity Callbacks
        
    private void Awake()
    {
        ServiceLocator.Instance.RegisterService(this);
        currHP = maxHP;
        currEssence = startAmountEssence;
    }

    private void Start()
    {
        
    }

    #endregion

    public Tuple<int, int> GetInitialValues()
    {
        return new Tuple<int, int>(maxHP, startAmountEssence);
    }

    public void AddEssence(int amount)
    {
        currEssence += amount;
        if (OnEssenceChanged != null) OnEssenceChanged(currEssence);
    }
    
    public bool RemoveEssence(int amount)
    {
        if (currEssence < amount) return false;
        currEssence -= amount;
        if (OnEssenceChanged != null) OnEssenceChanged(currEssence);
        return true;
    }

    public void RemoveHP(int amount)
    {
        currHP -= amount;
        if (OnHPChanged != null) OnHPChanged(currHP,maxHP);
        if (currHP <= 0)
        {
            TriggerGameOver();
        }
    }

    public void AddHP(int amount)
    {
        currHP += amount;
        if (OnHPChanged != null) OnHPChanged(currHP, maxHP);
    }
    
    private void TriggerGameOver()
    {
        //Debug.Log("<color=#00FF00><b>---=== GAME OVER, YOU ARE DONE :D ===---</b></color>");
    }
}
