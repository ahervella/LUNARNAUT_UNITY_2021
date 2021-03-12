using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "new_SO_BA_PowerOn", menuName = "ScriptableObjects/BoolArguments", order = 1)]
public class SO_BA_PowerOn : SO_BoolArgument
{
    // Use this for initialization
    public override bool IsTrue()
    {
        return S_LevelArguments.Current.PrologueLvl.TROLLY_POWERED_ON;

        //all the logic for is power on!
    }
}
