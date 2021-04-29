using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_MarkTrollyStopped", menuName = "ScriptableObjects/Reactions/PrologueLvl/MarkTrollyStopped")]
public class SO_RA_MarkTrollyStopped : SO_Reaction
{
    public override void Execute()
    {
        S_Global.Current.PrologueLvl.TROLLY_MOVING = false;
        Debug.Log("Trolly Stopped Moving.");
    }
}