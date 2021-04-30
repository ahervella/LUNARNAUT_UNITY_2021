using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmitterSource : MonoBehaviour
{
    [SerializeField]
    private AK.Wwise.Event soundEvent;
    public AK.Wwise.Event SoundEvent => soundEvent;

    public void PlaySound()
    {
        soundEvent.Post(gameObject);
    }
    public void StopSound(int transTime)
    {
        soundEvent.Stop(gameObject, transTime);
    }
}
