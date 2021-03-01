using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    private DoorArgument doorArgument;
    private BoxCollider2D closedColl;
    //TODO: custom closedCollider, one way doors...
    //TODO: inherit from Interactive class

    private void Awake()
    {
        closedColl = GetComponentInChildren<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ASTRO"))
        {
            if (doorArgument == null || doorArgument.Argument())
            {
                //@Sean: play door open sound here
                closedColl.enabled = false;
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("ASTRO"))
        {
            //@Sean: play door closed sound here
            closedColl.enabled = true;
        }
    }

    public abstract class DoorArgument : ScriptableObject
    {
        public abstract bool Argument();
    }
}
