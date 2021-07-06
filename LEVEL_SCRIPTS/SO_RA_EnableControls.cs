using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_EnableControls", menuName = "ScriptableObjects/Reactions/EnableControls")]
public class SO_RA_EnableControls : SO_Reaction
{
    public override void Execute()
    {
        S_InputManager.Current.ControlsEnabled = true;
    }
}
