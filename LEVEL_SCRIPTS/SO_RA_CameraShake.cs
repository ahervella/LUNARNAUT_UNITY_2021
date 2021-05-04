using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_RA_CameraShake", menuName = "ScriptableObjects/Reactions/CameraShake")]
public class SO_RA_CameraShake : SO_Reaction
{
    [SerializeField]
    private AstroCamera.SHAKE shakeType = AstroCamera.SHAKE.MED;
    [SerializeField]
    private float shakeTime = 1f;
    [SerializeField]
    private float delayTime = 0f;
    [SerializeField]
    private float easeInTime = 0.5f;
    [SerializeField]
    private float easeOutTime = 0.5f;

    public static event Action<AstroCamera.SHAKE, float, float, float, float> NewReactionShake = delegate { };

    public override void Execute()
    {
        if (shakeTime <= 0f)
        {
            Debug.LogErrorFormat("SO_RA_CameraShake can not have a shake time less than or equal to zero!");
            return;
        }

        Debug.LogFormat("Started a camera reaction shake of type {0} for {1} seconds.", shakeType, shakeTime);
        NewReactionShake(shakeType, shakeTime, delayTime, easeInTime, easeOutTime);
    }
}
