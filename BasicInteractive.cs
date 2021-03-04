using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Collider2D))]
public abstract class BasicInteractive : A_Interactive
{
    //TODO: find a smart way to group animated texts so that we can share
    //the textgameobject where ever it makes sense
    [SerializeField]
    private SO_AnimatedText onAstroEnterText;
    private SO_AnimatedText onAstroExitText;
    private SO_AnimatedText onInteractText;
    private SO_AnimatedText onReleaseInteractText;

    //TODO: add an interact queue system in astro like the AstroText system?
    //Maybe combine them since they go hand in hand?

    //TODO: implement adding to astro text system
    protected override void OnAstroEnter()
    {
        onAstroEnterText.StartAnim();
    }

    protected override void OnAstroExit()
    {
        onAstroExitText.StartAnim();
    }

    //Note ^^^ are protected and only for things that inherit this
    //vvv these are meant to be called by the astro player script
    public override void OnInteract()
    {
        onInteractText.StartAnim();
    }

    public override void OnReleaseInteract()
    {
        onReleaseInteractText.StartAnim();
    }

}
