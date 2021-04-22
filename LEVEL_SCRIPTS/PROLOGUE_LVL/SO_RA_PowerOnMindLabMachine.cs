using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_PowerOnMindLabMachine", menuName = "ScriptableObjects/Reactions/PrologueLvl/PowerOnMindLabMachine")]
public class SO_RA_PowerOnMindLabMachine : SO_Reaction
{
    public override void Execute()
    {
        S_Global.Current.PrologueLvl.MIND_LAB_MACHINE_ON = true;
        Debug.Log("Mind Lab Machine Powered On");
    }
}