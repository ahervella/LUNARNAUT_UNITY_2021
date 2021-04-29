using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_UnlockFutureElevatorRoom", menuName = "ScriptableObjects/Reactions/PrologueLvl/UnlockFutureElevatorRoom")]
public class SO_RA_UnlockFutureElevatorRoom : SO_Reaction
{
    public override void Execute()
    {
        S_Global.Current.PrologueLvl.ELEVATOR_ROOM_UNLOCKED = true;
        Debug.Log("Unlocked future elevator room.");
    }
}
