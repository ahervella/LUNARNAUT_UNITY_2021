using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_BA_AND", menuName = "ScriptableObjects/BoolArguments/SO_BA_AND")]
public class SO_BA_AND : SO_BoolArgument
{
    [SerializeField]
    private List<SO_BoolArgument> andArguments;

    public override bool IsTrue()
    {
        foreach (SO_BoolArgument ba in andArguments)
        {
            if (!ba.IsTrue())
            {
                return false;
            }
        }
        return true;
    }
}
