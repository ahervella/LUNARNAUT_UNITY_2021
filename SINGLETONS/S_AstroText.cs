using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_AstroText : Singleton<S_AstroText>
{
    public enum TEXT_ORIENTATION { LEFT, RIGHT, ASTRO_FRONT, ASTRO_BEHIND, CUSTOM }

    [SerializeField]
    private Vector2 rightOrientationOffset;
    [SerializeField]
    private Vector2 leftOrientationOffset;

    private List<SO_AnimatedText> ATQueue = new List<SO_AnimatedText>();
    private Coroutine removingCR;

    public void AddTextToQueue(SO_AnimatedText newAT, TEXT_ORIENTATION textOrientation, Vector2 customRelativeOffset = new Vector2())
    {
        switch (textOrientation)
        {
            case TEXT_ORIENTATION.RIGHT:
                newAT.ChangeAnchor(SO_AnimatedText.AT_ANCHOR.LOCAL_POS, rightOrientationOffset);
                break;

            case TEXT_ORIENTATION.LEFT:
                newAT.ChangeAnchor(SO_AnimatedText.AT_ANCHOR.LOCAL_POS, leftOrientationOffset);
                break;

            case TEXT_ORIENTATION.ASTRO_FRONT:
                //TODO: determine if facing right or left
                newAT.ChangeAnchor(SO_AnimatedText.AT_ANCHOR.LOCAL_POS, rightOrientationOffset);
                break;

            case TEXT_ORIENTATION.ASTRO_BEHIND:
                //TODO: determine if facing right or left
                newAT.ChangeAnchor(SO_AnimatedText.AT_ANCHOR.LOCAL_POS, rightOrientationOffset);
                break;

            case TEXT_ORIENTATION.CUSTOM:
                newAT.ChangeAnchor(SO_AnimatedText.AT_ANCHOR.LOCAL_POS, customRelativeOffset);
                break;
        }

        if (ATQueue.Count > 0)
        {
            ATQueue.Add(newAT);
            return;
        }
        newAT.StartAnim();
    }

    public void RemoveTextFromQueue(SO_AnimatedText removingAT)
    {
        //If we are currently deanimating something that was just removed,
        //let that play out and just remove this without deanimating
        if (removingCR != null)
        {
            ATQueue.Remove(removingAT);
            return;
        }
        removingCR = StartCoroutine(StartRemovalCoroutine(removingAT));
    }

    private IEnumerator StartRemovalCoroutine(SO_AnimatedText removingAT)
    {
        //try to remove it, and if this was found in the queue...
        if (ATQueue.Remove(removingAT))
        {
            float deanimateTime = removingAT.StopAndClearAnim(deanimate: true);
            yield return new WaitForSeconds(deanimateTime);
        }

        //play the next one in queue if any
        if (ATQueue.Count > 0)
        {
            ATQueue[0].StartAnim();
        }
        removingCR = null;
    }
}
