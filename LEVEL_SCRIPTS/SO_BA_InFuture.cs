using UnityEngine;

[CreateAssetMenu(fileName = "SO_BA_InFuture", menuName = "ScriptableObjects/BoolArguments/InFuture")]
public class SO_BA_InFuture : SO_BoolArgument
{
    public override bool IsTrue()
    {
        return S_TimeTravel.Current.InFuture();
    }
}
