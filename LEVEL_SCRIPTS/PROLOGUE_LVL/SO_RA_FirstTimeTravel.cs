using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_FirstTimeTravel", menuName = "ScriptableObjects/Reactions/PrologueLvl/FirstTimeTravel")]
public class SO_RA_FirstTimeTravel : SO_Reaction
{
    public static event Action OnFirstTimeTravel = delegate { };
    public override void Execute()
    {
        Debug.Log("Initiating FirstTimeTravelSequence");
        OnFirstTimeTravel();
    }
}
