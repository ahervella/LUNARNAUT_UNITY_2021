using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvInteractive : A_Interactive
{
    [SerializeField]
    private AK.Wwise.Event enterEnvEvent;
    [SerializeField]
    private AK.Wwise.Event exitEnvEvent;

    [SerializeField]
    private List<EmitterSource> emitterSources;

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
        foreach (EmitterSource esource in emitterSources)
        {
            enterEnvEvent.Post(esource.gameObject);
        }
    }

    protected override void OnAstroExit(GameObject astroGO)
    {
        foreach (EmitterSource esource in emitterSources)
        {
            exitEnvEvent.Post(esource.gameObject);
        }
    }
}
