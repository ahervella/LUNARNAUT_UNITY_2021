using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_MarkMaeLogsFound", menuName = "ScriptableObjects/Reactions/PrologueLvl/MarkMaeLogsFound")]
public class SO_RA_MarkMaeLogsFound : SO_Reaction
{
    public override void Execute()
    {
        S_Global.Current.PrologueLvl.MAE_LOGS_FOUND = true;
        Debug.Log("Found Mae's logs.");
    }
}
