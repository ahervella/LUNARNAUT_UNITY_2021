using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class A_Interactive : MonoBehaviour
{
    private const string ASTRO_TAG = "ASTRO";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(ASTRO_TAG))
        {
            //This order is important for the interactive system work properly
            //if we are queueing texts in OnAstroEnter
            S_AstroInteractiveQueue.Current.AddInteractive(this);
            OnAstroEnter();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(ASTRO_TAG))
        {
            OnAstroExit();
            S_AstroInteractiveQueue.Current.RemoveInteractive(this);
        }
    }

    /// <summary>
    /// When astro player enters this interactive's area
    /// </summary>
    protected abstract void OnAstroEnter();

    /// <summary>
    /// When astro player leaves this interactive's area
    /// </summary>
    protected abstract void OnAstroExit();

    /// <summary>
    /// When astro gets to this interactive in the Astro Interactive Queuing system
    /// </summary>
    public abstract void OnAstroFocus();

    /// <summary>
    /// When astro player presses the interactive button (one frame)
    /// </summary>
    public abstract void OnInteract();

    /// <summary>
    /// When astro player releases the interactive button (one frame)
    /// </summary>
    public abstract void OnReleaseInteract();
}
