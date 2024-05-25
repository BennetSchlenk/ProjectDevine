using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DamageData: IDamageData
{
    public DamageTypeSO DamageType;

    #region Damage stats

    [SerializeField] private float damage;
    public float Damage => damage;

    [SerializeField] private float damageOverTime;
    public float DamageOverTime => damageOverTime;

    [SerializeField] private float damageOverTimeDuration;
    public float DamageOverTimeDuration => damageOverTimeDuration;

    [SerializeField] private float damageOverTimeTickRate;
    public float DamageOverTimeTickRate => damageOverTimeTickRate;

    #endregion

    public DamageData(DamageTypeSO damageType)
    {
        DamageType = damageType;
    }

    public void ApplyUpgrade(DamageDataUpgrade upgrade)
    {
        if (upgrade.DamageType != DamageType)
        {
            Debug.LogWarning("Trying to apply an upgrade with a different damage type.");
            return;
        }

        damage += upgrade.Damage;
        damageOverTime += upgrade.DamageOverTime;
        damageOverTimeDuration += upgrade.DamageOverTimeDuration;
        damageOverTimeTickRate += upgrade.DamageOverTimeTickRate;
    }

    public void RemoveUpgrade(DamageDataUpgrade upgrade)
    {
        if (upgrade.DamageType != DamageType)
        {
            Debug.LogWarning("Trying to remove an upgrade with a different damage type.");
            return;
        }

        damage -= upgrade.Damage;
        damageOverTime -= upgrade.DamageOverTime;
        damageOverTimeDuration -= upgrade.DamageOverTimeDuration;
        damageOverTimeTickRate -= upgrade.DamageOverTimeTickRate;
    }

    public void ResetStats()
    {
        damage = 0;
        damageOverTime = 0;
        damageOverTimeDuration = 0;
        damageOverTimeTickRate = 0;
    }
}

public interface IDamageData
{
    float Damage { get; }
    float DamageOverTime { get; }
    float DamageOverTimeDuration { get; }
    float DamageOverTimeTickRate { get; }
}