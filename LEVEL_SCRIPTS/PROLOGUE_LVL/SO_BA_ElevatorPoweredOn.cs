using UnityEngine;

[CreateAssetMenu(fileName = "SO_BA_ElevatorPoweredOn", menuName = "ScriptableObjects/BoolArguments/PrologueLvl/ElevatorPoweredOn")]
public class SO_BA_ElevatorPoweredOn : SO_BoolArgument
{
    public override bool IsTrue()
    {
        return S_Global.Current.PrologueLvl.ELEVATOR_ROOM_ELEVATOR_POWERED_ON;
    }
}
