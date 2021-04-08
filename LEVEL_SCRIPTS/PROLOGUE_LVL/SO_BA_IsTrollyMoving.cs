using UnityEngine;

[CreateAssetMenu(fileName = "SO_BA_IsTrollyMoving", menuName = "ScriptableObjects/BoolArguments/PrologueLvl/IsTrollyMoving")]
public class SO_BA_IsTrollyMoving : SO_BoolArgument
{
    public override bool IsTrue()
    {
        return S_Global.Current.PrologueLvl.TROLLY_MOVING;
    }
}
