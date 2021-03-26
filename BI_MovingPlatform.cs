﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BI_MovingPlatform : BasicInteractive
{
    [Header("Moving Platform")]
    [SerializeField]
    private bool triggerOnSuccessInteract = true;

    [SerializeField]
    private KeyCode devToolsMoveHotKey;

    [SerializeField]
    private Transform movingPlatform;

    [SerializeField]
    private float moveTime = 3f;
    /*
     * implement
    [SerializeField]
    private bool automaticLoop = false;

    [SerializeField]
    private bool orderInitWPByName = false;
    */

    [SerializeField]
    private bool usePlatformPosAsFirstWayPoint = true;

    [SerializeField]
    private List<Transform> initialWayPoints = new List<Transform>();

    private float totalDist;
    private float moveTimeStep;
    private float currTime = 1f;


    private List<Vector3> wayPointsPos = new List<Vector3>();
    private List<float> wayPointsRot = new List<float>();

    private List<WayPointWrapper> wpWrappers = new List<WayPointWrapper>();

    //class is used to have a cached list of the vector 2 version of the pos
    //and the distance along the path from the start
    private class WayPointWrapper
    {
        public Vector2 pos;
        public float rot;
        public float startDist;

        public WayPointWrapper(Vector2 pos, float rot, float startDist)
        {
            this.pos = pos;
            this.rot = rot;
            this.startDist = startDist;
        }
    }


    private int currWPIndex = 0;

    private float currWPSlope;
    private float currWPIntercept;

    private Vector2 currAPoint;
    private Vector2 currBPoint;

    private float currARotation;
    private float currBRotation;


    protected override void Awake()
    {
        base.Awake();

        S_DeveloperTools.Current.DevToolsMovingPlatformsChanged -= S_DeveloperTools_EnabledChanged;
        S_DeveloperTools.Current.DevToolsMovingPlatformsChanged += S_DeveloperTools_EnabledChanged;
        S_DeveloperTools.Current.EnableDevToolsChanged -= S_DeveloperTools_EnabledChanged;
        S_DeveloperTools.Current.EnableDevToolsChanged += S_DeveloperTools_EnabledChanged;

        S_DeveloperTools_EnabledChanged();

        if (usePlatformPosAsFirstWayPoint)
        {
            initialWayPoints.Insert(0, movingPlatform.transform);
        }

        //transforms move as we move the platform, so
        //just cache the points right away
        foreach (Transform wp in initialWayPoints)
        {
            wayPointsPos.Add(wp.position);
            wayPointsRot.Add(wp.rotation.eulerAngles.z);
        }

        if (ErrorCheckForWayPointsSize())
        {
            return;
        }

        totalDist = CalcTotalDist();
        //cache this so we don't calculate per step
        moveTimeStep = Time.fixedDeltaTime / moveTime;
        GetNextWP();


        //MovePlatform();
    }

    private bool DEVTOOLS_movePlatformHotKeyEnabled = false;
    private void S_DeveloperTools_EnabledChanged()
    {
        DEVTOOLS_movePlatformHotKeyEnabled = S_DeveloperTools.Current.DevToolsEnabled_MOVING_PLATFORMS();
    }

    private float CalcTotalDist()
    {
        wpWrappers.Clear();

        float total = 0;
        for(int i = 0; i < wayPointsPos.Count; i++)
        {
            Vector3 wpp = wayPointsPos[i];
            float wpr = wayPointsRot[i];
            Vector2 pt = new Vector2(wpp.x, wpp.y);

            if (i != 0)
            {
                Vector3 wpPrev = wayPointsPos[i - 1];
                Vector2 ptPrev = new Vector2(wpPrev.x, wpPrev.y);
                total += Vector2.Distance(pt, ptPrev);
            }

            wpWrappers.Add(new WayPointWrapper(pt, wpr, total));
        }

        return total;
    }

    protected override void OnSuccessfulInteract()
    {
        base.OnSuccessfulInteract();
        if (triggerOnSuccessInteract)
        {
            MovePlatform();
        }
    }

    private void MovePlatform()
    {
        if (ErrorCheckForWayPointsSize())
        {
            return;
        }

        //if in progress, don't interrupt
        if (currTime != 1f)
        {
            return;
        }

        currTime = 0f;
    }


    private void FixedUpdate()
    {
        WPFixedUpdate();
        if (DEVTOOLS_movePlatformHotKeyEnabled && Input.GetKeyDown(devToolsMoveHotKey))
        {
            MovePlatform();
        }
    }

    private void GetNextWP()
    {
        //reset
        if (currWPIndex == wpWrappers.Count -1)
        {
            currWPIndex = 0;
            //reverse points so we go in opposite direction now
            //next time time is set to 0
            wayPointsPos.Reverse();
            wayPointsRot.Reverse();
            CalcTotalDist();
        }

        float prevWPDist = 0f;

        //if first one, can't use pointB as last pos
        if (currWPIndex == 0)
        {
            currAPoint = wpWrappers[currWPIndex].pos;
            currARotation = wpWrappers[currWPIndex].rot;
        }
        else
        {
            currAPoint = currBPoint;
            currARotation = currBRotation;
            prevWPDist = wpWrappers[currWPIndex].startDist;
        }
        currWPIndex++;
        currBPoint = wpWrappers[currWPIndex].pos;
        currBRotation = wpWrappers[currWPIndex].rot;

        float percentOfWholeDist = (wpWrappers[currWPIndex].startDist - prevWPDist) / totalDist; //divide by total

        currWPSlope = 1f / percentOfWholeDist;
        currWPIntercept = -1 * currWPSlope * prevWPDist/totalDist;
    }

    private void WPFixedUpdate()
    {
        if (currTime >= 1f)
        {
            return;
        }

        currTime += moveTimeStep;

        if (currTime >= 1f)
        {
            currTime = 1f;
            movingPlatform.position = currBPoint;
            movingPlatform.rotation = Quaternion.Euler(0, 0, currBRotation);
            GetNextWP();
            return;
        }

        float currSmoothTime = Mathf.SmoothStep(0f, 1f, currTime);
        float localTimeStep = currWPSlope * currSmoothTime + currWPIntercept;

        int failSafe = 0;
        //GetNextWP gaurantees increase in currWPIndex or that currWPIndex = wpWrappers.Count - 1
        while (localTimeStep >= 1f && currWPIndex < wpWrappers.Count - 1 && failSafe < 100)
        {
            failSafe++;
            GetNextWP();
            localTimeStep = currWPSlope * currSmoothTime + currWPIntercept;
        }

        movingPlatform.position = Vector2.Lerp(currAPoint, currBPoint, localTimeStep);
        movingPlatform.rotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(currARotation, currBRotation, localTimeStep));
    }

    private bool ErrorCheckForWayPointsSize()
    {
        if (wayPointsPos.Count < 2)
        {
            Debug.LogError(string.Format("There are only {0} way points to use for movable platform object {1}, need more!", wayPointsPos.Count, name));
            return true;
        }

        return false;
    }
}