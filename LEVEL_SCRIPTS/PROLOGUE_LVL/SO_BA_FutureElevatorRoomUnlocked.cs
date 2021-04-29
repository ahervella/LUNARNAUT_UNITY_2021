using UnityEngine;

[CreateAssetMenu(fileName = "SO_BA_FutureElevatorRoomUnlocked", menuName = "ScriptableObjects/BoolArguments/PrologueLvl/FutureElevatorRoomUnlocked")]
public class SO_BA_FutureElevatorRoomUnlocked : SO_BoolArgument
{
    public override bool IsTrue()
    {
        if (S_TimeTravel.Current.InFuture())
        {
            if (S_Global.Current.PrologueLvl.ELEVATOR_ROOM_UNLOCKED)
            {
                return true;
            }
            else
            {
                Debug.Log("Tried to enter elevator room in the future before unlocking it in the past.");
                return false;
            }
        }

        return true;
    }
}