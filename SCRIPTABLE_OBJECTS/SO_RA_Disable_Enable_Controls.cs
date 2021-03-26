using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_Disable_Enable_Controls", menuName = "ScriptableObjects/Reactions")]
public class SO_RA_Disable_Enable_Controls : SO_Reaction
{
    private enum CONTROLS_ON_OFF { ENABLE, DISABLE }

    [SerializeField]
    private CONTROLS_ON_OFF controlsState;

    public override void Execute()
    {
        if ( controlsState == CONTROLS_ON_OFF.ENABLE)
        {
            //TODO: make input manager and enable controls
        }
        else
        {
            //TODO: make input manager and disable controls
        }
    }

}
