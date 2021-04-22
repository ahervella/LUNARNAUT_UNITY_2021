using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvInteractive : A_Interactive
{
    [Header("Roomtone")]
    [SerializeField]
    private AK.Wwise.Event enterEvent = default;
    [SerializeField]
    private AK.Wwise.Event exitEvent = default;
    [SerializeField]
    private AK.Wwise.Event pressureEvent = default;

    [Header("Emitter Logic")]
    [SerializeField]
    private AK.Wwise.Event astroInRoomEvent = default;
    [SerializeField]
    private AK.Wwise.Event astroOutRoomEvent = default;

    [Serializable]
    private class EmitterSourceListElement
    {
        [SerializeField]
        private EmitterSource emitterSource = default;

        public EmitterSource EmitterSource => emitterSource;

        [SerializeField]
        private bool playOnStart = false; //default

        public bool PlayOnStart => playOnStart;
    }

    [SerializeField]
    private List<EmitterSourceListElement> emitterSourceList;

    public override void OnAstroFocus()
    {

    }

    public override void OnInteract()
    {

    }

    public override void OnReleaseInteract()
    {

    }

    protected override void OnAstroEnter(GameObject astroGO)
    {
        if (enterEvent.Name != "")
        {
            enterEvent.Post(gameObject);
        }
        else
        {
            Debug.LogErrorFormat("Wwise Environment ' {0} ' had no enterEvent specified", gameObject.name);
        }

        if (pressureEvent.Name != "")
        {
            pressureEvent.Post(gameObject);
        }
        else
        {
            Debug.LogFormat("Wwise Environment ' {0} ' had no pressureEvent specified", gameObject.name);
        }

        foreach (EmitterSourceListElement esle in emitterSourceList)
        {
            astroInRoomEvent.Post(esle.EmitterSource.gameObject);
        }
    }

    protected override void OnAstroExit(GameObject astroGO)
    {
        if (exitEvent.Name != "")
        {
            exitEvent.Post(gameObject);
        }
        else
        {
            Debug.LogErrorFormat("Wwise Environment ' {0} ' had no exitEvent specified", gameObject.name);
        }

        foreach (EmitterSourceListElement esle in emitterSourceList)
        {
            astroOutRoomEvent.Post(esle.EmitterSource.gameObject);
        }
    }

    public void Start()
    {
        foreach (EmitterSourceListElement esle in emitterSourceList)
        {
            if (esle.PlayOnStart)
            {
                esle.EmitterSource.PlaySound();
            }
        }
    }
}
