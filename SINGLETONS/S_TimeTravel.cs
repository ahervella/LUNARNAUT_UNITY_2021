using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_TimeTravel : Singleton<S_TimeTravel>
{
    public enum TIME_PERIOD { PAST, FUTURE}

    public event System.Action TimelineChanged = delegate { };
    //public event System.Action LeavingCurrTimeline = delegate { };
    public event System.Action ComposeAstroTTD = delegate { };
    public event System.Action ParseAstroTTD = delegate { };
    public event System.Action UpdateCamera = delegate { };
    //public event System.Action EnteringNewTimeline = delegate { };
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
            ComposeAstroTTD();
            TimelineChanged();
            ParseAstroTTD();
            UpdateCamera();
        }
    }

    public bool InPast() => Timeline == TIME_PERIOD.PAST;
    public bool InFuture() => Timeline == TIME_PERIOD.FUTURE;
}
