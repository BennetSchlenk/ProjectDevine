using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDataUpgrade : MonoBehaviour, IDamageData
{
#if UNITY_EDITOR
    [Header("These values should increase the existing ones, not override them.")]
    [Space(10)]
#endif

    #region Damage stats

    [SerializeField] private DamageTypeSO damageType;
    public DamageTypeSO DamageType => damageType;

    [SerializeField] private float damage;
    public float Damage => damage;

    [SerializeField] private float damageOverTime;
    public float DamageOverTime => damageOverTime;

    [SerializeField] private float damageOverTimeDuration;
    public float DamageOverTimeDuration => damageOverTimeDuration;

    [SerializeField] private float damageOverTimeTickRate;
    public float DamageOverTimeTickRate => damageOverTimeTickRate;

    #endregion
}
