using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : BasicInteractive
{
    [SerializeField]
    private bool automaticDoor = false;
    private BoxCollider2D closedColl;
    //TODO: custom closedCollider, one way doors...
    //TODO: implement airlock logic

    private void Awake()
    {
        closedColl = GetComponentInChildren<BoxCollider2D>();
    }

    protected override void OnAstroEnter()
    {
        base.OnAstroEnter();
        if (automaticDoor && AllInteractArgumentsTrue())
        {
            OpenDoor();
        }
    }

    protected override void OnSuccessfulInteract()
    {
        base.OnSuccessfulInteract();
        //door will have already been open
        if (!automaticDoor)
        {
            OpenDoor();
        }
    }

    protected override void OnAstroExit()
    {
        CloseDoor();
    }

    private void OpenDoor()
    {
        //nevermind if already open
        if (!closedColl.enabled)
        {
            return;
        }

        closedColl.enabled = false;
    }

    private void CloseDoor()
    {
        //nevermind if already closed
        if (closedColl.enabled)
        {
            return;
        }
        closedColl.enabled = true;
    }
}
