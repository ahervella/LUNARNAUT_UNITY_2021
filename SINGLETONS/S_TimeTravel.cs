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
    public event System.Action InitTimeTravelFade = delegate { };

    private bool ttInProgress = false;
    private bool ttWereControlsEnabled = true;

    [SerializeField]
    private AK.Wwise.State futureWwiseState;
    [SerializeField]
    private AK.Wwise.State pastWwiseState;

    private TIME_PERIOD timeline = TIME_PERIOD.FUTURE;
    public TIME_PERIOD Timeline
    {
        get => timeline;
        private set
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
            SetWwiseTimeState();
            ttInProgress = false;
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

    private void SetWwiseTimeState()
    {
        if (Timeline == TIME_PERIOD.FUTURE)
        {
            futureWwiseState.SetValue();
        }
        else if (Timeline == TIME_PERIOD.PAST)
        {
            pastWwiseState.SetValue();
        }
    }

    private void Awake()
    {
        S_DeveloperTools.Current.TogglePlayerEnableTimeTravelChanged -= S_DeveloperTools_TogglePlayerEnableTimeTravelChanged;
        S_DeveloperTools.Current.TogglePlayerEnableTimeTravelChanged += S_DeveloperTools_TogglePlayerEnableTimeTravelChanged;

        AstroCamera.FadeOutComplete -= AstroCamera_FadeOutComplete;
        AstroCamera.FadeOutComplete += AstroCamera_FadeOutComplete;

        AstroCamera.FadeInComplete -= AstroCamera_FadeInComplete;
        AstroCamera.FadeInComplete += AstroCamera_FadeInComplete;

        S_DeveloperTools_TogglePlayerEnableTimeTravelChanged();
    }

    private void S_DeveloperTools_TogglePlayerEnableTimeTravelChanged()
    {
        if (S_DeveloperTools.Current.DevToolsEnabled_TIME_TRAVEL())
        {
            PlayerTimeTravelEnabled = S_DeveloperTools.Current.TogglePlayerEnableTimeTravel;
        }
    }

    public void ToggleTimeTravel()
    {
        if (InFuture())
        {
            InitTimeTravel(TIME_PERIOD.PAST);
        }
        else
        {
            InitTimeTravel(TIME_PERIOD.FUTURE);
        }
    }

    public void InitTimeTravel(TIME_PERIOD newTimeline)
    {
        if (ttInProgress)
        {
            Debug.Log("Tried to time travel when it was in progress...");
            return;
        }
        ttInProgress = true;
        S_AstroInputManager.Current.ControlsEnabled = false;
        InitTimeTravelFade();
    }

    public void AstroCamera_FadeOutComplete()
    {
        Timeline = InFuture() ? TIME_PERIOD.PAST : TIME_PERIOD.FUTURE;
    }

    public void AstroCamera_FadeInComplete()
    {
        S_AstroInputManager.Current.ControlsEnabled = ttWereControlsEnabled;
    }

    public bool InPast() => Timeline == TIME_PERIOD.PAST;
    public bool InFuture() => Timeline == TIME_PERIOD.FUTURE;
}
