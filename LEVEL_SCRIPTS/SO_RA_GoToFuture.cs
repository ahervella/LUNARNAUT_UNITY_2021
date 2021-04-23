using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_GoToFuture", menuName = "ScriptableObjects/Reactions/GoToFuture")]
public class SO_RA_GoToFuture : SO_Reaction
{
    public override void Execute()
    {
        Debug.Log("Attempting to time traveling to the future...");
        S_TimeTravel.Current.Timeline = S_TimeTravel.TIME_PERIOD.FUTURE;
    }
}
