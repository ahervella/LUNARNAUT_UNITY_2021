using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_GoToFuture", menuName = "ScriptableObjects/Reactions/GoToFuture")]
public class SO_RA_GoToFuture : SO_Reaction
{
    public override void Execute()
    {
        if (!S_Global.Current.IN_PAST)
        {
            Debug.Log("Tried to travel to the future, but we are here, in the future!");
            return;
        }
        S_Global.Current.IN_PAST = false;
        Debug.Log("Time traveling to the future...");
    }
}
