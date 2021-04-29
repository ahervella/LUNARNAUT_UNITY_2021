using UnityEngine;

[CreateAssetMenu(fileName = "SO_BA_MaeLogsFound", menuName = "ScriptableObjects/BoolArguments/PrologueLvl/MaeLogsFound")]
public class SO_BA_MaeLogsFound : SO_BoolArgument
{
    public override bool IsTrue()
    {
        return S_Global.Current.PrologueLvl.MAE_LOGS_FOUND;
    }
}