using System;
using UnityEngine;

[Serializable]
public class InteractiveActionWrapper
{
    [SerializeField]
    private AnimatedTextContainer animTextCont;
    public AnimatedTextContainer AnimTextCont => animTextCont;

    [SerializeField]
    private AK.Wwise.Event soundEvent;
    public AK.Wwise.Event SoundEvent => soundEvent;

    [SerializeField]
    private float soundDelay;
    public float SoundDelay => soundDelay;

    public void OnAwake()
    {
        AnimTextCont.OnAwake();
    }
}
