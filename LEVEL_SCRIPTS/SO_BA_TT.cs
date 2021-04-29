using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_BA_TT", menuName = "ScriptableObjects/BoolArguments/SO_BA_TT")]
public class SO_BA_TT : SO_BoolArgument
{
    [SerializeField]
    private List<SO_BoolArgument> futureArguments;

    [SerializeField]
    private List<SO_BoolArgument> pastArguments;

    public override bool IsTrue()
    {
        if (S_TimeTravel.Current.InFuture())
        {
            return AllArgumentsTrue(futureArguments);
        }
        else
        {
            return AllArgumentsTrue(pastArguments);
        }
    }

    public bool AllArgumentsTrue(List<SO_BoolArgument> args)
    {
        foreach(SO_BoolArgument ba in args)
        {
            if (!ba.IsTrue())
            {
                return false;
            }
        }
        return true;
    }
}