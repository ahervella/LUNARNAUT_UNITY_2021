using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_MarkTrollyMoving", menuName = "ScriptableObjects/Reactions/PrologueLvl/MarkTrollyMoving")]
public class SO_RA_MarkTrollyMoving : SO_Reaction
{
    public override void Execute()
    {
        S_Global.Current.PrologueLvl.TROLLY_MOVING = true;
        Debug.Log("Trolly Started Moving.");
    }
}