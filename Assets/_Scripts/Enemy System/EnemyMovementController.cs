using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{

    public bool IsMoving {  get; private set; }

    // cached vars
    private Enemy enemy;

    #region Unity Callbacks
        
    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }


    private void Update()
    {
        
    }

    #endregion

    public void StartRandomMovement()
    {
        // Generate random direction vector
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        // Generate random distance between 5 and 15 units
        float randomDistance = Random.Range(5f, 15f);

        // Calculate target position based on direction and distance
        Vector3 targetPosition = gameObject.transform.position + new Vector3(randomDirection.x, 0f, randomDirection.y) * randomDistance;

        // Move the object towards the target position with fixed speed
        StartCoroutine(MoveToTarget(targetPosition, enemy.Stats.MovementSpeed));
    }

    IEnumerator MoveToTarget(Vector3 target, float speed)
    {
        while (Vector2.Distance(transform.position, target) > 0.1f)
        {
            // Calculate movement direction
            Vector2 direction = target - transform.position;

            // Normalize the direction to get a unit vector (avoid faster diagonal movement)
            direction.Normalize();

            // Move towards the target at fixed speed
            transform.Translate(direction * speed * Time.deltaTime);

            yield return null;
        }
    }


}
