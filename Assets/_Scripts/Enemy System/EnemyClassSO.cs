using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyClass", menuName = "Project Divine/Enemies/EnemyClassSO")]
public class EnemyClassSO : ScriptableObject
{
    [Header("Base stats")]
    [SerializeField] private float initialLife;
    [SerializeField] private float initialArmor;
    [SerializeField] private float movementSpeed;
    //[SerializeField] private float rotationSpeed;
    [SerializeField] private int pointsForPlayerIfKilled;
    [SerializeField] private int coreDamage;

    // Getters
    public float InitialLife => initialLife;
    public float InitialArmor => initialArmor;
    public float MovementSpeed => movementSpeed;
    public float RotationSpeed => movementSpeed * 100f;
    public int PointsForPlayerIfKilled => pointsForPlayerIfKilled;
    public int CoreDamage => coreDamage;

}
