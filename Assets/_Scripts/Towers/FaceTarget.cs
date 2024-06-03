using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceTarget : MonoBehaviour
{
    [SerializeField] private bool rotateX = true;
    [SerializeField] private Vector2 rotationLimitsX = new Vector2(-360, 360);
    [SerializeField] private bool rotateY = true;
    [SerializeField] private Vector2 rotationLimitsY = new Vector2(-360, 360);
    [SerializeField] private bool rotateZ = true;
    [SerializeField] private Vector2 rotationLimitsZ = new Vector2(-360, 360);
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
        {
            // Aply the rotation limits
            lookRotation.x = Mathf.Clamp(lookRotation.x, rotationLimitsX.x, rotationLimitsX.y);
        }
        if (!rotateY)
            lookRotation.y = Mathf.Clamp(lookRotation.y, rotationLimitsY.x, rotationLimitsY.y);
        if (!rotateZ)
            lookRotation.z = Mathf.Clamp(lookRotation.z, rotationLimitsZ.x, rotationLimitsZ.y);

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
}
