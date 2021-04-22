using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_GoToPast", menuName = "ScriptableObjects/Reactions/GoToPast")]
public class SO_RA_GoToPast : SO_Reaction
{
    public override void Execute()
    {
        if (S_Global.Current.IN_PAST)
        {
            Debug.Log("Tried to travel to the past, but we are here, in the past!");
            return;
        }
        S_Global.Current.IN_PAST = true;
        Debug.Log("Time traveling to the past...");
    }
}
