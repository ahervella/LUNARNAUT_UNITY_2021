using UnityEngine;

[CreateAssetMenu(fileName = "SO_BA_InPast", menuName = "ScriptableObjects/BoolArguments/InPast")]
public class SO_BA_InPast : SO_BoolArgument
{
    public override bool IsTrue()
    {
        return S_Global.Current.IN_PAST;
    }
}
