using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New TowerStatsUpgrade", menuName = "Project Divine/Towers/Tower Stats Upgrade")]
public class TowerDataUpgradeSO : ScriptableObject, ITowerData
{
#if UNITY_EDITOR
    [Header("These values should increase the existing ones, not override them.")]
    [Space(10)]
#endif

    #region Tower data

    [SerializeField] private float range;
    public float Range => range;

    [Tooltip("Fire rate in shots per second")]
    [SerializeField] private float fireRate;
    public float FireRate => fireRate;

    [SerializeField] private float fireDuration;
    public float FireDuration => fireDuration;

    [SerializeField] private float fireCooldown;
    public float FireCooldown => fireCooldown;

    [SerializeField] private float projectileSpeed;
    public float ProjectileSpeed => projectileSpeed;

    #endregion
}