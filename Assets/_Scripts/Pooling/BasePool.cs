using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BasePool : MonoBehaviour
{
    [Header("Pool Settings")]
    public ObjectPool<Reusable> pool;

    [Tooltip("If true, the pool will check if the object is already in the pool before instantiating a new one. Set false to save CPU cycles.")]
    [SerializeField] private bool collectionCheck = false;
    [SerializeField] private Reusable objectToPool;
    [SerializeField] private int defaultCapacity = 10;
    [Tooltip("If the pool is at max capacity, the instantiated objects will be destroyed rather than returned to the pool.")]
    [SerializeField] private int maxCapacity = 20;

    [Header("Object Settings")]
    [SerializeField] private bool setActiveOnGet = true;

    private void Start()
    {
        pool = new ObjectPool<Reusable>(InstantiatePooledObject, OnGetFromPool, OnReturnToPool, OnDestroyPoolObject, collectionCheck, defaultCapacity, maxCapacity);
    }

    // Instantiate a new pooled object
    private Reusable InstantiatePooledObject()
    {
        GameObject tmp = Instantiate(objectToPool.gameObject);
        Reusable reusable = tmp.GetComponent<Reusable>();
        reusable.SetPool(pool);

        return reusable;
    }

    private void OnGetFromPool(Reusable reusable)
    {
        if (setActiveOnGet)
            reusable.gameObject.SetActive(true);

        reusable.OnGet();
    }

    private void OnReturnToPool(Reusable reusable)
    {
        Debug.Log("OnReturnToPool " + reusable.gameObject.name);
        reusable.gameObject.SetActive(false);
    }

    // What will happen when the pool object is destroyed because the pool is at max capacity
    private void OnDestroyPoolObject(Reusable reusable)
    {

    }
}