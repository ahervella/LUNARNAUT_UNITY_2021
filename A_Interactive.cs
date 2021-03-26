using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class A_Interactive : MonoBehaviour
{
    private const string ASTRO_TAG = "ASTRO";
    private const KeyCode INTERACT_KEY = KeyCode.E;
    private bool astroInArea = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(ASTRO_TAG))
        {
            //This order is important for the interactive system work properly
            //if we are queueing texts in OnAstroEnter
            S_AstroInteractiveQueue.Current.AddInteractive(this);
            OnAstroEnter(collision.gameObject);
            astroInArea = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(ASTRO_TAG))
        {
            OnAstroExit(collision.gameObject);
            S_AstroInteractiveQueue.Current.RemoveInteractive(this);
            astroInArea = false;

            //Trigger on release if we left area while holding down key
            //TODO: will this possibly trigger two OnRealses
            if (Input.GetKey(INTERACT_KEY) || Input.GetKeyDown(INTERACT_KEY))
            {
                OnReleaseInteract();
            }
        }
    }

    protected virtual void Update()
    {
        if (!astroInArea)
        {
            return;
        }

        if (Input.GetKeyDown(INTERACT_KEY))
        {
            OnInteract();
        }

        if (Input.GetKeyUp(INTERACT_KEY))
        {
            OnReleaseInteract();
        }
    }

    /// <summary>
    /// When astro player enters this interactive's area
    /// </summary>
    protected abstract void OnAstroEnter(GameObject astroGO);

    /// <summary>
    /// When astro player leaves this interactive's area
    /// </summary>
    protected abstract void OnAstroExit(GameObject astroGO);

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
