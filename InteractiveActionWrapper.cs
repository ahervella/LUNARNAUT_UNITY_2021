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

    [SerializeField]
    private List<GameObject> enableList;
    //public List<GameObject> EnableList => enableList;
    [SerializeField]
    private List<GameObject> disableList;
    //public List<GameObject> DisableList => disableList;

    [Serializable]
    public class EmitterContainer
    {
        public enum START_OR_STOP {START, STOP};

        [SerializeField]
        private START_OR_STOP startOrStop = START_OR_STOP.START;
        public START_OR_STOP StartOrStop => startOrStop;

        [SerializeField]
        private EmitterSource otherEmitter;
        public EmitterSource OtherEmitter => otherEmitter;

        [SerializeField]
        private float soundDelay = 0;
        public float SoundDelay => soundDelay;

        [SerializeField]
        private int stopTransTime = 0;
        public int StopTransTime => stopTransTime;
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
        if (!aint.enabled)
        {
            Debug.LogFormat("Tried to play sound on A_Interactive: {0} but it is disabled.", aint.name);
            return;
        }
        if (SoundEvent.Name != "")
        {
            if (aint.Audio3DSource == null)
            {
                aint.StartCoroutine(DelaySoundEvent(aint.gameObject));
            }
            else
            {
                aint.StartCoroutine(DelaySoundEvent(aint.Audio3DSource));
            }
        }
    }

    public void EnableDisableObjects()
    {
        foreach(GameObject obj in enableList)
        {
            obj.SetActive(true);
        }

        foreach (GameObject obj in disableList)
        {
            obj.SetActive(false);
        }
    }

    public void TryOtherEmitters(MonoBehaviour sourceObjMono)
    {
        if (!sourceObjMono.enabled)
        {
            Debug.LogFormat("Tried to play sound on object: {0} but it is disabled.", sourceObjMono.name);
            return;
        }
        foreach (EmitterContainer emitc in EmitterContainers)
        {
            if (emitc.OtherEmitter.SoundEvent.Name == "")
            {
                continue;
            }
            sourceObjMono.StartCoroutine(DelayEmitterEvent(emitc));
        }
    }

    public void TryTextAnim(A_Interactive aint)
    {
        AnimTextCont?.AT?.StartAnimBasedOnAnchor(aint);
    }

    public void TryCompleteAction(A_Interactive aint)
    {
        EnableDisableObjects();
        TrySoundEvent(aint);
        TryOtherEmitters(aint);
        TryTextAnim(aint);
    }

    protected IEnumerator DelaySoundEvent(GameObject soundObject)
    {
        yield return new WaitForSeconds(SoundDelay);

        if (soundObject == null)
        {
            Debug.LogError(String.Format("No sound source obj specified for: {0} from class: {1}", soundEvent, this));
        }

        soundEvent.Post(soundObject);
    }

    protected IEnumerator DelayEmitterEvent(EmitterContainer emitc)
    {
        yield return new WaitForSeconds(emitc.SoundDelay);
        if (emitc.StartOrStop == EmitterContainer.START_OR_STOP.START)
        {
            emitc.OtherEmitter.PlaySound();
            Debug.LogFormat("Started sound on: {0}", emitc.OtherEmitter.gameObject.name);
        }
        else
        {
            emitc.OtherEmitter.StopSound(emitc.StopTransTime);
            Debug.LogFormat("Stopped sound on: {0}", emitc.OtherEmitter.gameObject.name);
        }
    }
}
