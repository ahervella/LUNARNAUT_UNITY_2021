using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_TimeTravel : Singleton<S_TimeTravel>
{
    public enum TIME_PERIOD { PAST, FUTURE}

    public event System.Action TimelineChanged = delegate { };
    public event System.Action ComposeAstroTTD = delegate { };
    public event System.Action ParseAstroTTD = delegate { };
    public event System.Action UpdateCamera = delegate { };

    [SerializeField]
    private AK.Wwise.State timeStateFuture;
    [SerializeField]
    private AK.Wwise.State timeStatePast;

    private TIME_PERIOD timeline = TIME_PERIOD.FUTURE;
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
            UpdateWwiseTimeState();
            ComposeAstroTTD();
            TimelineChanged();
            ParseAstroTTD();
            UpdateCamera();
        }
    }

    public event System.Action PlayerTimeTravelEnabledChanged = delegate { };
    private bool playertimeTravelEnabled;
    public bool PlayerTimeTravelEnabled
    {
        get => playertimeTravelEnabled;
        set
        {
            bool prevVal = playertimeTravelEnabled;
            playertimeTravelEnabled = value;
            if (prevVal != playertimeTravelEnabled)
            {
                PlayerTimeTravelEnabledChanged();
            }
        }
    }





    private void Awake()
    {
        S_DeveloperTools.Current.TogglePlayerEnableTimeTravelChanged -= S_DeveloperTools_TogglePlayerEnableTimeTravelChanged;
        S_DeveloperTools.Current.TogglePlayerEnableTimeTravelChanged += S_DeveloperTools_TogglePlayerEnableTimeTravelChanged;

        S_DeveloperTools_TogglePlayerEnableTimeTravelChanged();
    }

    private void S_DeveloperTools_TogglePlayerEnableTimeTravelChanged()
    {
        if (S_DeveloperTools.Current.DevToolsEnabled_TIME_TRAVEL())
        {
            PlayerTimeTravelEnabled = S_DeveloperTools.Current.TogglePlayerEnableTimeTravel;
        }
    }

    private void UpdateWwiseTimeState()
    {
        if (Timeline == TIME_PERIOD.FUTURE)
        {
           timeStateFuture.SetValue();
        }
        if (Timeline == TIME_PERIOD.PAST)
        {
            timeStatePast.SetValue();
        }
    }

    public bool InPast() => Timeline == TIME_PERIOD.PAST;
    public bool InFuture() => Timeline == TIME_PERIOD.FUTURE;
}
