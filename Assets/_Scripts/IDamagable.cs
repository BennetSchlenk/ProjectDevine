
using System;
using System.Collections.Generic;

public interface IDamagable
{
    float Health { get; set; }
    float Armor {  get; set; }

    /// <summary>
    /// Takes in a list DamageData
    /// </summary>
    /// <param name="damageDataList">list DamageData</param>
    /// <param name="xpGainer"></param>
    void TakeDamage(List<DamageData> damageDataList, IXPGainer xpGainer);
    
    /// <summary>
    /// This will kill the damage received instantly and returns their damage taken (all remaining health)
    /// </summary>
    void InstantKill();
    
}
