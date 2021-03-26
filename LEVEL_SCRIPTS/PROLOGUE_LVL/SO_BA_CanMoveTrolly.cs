using UnityEngine;

[CreateAssetMenu(fileName = "SO_BA_CanMoveTrolly", menuName = "ScriptableObjects/BoolArguments/PrologueLvl/CanMoveTrolly")]
public class SO_BA_CanMoveTrolly : SO_BoolArgument
{
    public override bool IsTrue()
    {
        return S_LevelArguments.Current.PrologueLvl.TROLLY_POWERED_ON;
    }
}
