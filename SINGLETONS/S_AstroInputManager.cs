using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_AstroInputManager : Singleton<S_AstroInputManager>
{
    //TODO: bring in all astro controls

    private bool controlsEnabled = true;
    public event System.Action ControlsEnabledChanged = delegate { };
    public bool ControlsEnabled
    {
        get => controlsEnabled;
        set
        {
            bool prevVal = controlsEnabled;
            controlsEnabled = value;
            if (prevVal != value)
            {
                ControlsEnabledChanged();
                string logText = value ? "Astro controls enabled." : "Astro controls disabled.";
                Debug.Log(logText);
            }
        }
    }
}
