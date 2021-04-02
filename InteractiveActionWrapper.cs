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

    public void OnAwake()
    {
        AnimTextCont.OnAwake();
    }
}
