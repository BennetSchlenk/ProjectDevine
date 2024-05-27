using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemyMovementController : MonoBehaviour
{
    
    [SerializeField] private float reachWaypointTolerance = .3f;

    public bool IsMoving {  get; private set; }
    public Vector3 CurrentTargetPosition { get { return waypoints[currentWayPointIndex]; } }

    // cached vars
    private Enemy enemy;
    private List<Vector3> waypoints = new();

    private int currentWayPointIndex = 0;
    

    #region Unity Callbacks

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }


    private void Update()
    {
        if (IsMoving)
        {
            MoveEnemy();
        }
    }

    #endregion

    public void StartMoving(List<Vector3> waypoints)
    {
        this.waypoints = waypoints;
        Assert.IsTrue(waypoints.Count > 0);
        ChangeToNextWaypoint(); // go directly to next as first item is the spawn point
        IsMoving = true;
    }

    private void MoveEnemy()
    {

        Vector3 destinationDirection = CurrentTargetPosition - transform.position;
        destinationDirection.y = 0f;

        float destinationDistance = destinationDirection.magnitude;

        if (destinationDistance >= reachWaypointTolerance)
        {
            Quaternion targetRotation = Quaternion.LookRotation(destinationDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, enemy.Stats.RotationSpeed * Time.deltaTime);
            transform.Translate(Vector3.forward * enemy.Stats.MovementSpeed * Time.deltaTime);
        }
        else
        {
            ChangeToNextWaypoint();
        }

    }

    private void ChangeToNextWaypoint()
    {
        if (currentWayPointIndex + 1 < waypoints.Count)
        {
            currentWayPointIndex++;
        }
        else
        {
            Debug.LogWarning($"We should not see this: we run out of waypoints and [{enemy.gameObject.name}] is still alive and not destroyed by the core");
            IsMoving = false;
        }
        
    }
}
