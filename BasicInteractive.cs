using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Collider2D))]
public class BasicInteractive : A_Interactive
{
    //TODO: find a smart way to group animated texts so that we can share
    //the textgameobject where ever it makes sense
    [Header("Basic Interactive")]
    [SerializeField]
    private AnimatedTextContainer astroEnterText;
    [SerializeField]
    private AnimatedTextContainer astroExitText;
    [SerializeField]
    private AnimatedTextContainer succesfulInteractText;
    [SerializeField]
    private AnimatedTextContainer failedInteractText;
    [SerializeField]
    protected List<SO_BoolArgument> interactArguments;
    [SerializeField]
    protected List<SO_Reaction> interactReactions;
    [SerializeField]
    private bool resetInteractOnExit = false;
    
    private bool interacted = false;

    //TODO: add an interact queue system in astro like the AstroText system?
    //Maybe combine them since they go hand in hand?

    //TODO: implement adding to astro text system
    protected override void OnAstroEnter()
    {
        if (!interacted)
        {
            TryAnimateText(astroEnterText.AT);
        }
        else
        {
            TryAnimateText(succesfulInteractText.AT);
        }

        //could and should also work in OnAstroFocus
        //S_AstroInteractiveQueue.Current.QueueInteractiveText(this, astroEnterText);
    }

    protected override void OnAstroExit()
    {
        TryAnimateText(astroExitText.AT);
        if (resetInteractOnExit)
        {
            interacted = false;
        }
    }

    //Note ^^^ are protected and only for things that inherit this
    //vvv these are meant to be called by the astro player script
    public override void OnInteract()
    {
        if (interacted)
        {
            return;
        }

        if (AllInteractArgumentsTrue())
        {
            interacted = true;
            TryAnimateText(succesfulInteractText.AT);
            ExecuteAllInteractReactions();
            OnSuccessfulInteract();
        }
        else
        {
            TryAnimateText(failedInteractText.AT);
            OnAllInteractArgumentsFalse();
        }
    }

    protected virtual void OnSuccessfulInteract() { }

    protected virtual void OnAllInteractArgumentsFalse() { }

    protected bool AllInteractArgumentsTrue()
    {
        if (interactArguments == null)
        {
            return true;
        }

        foreach (SO_BoolArgument arg in interactArguments)
        {
            if (!arg.IsTrue())
            {
                return false;
            }
        }

        return true;
    }

    protected void ExecuteAllInteractReactions()
    {
        if (interactReactions == null)
        {
            return;
        }

        foreach (SO_Reaction reaction in interactReactions)
        {
            reaction.Execute();
        }
    }

    protected void TryAnimateText(SO_AnimatedText at)
    {
        if (at != null)
        {
            at.StartAnimBasedOnAnchor(this);
        }
    }

    public override void OnAstroFocus()
    {
    }

    public override void OnReleaseInteract()
    {
    }
}
