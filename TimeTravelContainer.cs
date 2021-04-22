using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTravelContainer : MonoBehaviour
{
    private SO_BA_InPast inPast;

    [SerializeField]
    private A_Interactive futureInteractive;
    [SerializeField]
    private List<SO_BoolArgument> extraFutureArgs = default;
    [SerializeField]
    private List<GameObject> futureObjects = default;

    [SerializeField]
    private A_Interactive pastInteractive;
    [SerializeField]
    private List<SO_BoolArgument> extraPastArgs = default;
    [SerializeField]
    private List<GameObject> pastObjects = default;

    private void Awake()
    {
        
    }

    private void Start()
    {
        if (inPast.IsTrue())
        {
            if (AllBoolArgsTrue(extraPastArgs))
            {
                SetEnableAllObjects(ref pastObjects, true);
                SetEnableAllObjects(ref futureObjects, false);
            }
        }
        else if (AllBoolArgsTrue(extraFutureArgs))
        {
            SetEnableAllObjects(ref pastObjects, false);
            SetEnableAllObjects(ref futureObjects, true);
        }
    }

    private bool AllBoolArgsTrue(List<SO_BoolArgument> boolArgs)
    {
        foreach (SO_BoolArgument ba in boolArgs)
        {
            if (!ba.IsTrue())
            {
                return false;
            }
        }

        return true;
    }

    private void SetEnableAllObjects(ref List<GameObject> objs, bool enableVal)
    {
        foreach(GameObject obj in objs)
        {
            obj.SetActive(enableVal);
        }
    }
}
