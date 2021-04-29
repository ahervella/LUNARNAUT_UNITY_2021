using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_EnablePlayerTimeTraveling", menuName = "ScriptableObjects/Reactions/EnablePlayerTimeTraveling")]
public class SO_RA_EnablePlayerTimeTraveling : SO_Reaction
{
    public override void Execute()
    {
        Debug.Log("Enabled Player Time Traveling");
        S_TimeTravel.Current.PlayerTimeTravelEnabled = true;
    }
}