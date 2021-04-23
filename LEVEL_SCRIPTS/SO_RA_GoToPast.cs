using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_GoToPast", menuName = "ScriptableObjects/Reactions/GoToPast")]
public class SO_RA_GoToPast : SO_Reaction
{
    public override void Execute()
    {
        Debug.Log("Attempting to time traveling to the past...");
        S_TimeTravel.Current.Timeline = S_TimeTravel.TIME_PERIOD.PAST;
    }
}
