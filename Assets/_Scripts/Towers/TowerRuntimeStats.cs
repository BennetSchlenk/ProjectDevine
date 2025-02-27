using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[System.Serializable]
public class TowerRuntimeStats: ITowerData
{
    public TowerDataUpgradeSO BaseStats;
    

    #region Tower runtime stats

    private int level = 1;
    private int tier;
    private float range;
    private float fireRate;
    private float fireDuration;
    private float fireCooldown;
    private float projectileSpeed;

    #endregion  

    #region Tower stats getters

    public int Level => level;
    public int Tier { get => tier; set => tier = value;}
    public float Range => BaseStats ? BaseStats.Range + range : range;
    public float FireRate => BaseStats ? BaseStats.FireRate + fireRate : fireRate;
    public float FireDuration => BaseStats ? BaseStats.FireDuration + fireDuration : fireDuration;
    public float FireCooldown => BaseStats ? BaseStats.FireCooldown + fireCooldown : fireCooldown;
    public float ProjectileSpeed => BaseStats ? BaseStats.ProjectileSpeed : projectileSpeed;

    #endregion

    public TowerRuntimeStats(TowerDataUpgradeSO baseStats, int _tier)
    {
        BaseStats = baseStats;
        Tier = _tier;
    }

    public void ApplyUpgrade(TowerDataUpgradeSO upgrade, bool levelUp = false)
    {
        range += upgrade.Range;
        fireRate += upgrade.FireRate;
        fireDuration += upgrade.FireDuration;
        fireCooldown += upgrade.FireCooldown;

        if (levelUp)
            level++;
    }

    public void RemoveUpgrade(TowerDataUpgradeSO upgrade)
    {
        
        range -= upgrade.Range;
        fireRate -= upgrade.FireRate;
        fireDuration -= upgrade.FireDuration;
        fireCooldown -= upgrade.FireCooldown;

        // Return the card with the removed stats to the pool
    }

    public void ResetStats()
    {
        range = 0;
        fireRate = 0;
        fireDuration = 0;
        fireCooldown = 0;
    }

}

public interface ITowerData
{
    
    float Range { get; }
    float FireRate { get; } // Fire rate in shots per second
    float FireDuration { get; } // Duration of the fire round in seconds
    float FireCooldown { get; } // Cooldown between fire rounds in seconds
    float ProjectileSpeed { get; } // Projectile speed in units per second
}


