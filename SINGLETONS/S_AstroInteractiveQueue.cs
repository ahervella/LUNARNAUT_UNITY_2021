using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "S_AstroInteractiveQueue", menuName = "ScriptableObjects/Singletons/AstroInteractiveQueue")]
public class S_AstroInteractiveQueue : Singleton<S_AstroInteractiveQueue>
{
    public enum TEXT_ORIENTATION { LEFT, RIGHT, ASTRO_FRONT, ASTRO_BEHIND, CUSTOM }

    private Dictionary<A_Interactive, SO_AnimatedTextTemplate> ATQueue = new Dictionary<A_Interactive, SO_AnimatedTextTemplate>();
    private Coroutine removingCR;


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

    public void QueueInteractiveText(A_Interactive interactiveOwner, SO_AnimatedTextTemplate at, Vector2 customRelativeOffset = new Vector2())
    {
        if (!ATQueue.ContainsKey(interactiveOwner))
        {
            Debug.LogError(string.Format("Astro Interactive Queue is not in the queue the key {0}, no bueno!", interactiveOwner));
            return;
        }

        
        //handy if we want to immediately remove the text for whatever reason
        if (at == null)
        {
            //ATQueue[interactiveOwner].StopAndClearAnim(deanimate: false);
            ATQueue[interactiveOwner] = null;
            return;
        }

        ATQueue[interactiveOwner] = at;
    }


    public void RemoveInteractive(A_Interactive interactiveOwner)
    {
        if (!ATQueue.ContainsKey(interactiveOwner))
        {
            return;
        }

        //If we are currently deanimating something that was just removed,
        //let that play out and just remove this without deanimating
        if (removingCR != null || ATQueue[interactiveOwner] == null)
        {
            ATQueue.Remove(interactiveOwner);
            return;
        }

        removingCR = StartCoroutine(StartRemovalCoroutine(interactiveOwner));
    }

    private IEnumerator StartRemovalCoroutine(A_Interactive interactiveOwner)
    {
        //float deanimateTime = ATQueue[interactiveOwner].StopAndClearAnim(deanimate: true);
        ATQueue.Remove(interactiveOwner);
        //yield return new WaitForSeconds(deanimateTime);

        //play the next one in queue if any
        if (ATQueue.Count > 0)
        {
            ATQueue.First().Key.OnAstroFocus();
            //ATQueue.First().Value.StartAnim();
        }
        removingCR = null;
        yield break;
    }
}
