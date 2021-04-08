using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_AstroInputManager : Singleton<S_AstroInputManager>
{
    //TODO: bring in all astro controls

    private bool controlsEnabled = false;
    public event System.Action ControlsEnabledChanged = delegate { };
    public bool ControlsEnabled
    {
        get => controlsEnabled;
        set
        {
            controlsEnabled = value;
            ControlsEnabledChanged();
        }
    }
}
