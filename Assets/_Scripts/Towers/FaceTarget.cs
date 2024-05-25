using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceTarget : MonoBehaviour
{
    [SerializeField] private bool rotateX = true;
    [SerializeField] private bool rotateY = true;
    [SerializeField] private bool rotateZ = true;
    [SerializeField] private float rotationSpeed = 5f;

    private Transform target;

    #region Unity Callbacks
        
    private void Awake()
    {
        
    }

    private void Start()
    {
        
    }

    #endregion


    public void SetTarget(Transform target)
    {
       if (target == null)
            return;

       this.target = target;     
    }

    private void Update()
    {
        if (target == null)
            return;

        Vector3 direction = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        if (!rotateX)
            lookRotation.x = transform.rotation.x;
        if (!rotateY)
            lookRotation.y = transform.rotation.y;
        if (!rotateZ)
            lookRotation.z = transform.rotation.z;

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
}
