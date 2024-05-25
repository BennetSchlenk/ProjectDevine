using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(EnemyMovementController))]
public class Enemy : MonoBehaviour, IDamagable
{
    [Header("Basic configs")]    
    [SerializeField] private EnemyStatsSO stats;

    public float Health { get; set; }
    public EnemyStatsSO Stats { get {  return stats; } }
    public EnemyMovementController MovementController {  get; private set; }

    #region Unity Callbacks

    private void Awake()
    {
        Health = stats.InitialLife;
        MovementController = GetComponent<EnemyMovementController>();
    }

    private void Update()
    {
        
    }

    #endregion

    public void Init()
    {
        MovementController.StartRandomMovement();
    }
    
    public float TakeDamage(float incomingAmount)
    {
        float damageTaken;

        Health = Mathf.Clamp(Health - incomingAmount, 0f, Health);

        damageTaken = Health - (Health - incomingAmount);

        if (damageTaken > 0f)
        {
            // Handle the damage taken, animations, effects, etc

            // trigger death of enemy
            if (Health <= 0f)
            {
                DestroySelf();
            }
        }
        return damageTaken;
    }


    private void DestroySelf()
    {
        Destroy(gameObject);
    }


}
