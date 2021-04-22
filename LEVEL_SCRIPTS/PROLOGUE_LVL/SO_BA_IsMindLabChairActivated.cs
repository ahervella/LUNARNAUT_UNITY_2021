using UnityEngine;

[CreateAssetMenu(fileName = "SO_BA_IsMindLabChairActivated", menuName = "ScriptableObjects/BoolArguments/PrologueLvl/IsMindLabChairActivated")]
public class SO_BA_IsMindLabChairActivated : SO_BoolArgument
{
    public override bool IsTrue()
    {
        //
        return S_Global.Current.PrologueLvl.MIND_LAB_CHAIR_ACTIVATED;
    }
}
