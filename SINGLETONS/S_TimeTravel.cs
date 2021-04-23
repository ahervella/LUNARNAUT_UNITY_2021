using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_TimeTravel : Singleton<S_TimeTravel>
{
    public enum TIME_PERIOD { PAST, FUTURE}

    public event System.Action TimelineChanged = delegate { };
    private TIME_PERIOD timeline;
    public TIME_PERIOD Timeline
    {
        get => timeline;
        set
        {
            if (timeline == value)
            {
                Debug.LogFormat("Tried to travel to the {0}, but we are here, in the {0}!", value);
                return;
            }

            timeline = value;
            TimelineChanged();
        }
    }

    public bool InPast() => Timeline == TIME_PERIOD.PAST;
    public bool InFuture() => Timeline == TIME_PERIOD.PAST;
}
