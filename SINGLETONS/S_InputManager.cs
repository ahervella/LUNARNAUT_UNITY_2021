using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "S_InputManager", menuName = "ScriptableObjects/Singletons/InputManager")]
public class S_InputManager : Singleton<S_InputManager>
{
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
