using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_ActivateMindLabChair", menuName = "ScriptableObjects/Reactions/PrologueLvl/ActivateMindLabChair")]
public class SO_RA_ActivateMindLabChair : SO_Reaction
{
    public override void Execute()
    {
        S_Global.Current.PrologueLvl.MIND_LAB_CHAIR_ACTIVATED = true;
        Debug.Log("Mind Lab Chair Activated (initiating mind swap...)");
    }
}
