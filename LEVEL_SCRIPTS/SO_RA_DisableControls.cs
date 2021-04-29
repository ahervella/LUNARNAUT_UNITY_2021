using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_DisableControls", menuName = "ScriptableObjects/Reactions/DisableControls")]
public class SO_RA_DisableControls : SO_Reaction
{
    public override void Execute()
    {
        S_AstroInputManager.Current.ControlsEnabled = false;
    }
}