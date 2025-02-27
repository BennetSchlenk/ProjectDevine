using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public class SoundEmitterPool : MonoBehaviour
{
    [Header("Pool Settings")]
    public ObjectPool<SoundEmitter> Pool;
    [SerializeField, Space] private Transform parentForPoolObjects;

    [Tooltip("If true, the pool will check if the object is already in the pool before instantiating a new one. Set false to save CPU cycles.")]
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private SoundEmitter soundEmitterPrefab;
    [SerializeField] private int defaultCapacity = 30;
    [Tooltip("If the pool is at max capacity, the instantiated objects will be destroyed rather than returned to the pool.")]
    [SerializeField] private int maxCapacity = 60;

    [Header("Object Settings")]
    [SerializeField] private bool setActiveOnGet = true;

    private void Start()
    {
        Pool = new ObjectPool<SoundEmitter>(InstantiatePooledObject, OnGetFromPool, OnReturnToPool, OnDestroyPoolObject, collectionCheck, defaultCapacity, maxCapacity);
    }

    public void InitPoolWithValues(Transform parentForPoolObjects, bool collectionCheck, SoundEmitter soundEmitterPrefab, int defaultCapacity, int maxCapacity, bool setActiveOnGet)
    {
        this.parentForPoolObjects = parentForPoolObjects;
        this.collectionCheck = collectionCheck;
        this.soundEmitterPrefab = soundEmitterPrefab;
        this.defaultCapacity = defaultCapacity;
        this.maxCapacity = maxCapacity;
        this.setActiveOnGet = setActiveOnGet;

    }

    // Instantiate a new pooled object
    private SoundEmitter InstantiatePooledObject()
    {
        GameObject tmp = Instantiate(soundEmitterPrefab.gameObject, parentForPoolObjects);
        SoundEmitter soundEmitter = tmp.GetComponent<SoundEmitter>();
        soundEmitter.SetPool(Pool);
        soundEmitter.Init();

        return soundEmitter;
    }

    private void OnGetFromPool(SoundEmitter soundEmitter)
    {
        if (setActiveOnGet)
            soundEmitter.gameObject.SetActive(true);

        soundEmitter.OnGet();
    }

    private void OnReturnToPool(SoundEmitter soundEmitter)
    {
        //Debug.Log("OnReturnToPool " + soundEmitter.gameObject.name);
        soundEmitter.gameObject.SetActive(false);
    }

    // What will happen when the pool object is destroyed because the pool is at max capacity
    private void OnDestroyPoolObject(SoundEmitter soundEmitter)
    {

    }

}
