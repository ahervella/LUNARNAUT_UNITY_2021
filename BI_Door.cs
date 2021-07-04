using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BI_Door : BasicInteractive
{
    private enum DOOR_ENTRANCE { BOTH, ENTER_RIGHT_SIDE_ONLY, ENTER_LEFT_SIDE_ONLY}

    [Header("Door")]
    [SerializeField]
    private bool automaticDoor = false;
    [SerializeField]
    private bool autoDoorAfterInteract = false;
    [SerializeField]
    private bool needInteractArgsForAuto = true;


    //TODO: split trigger into two somehow to be visible to editor?
    [SerializeField]
    private DOOR_ENTRANCE oneWayBehavior = DOOR_ENTRANCE.BOTH;
    [SerializeField]
    private BoxCollider2D closedColl;

    //TODO: custom closedCollider, one way doors...
    //TODO: implement airlock logic

    [SerializeField]
    private AK.Wwise.Event doorOpenEvent;

    private bool astroOnRightSide;

    protected override void OnAstroEnter(GameObject astroGO)
    {
        astroOnRightSide = astroGO.transform.position.x > transform.position.x;

        if (OneWayRestriction())
        {
            return;
        }

        base.OnAstroEnter(astroGO);
        if ((automaticDoor || (autoDoorAfterInteract && Interacted)) && (AllInteractArgumentsTrue() || !needInteractArgsForAuto))
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
            case DOOR_ENTRANCE.ENTER_LEFT_SIDE_ONLY:
                return astroOnRightSide;
            case DOOR_ENTRANCE.ENTER_RIGHT_SIDE_ONLY:
                return !astroOnRightSide;
            default:
                return false;
        }
    }

    public override void OnInteract()
    {
        if (OneWayRestriction())
        {
            return;
        }
        base.OnInteract();
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

        doorOpenEvent.Post(Audio3DSource);

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
