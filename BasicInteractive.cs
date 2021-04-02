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
    private InteractiveActionWrapper astroEnterAction;
    [SerializeField]
    private InteractiveActionWrapper astroExitAction;
    [SerializeField]
    private InteractiveActionWrapper succesfulInteractAction;
    [SerializeField]
    private InteractiveActionWrapper failedInteractAction;
    [SerializeField]
    protected List<SO_BoolArgument> interactArguments;
    [SerializeField]
    protected List<SO_Reaction> interactReactions;
    [SerializeField]
    private bool resetInteractOnExit = false;

    protected bool Interacted { get; private set; } = false;

    protected virtual void Awake()
    {
        astroEnterAction?.OnAwake();
        astroExitAction?.OnAwake();
        succesfulInteractAction?.OnAwake();
        failedInteractAction?.OnAwake();
    }

    //TODO: add an interact queue system in astro like the AstroText system?
    //Maybe combine them since they go hand in hand?

    //TODO: implement adding to astro text system
    protected override void OnAstroEnter(GameObject astroGO)
    {
        if (!Interacted)
        {
            TryAction(astroEnterAction);
        }
        else
        {
            TryAction(succesfulInteractAction);
        }

        //could and should also work in OnAstroFocus
        //S_AstroInteractiveQueue.Current.QueueInteractiveText(this, astroEnterText);
    }

    protected override void OnAstroExit(GameObject astroGO)
    {
        TryAction(astroExitAction);
        if (resetInteractOnExit)
        {
            Interacted = false;
        }
    }

    //Note ^^^ are protected and only for things that inherit this
    //vvv these are meant to be called by the astro player script
    public override void OnInteract()
    {
        if (Interacted)
        {
            return;
        }

        if (AllInteractArgumentsTrue())
        {
            Interacted = true;
            TryAction(succesfulInteractAction);
            ExecuteAllInteractReactions();
            OnSuccessfulInteract();
        }
        else
        {
            TryAction(failedInteractAction);
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

    protected void TryAction(InteractiveActionWrapper iaw)
    {
        if (iaw == null)
        {
            return;
        }

        iaw.AnimTextCont?.AT?.StartAnimBasedOnAnchor(this);
        //execute sound event
    }

    public override void OnAstroFocus()
    {
    }

    public override void OnReleaseInteract()
    {
    }
}
