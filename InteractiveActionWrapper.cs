using System;
using System.Collections;
using System.Collections.Generic;
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

    [Serializable]
    public class EmitterContainer
    {
        [SerializeField]
        private EmitterSource otherEmitter;
        public EmitterSource OtherEmitter => otherEmitter;

        [SerializeField]
        private float soundDelay = 0;
        public float SoundDelay => soundDelay;
    }

    [SerializeField]
    private List<EmitterContainer> emitterContainers;
    public List<EmitterContainer> EmitterContainers => emitterContainers;

    public void OnAwake()
    {
        AnimTextCont.OnAwake();
    }

    public void TrySoundEvent(A_Interactive aint)
    {
        if (SoundEvent.Name != "" && aint.enabled)
        {
            if (aint.Audio3DSource == null)
            {
                aint.StartCoroutine(DelayPlaySound(soundDelay, SoundEvent, aint.gameObject));
            }
            else
            {
                aint.StartCoroutine(DelayPlaySound(soundDelay, SoundEvent, aint.Audio3DSource));
            }
        }
    }

    public void TryOtherEmitters(MonoBehaviour sourceObjMono)
    {
        if (emitc.OtherEmitter.SoundEvent.Name != "" && sourceObjMono.enabled)
        {
            sourceObjMono.StartCoroutine(DelayPlaySound(emitc.SoundDelay, emitc.OtherEmitter.SoundEvent, emitc.OtherEmitter.gameObject));
        }
    }

    public void TryTextAnim(A_Interactive aint)
    {
        AnimTextCont?.AT?.StartAnimBasedOnAnchor(aint);
    }

    public void TryCompleteAction(A_Interactive aint)
    {
        TrySoundEvent(aint);
        TryOtherEmitters(aint);
        TryTextAnim(aint);
    }

    protected IEnumerator DelayPlaySound(float delay, AK.Wwise.Event soundEvent, GameObject soundObject)
    {
        yield return new WaitForSeconds(delay);

        if (soundObject == null)
        {
            Debug.LogError(String.Format("No sound source obj specified for: {0} from class: {1}", soundEvent, this));
        }

        soundEvent.Post(soundObject);
    }
}