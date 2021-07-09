using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/*
 * Singelton tutorials and code used:
 * https://blog.mzikmund.com/2019/01/a-modern-singleton-in-unity/
 * https://gamedev.stackexchange.com/questions/116009/in-unity-how-do-i-correctly-implement-the-singleton-pattern
 * 
 * Scriptable Object Singleton tutorial from this video:
 * https://www.youtube.com/watch?v=6kWUGEQiMUI&ab_channel=whateep
*/

public abstract class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
{
    //Path to scriptable object singletons
    private const string SSO_PATH = "SINGLETONS";
    private static readonly object _lock = new object();
    #region **NOTE EDITS ON RUNTIME WILL ONLY APPLY UNTIL NEXT RUN**
    #endregion
    //Too risky to include because this saves the state of non serialized variables temporarily too
    //[SerializeField]
    //private bool keepChangesInPlayMode = false;
    //public bool KeepChangesInPlayMode => keepChangesInPlayMode;

    private static bool _threadSafe = true;

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
            T[] scriptableObjs = Resources.LoadAll<T>(SSO_PATH);
            if (scriptableObjs == null || scriptableObjs.Length < 1)
            {
                throw new Exception("No singleton scriptable object of type " + typeof(T).FullName + "could be found");
            }
            else if (scriptableObjs.Length > 1)
            {
                Debug.LogError("There is more than one scriptable object of the singleton scriptable object: " + typeof(T).FullName);
            }

            _current = /*scriptableObjs[0].KeepChangesInPlayMode? scriptableObjs[0] :*/ Instantiate(scriptableObjs[0]);

            //for scriptable objects, we want to make sure we make a file for each manually
            //for every new one, lets not mess with creating new ones at runtime, won't be
            //able to once export game
            //_current = CreateSingleton();
        }
        return _current;
    }

    protected void OnEnable()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode && !Application.isPlaying)
        {
            return;
        }

        OnRuntimeEnable();
    }

    protected abstract void OnRuntimeEnable();

    protected Coroutine StartCoroutine(IEnumerator crMethod)
    {
        return S_GameObjectDumby.Current.StartCoroutine(crMethod);
    }

    //TODO: ^^ do the same but being able to add an AddToUpdate function and remove
}
