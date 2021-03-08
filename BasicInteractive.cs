using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Collider2D))]
public abstract class BasicInteractive : A_Interactive
{
    //TODO: find a smart way to group animated texts so that we can share
    //the textgameobject where ever it makes sense
    [SerializeField]
    private SO_AnimatedText astroEnterText;
    private SO_AnimatedText astroExitText;
    private SO_AnimatedText interactText;
    private SO_AnimatedText releaseInteractText;

    //TODO: add an interact queue system in astro like the AstroText system?
    //Maybe combine them since they go hand in hand?

    //TODO: implement adding to astro text system
    protected override void OnAstroEnter()
    {
        //could and should also work in OnAstroFocus
        S_AstroInteractiveQueue.Current.QueueInteractiveText(this, astroEnterText);
    }

    protected override void OnAstroExit()
    {
        S_AstroInteractiveQueue.Current.QueueInteractiveText(this, astroExitText);
    }

    //Note ^^^ are protected and only for things that inherit this
    //vvv these are meant to be called by the astro player script
    public override void OnInteract()
    {
        S_AstroInteractiveQueue.Current.QueueInteractiveText(this, interactText);
    }

    public override void OnReleaseInteract()
    {
        S_AstroInteractiveQueue.Current.QueueInteractiveText(this, releaseInteractText);
    }
}
