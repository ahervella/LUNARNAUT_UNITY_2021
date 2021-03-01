using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Collider2D))]
public class Interactive : MonoBehaviour
{
    private const string ASTRO_TAG = "ASTRO";
    private bool ASTRO_INSIDE = false;
    //protected Collider2D interactiveTrigger;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(ASTRO_TAG))
        {
            ASTRO_INSIDE = true;
            //TODO: display text
            //TODO: turn on update
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(ASTRO_TAG))
        {
            ASTRO_INSIDE = false;
            //TODO: turn off update
        }
    }

    private void Update()
    {
        //TODO: Make system to handle updates in Astro, or move to seperate script to access globaly via singleton?
        if (!ASTRO_INSIDE)
        {
            return;
        }

        if (Input.GetKey(KeyCode.E))
        {
            //@Sean: play interactive sound
            //TODO: activate interactive
        }

    }
}
