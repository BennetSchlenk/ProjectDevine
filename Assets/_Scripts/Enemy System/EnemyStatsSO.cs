using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Project Divine/Enemies/EnemyStatsSO")]
public class EnemyStatsSO : ScriptableObject
{
    [SerializeField] private float initialLife;
    [SerializeField] private float initialArmor;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rewardPointsForPlayer;
    [SerializeField] private float coreDamage;

    // Getters
    public float InitialLife => initialLife;
    public float InitialArmor => initialArmor;
    public float MovementSpeed => movementSpeed;
    public float RewardPointsForPlayer => rewardPointsForPlayer;
    public float CoreDamage => coreDamage;

}
