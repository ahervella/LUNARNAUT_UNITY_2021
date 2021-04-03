using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BI_Door : BasicInteractive
{
    private enum DOOR_ENTRANCE { BOTH, RIGHT_ONLY, LEFT_ONLY}

    [Header("Door")]
    [SerializeField]
    private bool automaticDoor = false;
    [SerializeField]
    private bool autoDoorAfterInteract = false;

    //TODO: split trigger into two somehow to be visible to editor?
    [SerializeField]
    private DOOR_ENTRANCE oneWayBehavior = DOOR_ENTRANCE.BOTH;

    private BoxCollider2D closedColl;
    //TODO: custom closedCollider, one way doors...
    //TODO: implement airlock logic

    [SerializeField]
    private AK.Wwise.Event doorOpenEvent;

    private bool astroOnRightSide;

    protected override void Awake()
    {
        base.Awake();
        closedColl = GetComponentInChildren<BoxCollider2D>();
    }

    protected override void OnAstroEnter(GameObject astroGO)
    {
        astroOnRightSide = astroGO.transform.position.x > transform.position.x;

        if (OneWayRestriction())
        {
            return;
        }

        base.OnAstroEnter(astroGO);
        if ((automaticDoor || (autoDoorAfterInteract && Interacted)) && AllInteractArgumentsTrue())
        {
            OpenDoor();
        }
    }

    private bool OneWayRestriction()
    {
        switch (oneWayBehavior)
        {
            case DOOR_ENTRANCE.BOTH:
                return false;
            case DOOR_ENTRANCE.LEFT_ONLY:
                return !astroOnRightSide;
            case DOOR_ENTRANCE.RIGHT_ONLY:
                return astroOnRightSide;
            default:
                return false;
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

    private void OpenDoor()
    {
        //nevermind if already open
        if (!closedColl.enabled)
        {
            return;
        }

        doorOpenEvent.Post(gameObject);

        closedColl.enabled = false;
    }



    protected override void OnAstroExit(GameObject astroGO)
    {
        base.OnAstroExit(astroGO);
        CloseDoor();
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
