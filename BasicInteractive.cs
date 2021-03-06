using System;
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
    protected InteractiveActionWrapper astroEnterAction;
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
    protected List<SO_Reaction> astroEnterReactions;
    [SerializeField]
    protected List<SO_Reaction> astroExitReactions;
    [SerializeField]
    private bool oneTimeInteract = true;
    [SerializeField]
    private bool useCustomInteractTime = false;
    [SerializeField]
    private float customInteractTime = 3f;
    [SerializeField]
    private bool resetInteractOnExit = false;
    [SerializeField]
    private bool onEnterNeedsRequirements = false;
    [SerializeField]
    private bool deanimateTextOnSuccess = false;

    private Coroutine customInteractTimeCR = null;

    private bool interacted = false;
    protected bool Interacted
    {
        get
        {
            if (!oneTimeInteract && !useCustomInteractTime)
            {
                return false;
            }

            return interacted;
        }

        set { interacted = value; }
    }

    protected bool EnteredWasTriggered { get; private set; } = false;

    protected AnimatedText animatedText = null;


    //TODO: add an interact queue system in astro like the AstroText system?
    //Maybe combine them since they go hand in hand?

    //TODO: implement adding to astro text system
    protected override void OnAstroEnter(GameObject astroGO)
    {
        if (onEnterNeedsRequirements && !AllInteractArgumentsTrue())
        {
            return;
        }

        if (!Interacted)
        {
            EnteredWasTriggered = true;
            animatedText = astroEnterAction.TryCompleteAction(this, animatedText);
            ExecuteAllReactions(astroEnterReactions);
        }
        //TODO: else play only text

        /*else
        {
            TryAction(succesfulInteractAction);
        }*/

        //could and should also work in OnAstroFocus
        //S_AstroInteractiveQueue.Current.QueueInteractiveText(this, astroEnterText);
    }

    protected override void OnAstroExit(GameObject astroGO)
    {
        if (!EnteredWasTriggered)
        {
            EnteredWasTriggered = false;
            return;
        }

        animatedText = astroExitAction.TryCompleteAction(this, animatedText);
        DeanimateAllAT();

        if (customInteractTimeCR != null)
        {
            StopCoroutine(customInteractTimeCR);
        }

        ExecuteAllReactions(astroExitReactions);
        if (resetInteractOnExit)
        {
            Interacted = false;
        }
    }

    private void DeanimateAllAT()
    {
        if (animatedText != null)
        {
            animatedText.DeanimateText();
        }
    }

    //Note ^^^ are protected and only for things that inherit this
    //vvv these are meant to be called by the astro player script
    public override void OnInteract()
    {
        Debug.LogFormat("This shit was triggered: {0}", name);
        if (Interacted)
        {
            return;
        }

        if (AllInteractArgumentsTrue())
        {
            Interacted = true;

            AnimateSuccessfulInteractionText();
            ExecuteAllReactions(interactReactions);
            OnSuccessfulInteract();
        }
        else
        {
            animatedText = failedInteractAction.TryCompleteAction(this, animatedText);
            OnUnsuccessfulInteract();
        }
    }

    private void AnimateSuccessfulInteractionText()
    {
        //Want to make sure this only happens if we don't have an on success text coming up (hence last condition in if statement)
        if (deanimateTextOnSuccess && animatedText != null && !succesfulInteractAction.AnimTextCont.CanAnimate())
        {
            animatedText.DeanimateText();
        }

        //^^ Needs to happen before this so that bool for is WasAnimated in ATCont is marked after that check

        animatedText = succesfulInteractAction.TryCompleteAction(this, animatedText);

        if (useCustomInteractTime)
        {
            if (customInteractTimeCR != null)
            {
                StopCoroutine(customInteractTimeCR);
            }
            customInteractTimeCR = StartCoroutine(CustomInteractTimeCR());
        }
    }

    protected virtual void OnSuccessfulInteract()
    {
        
    }

    private IEnumerator CustomInteractTimeCR()
    {
        yield return new WaitForSeconds(customInteractTime);
        Interacted = false;
    }

    protected virtual void OnUnsuccessfulInteract() { }

    protected bool AllInteractArgumentsTrue()
    {
        if (interactArguments == null)
        {
            return true;
        }

        foreach (SO_BoolArgument arg in interactArguments)
        {
            if (arg == null)
            {
                Debug.LogErrorFormat("The interactive {0} has an empty BoolArgument, fix that!", name);
                continue;
            }
            if (!arg.IsTrue())
            {
                return false;
            }
        }

        return true;
    }

    protected void ExecuteAllReactions(List<SO_Reaction> reactions)
    {
        if (reactions == null)
        {
            return;
        }

        foreach (SO_Reaction ra in reactions)
        {
            if (ra == null)
            {
                Debug.LogErrorFormat("The interactive {0} has an empty Reaction, fix that!", name);
                continue;
            }
            ra.Execute();
        }
    }

    public override void OnAstroFocus()
    {
    }

    public override void OnReleaseInteract()
    {
    }
}
