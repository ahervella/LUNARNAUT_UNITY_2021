using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_ActivateBrainLab", menuName = "ScriptableObjects/Reactions/PrologueLvl/ActivateBrainLab")]
public class SO_RA_ActivateBrainLab : SO_Reaction
{
    public override void Execute()
    {
        S_Global.Current.PrologueLvl.BRAIN_LAB_ACTIVATED = true;
        Debug.Log("Brain Lab Activated!");
    }
}