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
            OnAstroEnter();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(ASTRO_TAG))
        {
            OnAstroExit();
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
    /// When astro player presses the interactive button (one frame)
    /// </summary>
    public abstract void OnInteract();

    /// <summary>
    /// When astro player releases the interactive button (one frame)
    /// </summary>
    public abstract void OnReleaseInteract();
}
