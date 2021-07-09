using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Singelton tutorials and code used:
 * https://blog.mzikmund.com/2019/01/a-modern-singleton-in-unity/
 * https://gamedev.stackexchange.com/questions/116009/in-unity-how-do-i-correctly-implement-the-singleton-pattern
 * 
*/

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static readonly object _lock = new object();

    private static bool _threadSafe = true;
    private bool _persistent = true;

    private static T _current;
    public static T Current
    {
        get
        {
            if (_threadSafe)
            {
                lock (_lock)
                {
                    return GetSingleton();
                }
            }

            return GetSingleton();
        }
    }

    private static T GetSingleton()
    {
        if (_current == null)
        {
            _current = CreateSingleton();
        }
        return _current;
    }

    private static T CreateSingleton()
    {
        //in case we are using an inspector singleton
        T[] instances = FindObjectsOfType<T>();
        var count = instances.Length;
        if (count > 0)
        {
            for (int i = 1; i < count; i++)
            {
                if (i == 1)
                {
                    PrintMultiSingletonWarningMessage(count);
                }
                Destroy(instances[i]);
            }
            return instances[0];
        }

        var ownerObject = new GameObject($"{typeof(T).Name} (runtime singleton)");
        var instance = ownerObject.AddComponent<T>();
        return instance;
    }

    private static void PrintMultiSingletonWarningMessage(int numberOfTotalSingletonsFound)
    {
        Debug.LogWarning($"[{nameof(T)}] There should never be more than one {nameof(T)} of type {typeof(T)} in the scene, but {numberOfTotalSingletonsFound} were found. The first instance found will be used, and all others will be destroyed.");
    }

    protected virtual void Awake()
    {
        lock (_lock)
        {
            //SingletonSettings();

            if (_persistent)
            {
                DontDestroyOnLoad(gameObject);
            }

            CleanUpDuplicatedSingletons();
        }

    }


    protected static void CleanUpDuplicatedSingletons()
    {
        var instances = FindObjectsOfType<T>();
        var count = instances.Length;
        
        if (count > 1)
        {
            PrintMultiSingletonWarningMessage(count);
            for (var i = instances.Length-1; i >= 0; i--)
            {
                Debug.LogFormat("destroying singleton dup: {0}", instances[i].name);
                Destroy(instances[i]);
            }
        }
    }
}
