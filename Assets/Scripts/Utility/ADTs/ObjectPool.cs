using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class ObjectPool<T> where T : Object
{
    public delegate void ObjectCall(T item);
    private readonly List<T> objects = new();
    private readonly List<bool> available = new();
    private readonly T prefab;
    private readonly Transform parent;
    private readonly Action<T> initCall;
    private readonly Action<T> getCall;
    private readonly Action<T> returnCall;

    private T InstantiateNew()
    {
        T newObject = parent != null ? Object.Instantiate(prefab, parent) : Object.Instantiate(prefab);
        initCall?.Invoke(newObject);
        objects.Add(newObject);
        return newObject;
    }

    public ObjectPool(T prefab, Transform parent = null, int startAmount = 20,
        Action<T> initCall = null, Action<T> getCall = null, Action<T> returnCall = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.initCall = initCall;
        this.getCall = getCall;
        this.returnCall = returnCall;
        for (int i = 0; i < startAmount; i++)
        {
            InstantiateNew();
            available.Add(true);
        }
    }

    public T GetObject()
    {
        for (int i = 0; i < available.Count; i++)
            if (available[i])
            {
                available[i] = false;
                getCall?.Invoke(objects[i]);
                return objects[i];
            }
        T newObject = InstantiateNew();
        available.Add(false);
        getCall?.Invoke(newObject);
        return newObject;
    }

    public void ReturnObject(T returnedObject)
    {
        for (int i = 0; i < objects.Count; i++)
            if (objects[i] == returnedObject)
            {
                available[i] = true;
                returnCall?.Invoke(returnedObject);
                return;
            }
        Object.Destroy(returnedObject);
        Debug.LogError("Error: Trying to return an object that's not from this pool " +
            $"(of type {typeof(T)}), object destroyed");
    }

    public void ReturnAll()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            if (!available[i])
            {
                available[i] = true;
                returnCall?.Invoke(objects[i]);
            }
        }
    }
}