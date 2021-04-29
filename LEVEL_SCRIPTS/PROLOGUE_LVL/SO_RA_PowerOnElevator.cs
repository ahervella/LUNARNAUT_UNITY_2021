using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_PowerOnElevator", menuName = "ScriptableObjects/Reactions/PrologueLvl/PowerOnElevator")]
public class SO_RA_PowerOnElevator : SO_Reaction
{
    public override void Execute()
    {
        S_Global.Current.PrologueLvl.ELEVATOR_ROOM_ELEVATOR_POWERED_ON = true;
        Debug.Log("Elevator room elevator powered on!");
    }
}
