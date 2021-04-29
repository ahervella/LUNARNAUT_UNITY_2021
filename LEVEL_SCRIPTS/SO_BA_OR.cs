using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_BA_OR", menuName = "ScriptableObjects/BoolArguments/SO_BA_OR")]
public class SO_BA_OR : SO_BoolArgument
{
    [SerializeField]
    private List<SO_BoolArgument> orArguments;

    public override bool IsTrue()
    {
        foreach(SO_BoolArgument ba in orArguments)
        {
            if (ba.IsTrue())
            {
                return true;
            }
        }
        return false;
    }
}