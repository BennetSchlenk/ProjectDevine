using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServiceLocator
{
    private static ServiceLocator _instance = new ServiceLocator();
    private Dictionary<Type, object> services = new Dictionary<Type, object>();

    public static ServiceLocator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ServiceLocator();
            }

            return _instance;
        }
    }

    public bool RegisterService<T>(T service)
    {
        if (!services.ContainsValue(service))
        {
            services[typeof(T)] = service;
            return true;
        }
        else
        {
            return false;
        }
            
    }

    public T GetService<T>()
    {
        return (T)services[typeof(T)];
    }
}