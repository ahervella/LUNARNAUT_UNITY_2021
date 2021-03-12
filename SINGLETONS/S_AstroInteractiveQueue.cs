using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class S_AstroInteractiveQueue : Singleton<S_AstroInteractiveQueue>
{
    public enum TEXT_ORIENTATION { LEFT, RIGHT, ASTRO_FRONT, ASTRO_BEHIND, CUSTOM }

    [SerializeField]
    private Vector2 rightOrientationOffset;
    [SerializeField]
    private Vector2 leftOrientationOffset;
    private Dictionary<A_Interactive, SO_AnimatedText> ATQueue = new Dictionary<A_Interactive, SO_AnimatedText>();
    //private List<SO_AnimatedText> ATQueue = new List<SO_AnimatedText>();
    private Coroutine removingCR;

    /*
    public class S_AstroTextArgs
    {
        S_AstroTextArgs(SO_AnimatedText text, TEXT_ORIENTATION orientation)
        {
            this.text = text;
            this.orientation = orientation;
        }

        public SO_AnimatedText text { get; private set; }
        public TEXT_ORIENTATION orientation { get; private set; }
        public Vector2 customRelativeOffset { get; private set; } = new Vector2();
    }*/

    /// <summary>
    /// Used to add an interactive that has no text, in case there is still behavior that executes on focus
    /// </summary>
    /// <param name="interactive"></param>
    public void AddInteractive(A_Interactive interactive)
    {
        if (ATQueue.ContainsKey(interactive))
        {
            Debug.LogError(string.Format("Astro Interactive Queue already has the key {0}, no bueno!", interactive));
            return;
        }

        ATQueue.Add(interactive, null);
        if (ATQueue.Count == 1)
        {
            interactive.OnAstroFocus();
        }
    }

    public void QueueInteractiveText(A_Interactive interactiveOwner, SO_AnimatedText at, Vector2 customRelativeOffset = new Vector2())
    {
        if (!ATQueue.ContainsKey(interactiveOwner))
        {
            Debug.LogError(string.Format("Astro Interactive Queue is not in the queue the key {0}, no bueno!", interactiveOwner));
            return;
        }

        //handy if we want to immediately remove the text for whatever reason
        if (at == null)
        {
            ATQueue[interactiveOwner].StopAndClearAnim(deanimate: false);
            ATQueue[interactiveOwner] = null;
            return;
        }

        switch (at.CurrAnchor)
        {
            case SO_AnimatedText.AT_ANCHOR.ASTRO_RIGHT:
                at.AdjustLocalPos(rightOrientationOffset);
                break;

            case SO_AnimatedText.AT_ANCHOR.ASTRO_LEFT:
                at.AdjustLocalPos(leftOrientationOffset);
                break;

            case SO_AnimatedText.AT_ANCHOR.ASTRO_FRONT:
                //TODO: determine if facing right or left
                at.AdjustLocalPos(rightOrientationOffset);
                break;

            case SO_AnimatedText.AT_ANCHOR.ASTRO_BEHIND:
                //TODO: determine if facing right or left
                at.AdjustLocalPos(rightOrientationOffset);
                break;

            case SO_AnimatedText.AT_ANCHOR.ASTRO_CUSTOM:
                at.AdjustLocalPos(customRelativeOffset);
                break;

            default:
                Debug.LogError(string.Format("QueuedInteractiveText does not have an Astro anchor for text {0} on interactiveOwner {1}", at.CurrText, interactiveOwner));
                return;
        }

        if (ATQueue.First().Key == interactiveOwner)
        {
            //new text for the same interactive so we do not deanimate
            ATQueue[interactiveOwner].StopAndClearAnim(deanimate: false);
            at.StartAnim();
        }


        ATQueue[interactiveOwner] = at;
    }


    public void RemoveInteractive(A_Interactive interactiveOwner)
    {
        //If we are currently deanimating something that was just removed,
        //let that play out and just remove this without deanimating
        if (removingCR != null)
        {
            ATQueue.Remove(interactiveOwner);
            return;
        }

        if (ATQueue.ContainsKey(interactiveOwner))
        {
            removingCR = StartCoroutine(StartRemovalCoroutine(interactiveOwner));
        }
    }

    private IEnumerator StartRemovalCoroutine(A_Interactive interactiveOwner)
    {
        float deanimateTime = ATQueue[interactiveOwner].StopAndClearAnim(deanimate: true);
        ATQueue.Remove(interactiveOwner);
        yield return new WaitForSeconds(deanimateTime);

        //play the next one in queue if any
        if (ATQueue.Count > 0)
        {
            ATQueue.First().Key.OnAstroFocus();
            ATQueue.First().Value.StartAnim();
        }
        removingCR = null;
    }
}
