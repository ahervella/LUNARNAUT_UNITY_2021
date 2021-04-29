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


    private Dictionary<Type, ITimeTravelData> pastDatasCache = new Dictionary<Type, ITimeTravelData>();
    private Dictionary<Type, ITimeTravelData> futreDatasCache = new Dictionary<Type, ITimeTravelData>();

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
        ChangeEnabledTimeObjects();
    }

    private void ChangeEnabledTimeObjects()
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
        if (pastInteractive != null && futureInteractive != null)
        {
            if (S_TimeTravel.Current.InFuture())
            {
                pastDatasCache = MakeTTDDatasDeepCopy(pastInteractive.ComposeTimeTravelDatas(new Dictionary<Type, ITimeTravelData>()));

                //Only change the future if we did something to change the past so that
                //if we had set speicifc things in the future but still made no change in the past
                //and go back to the future, should be how we left them
                if (pastInteractive.TimeTravel_ChangedState)
                {
                    pastInteractive.TimeTravel_ChangedState = false;
                    futureInteractive.ParseTimeTravelDatas(MakeTTDDatasDeepCopy(pastDatasCache));
                }
                else
                {
                    futureInteractive.ParseTimeTravelDatas(MakeTTDDatasDeepCopy(futreDatasCache));
                }
            }
            else
            {
                futreDatasCache = MakeTTDDatasDeepCopy(futureInteractive.ComposeTimeTravelDatas(new Dictionary<Type, ITimeTravelData>()));

                pastInteractive.TimeTravel_ChangedState = false;
                pastInteractive.ParseTimeTravelDatas(MakeTTDDatasDeepCopy(pastDatasCache));
            }
        }


        ChangeEnabledTimeObjects();
    }

    private Dictionary<Type, ITimeTravelData> MakeTTDDatasDeepCopy(Dictionary<Type, ITimeTravelData> datas)
    {
        Dictionary<Type, ITimeTravelData> deepCopy = new Dictionary<Type, ITimeTravelData>();
        foreach (KeyValuePair<Type, ITimeTravelData> kvp in datas)
        {
            deepCopy.Add(kvp.Key, kvp.Value.MakeDeepCopy());
        }
        return deepCopy;
    }
}
