using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_PowerOnMindLab", menuName = "ScriptableObjects/Reactions/PrologueLvl/PowerOnMindLab")]
public class SO_RA_PowerOnMindLab : SO_Reaction
{
    public override void Execute()
    {
        S_Global.Current.PrologueLvl.MIND_LAB_POWER_ON = true;
        Debug.Log("Mind Lab Powered On");
    }
}