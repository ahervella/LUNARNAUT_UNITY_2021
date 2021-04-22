using UnityEngine;

[CreateAssetMenu(fileName = "SO_BA_IsMindLabPowerOn", menuName = "ScriptableObjects/BoolArguments/PrologueLvl/IsMindLabPowerOn")]
public class SO_BA_IsMindLabPowerOn : SO_BoolArgument
{
    public override bool IsTrue()
    {
        return S_Global.Current.PrologueLvl.MIND_LAB_POWER_ON;
    }
}
