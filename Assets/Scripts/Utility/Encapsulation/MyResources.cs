using UnityEngine;

/// <summary>
/// An encapsulation for loading resources.
/// </summary>
public class MyResources
{
    public static T Load<T>(string path) where T : Object
    {
        T obj = Resources.Load<T>(path);
        if (obj == null) Debug.Log("Load resource failed.");
        return obj;
    }
}
