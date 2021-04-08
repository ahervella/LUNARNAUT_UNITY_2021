using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_TurnTrollyOn", menuName = "ScriptableObjects/Reactions/PrologueLvl/TurnTrollyOn")]
public class SO_RA_TurnTrollyOn : SO_Reaction
{
    public override void Execute()
    {
        S_Global.Current.PrologueLvl.TROLLY_POWERED_ON = true;
        Debug.Log("Turned on trolly power");
    }
}
