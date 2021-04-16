﻿using System;
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
        private float delayTime = 0;
        public float DelayTime => delayTime;
    }

    [SerializeField]
    private List<EmitterContainer> emitterContainers;
    public List<EmitterContainer> EmitterContainers => emitterContainers;

    public void OnAwake()
    {
        AnimTextCont.OnAwake();
    }

    public void TrySoundEvent(MonoBehaviour sourceObjMono)
    {
        if (SoundEvent != null)
        {
            sourceObjMono.StartCoroutine(DelayPlaySound(SoundDelay, SoundEvent, sourceObjMono.gameObject));
        }
    }

    public void TryOtherEmitters(MonoBehaviour sourceObjMono)
    {
        if (EmitterContainers != null)
        {
            foreach (EmitterContainer emitc in EmitterContainers)
            {
                sourceObjMono.StartCoroutine(DelayPlaySound(emitc.DelayTime, emitc.OtherEmitter.SoundEvent, emitc.OtherEmitter.gameObject));
            }
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