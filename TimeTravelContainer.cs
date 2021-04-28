using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTravelContainer : MonoBehaviour
{
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
        S_TimeTravel.Current.TimelineChanged -= S_TimeTravel_TimelineChanged;
        S_TimeTravel.Current.TimelineChanged += S_TimeTravel_TimelineChanged;

        if (futureInteractive == null && pastInteractive == null)
        {
            return;
        }

        if (futureInteractive?.GetType() != pastInteractive?.GetType())
        {
            Debug.LogErrorFormat("TimeTravelContainer on object {0} does not have the same type A_Interactive for the past and future :(", gameObject.name);
        }
    }

    private void Start()
    {
        if (S_TimeTravel.Current.InPast())
        {
            if (AllBoolArgsTrue(extraPastArgs))
            {
                SetEnableAllObjects(ref pastObjects, true);
            }

            SetEnableAllObjects(ref futureObjects, false);
        }
        else
        {
            if (AllBoolArgsTrue(extraFutureArgs))
            {

                SetEnableAllObjects(ref futureObjects, true);
            }
            SetEnableAllObjects(ref pastObjects, false);
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

    private void S_TimeTravel_TimelineChanged()
    {
        if (S_TimeTravel.Current.InFuture())
        {

            Dictionary<Type, ITimeTravelData> datas = pastInteractive.ComposeTimeTravelDatas(new Dictionary<Type, ITimeTravelData>());
            futureInteractive.ParseTimeTravelDatas(datas);
        }
    }
}
