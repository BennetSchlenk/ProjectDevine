
using System;
using System.Collections.Generic;

public interface IDamagable
{
    float Health { get; set; }

    /// <summary>
    /// Takes in a float as incoming Damage and return the amount of damage dealt
    /// </summary>
    /// <param name="incomingDamage">incoming Damage</param>
    /// <returns> amount of damage dealt</returns>
    float TakeDamage(List<DamageData> damageDataList, Action GiveXp);
    
    /// <summary>
    /// This will kill the damage received instantly and returns their damage taken (all remaining health)
    /// </summary>
    /// <returns></returns>
    float InstantKill();
}
