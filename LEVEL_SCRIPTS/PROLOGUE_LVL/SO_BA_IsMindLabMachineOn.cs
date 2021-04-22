using UnityEngine;

[CreateAssetMenu(fileName = "SO_BA_IsMindLabMachineOn", menuName = "ScriptableObjects/BoolArguments/PrologueLvl/IsMindLabMachineOn")]
public class SO_BA_IsMindLabMachineOn : SO_BoolArgument
{
    public override bool IsTrue()
    {
        return S_Global.Current.PrologueLvl.MIND_LAB_MACHINE_ON;
    }
}
