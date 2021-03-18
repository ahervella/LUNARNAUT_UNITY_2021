using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BI_MovingPlatform : BasicInteractive
{
    [Header("Moving Platform")]
    [SerializeField]
    private float moveTime = 3f;
    /*
     * implement
    [SerializeField]
    private bool automaticLoop = false;
    */

    [SerializeField]
    private bool usePlatformPosAsFirstWayPoint = true;

    [SerializeField]
    private List<Transform> initialWayPoints = new List<Transform>();
    private List<Vector3> wayPoints = new List<Vector3>();

    private List<WayPointWrapper> wpWrappers = new List<WayPointWrapper>();

    //class is used to have a cached list of the vector 2 version of the pos
    //and the distance along the path from the start
    private class WayPointWrapper
    {
        public Vector2 pos;
        public float startDist;

        public WayPointWrapper(Vector2 pos, float startDist)
        {
            this.pos = pos;
            this.startDist = startDist;
        }
    }

    private int currWPIndex = 0;

    [SerializeField]
    private Transform destinaitonTransform;

    private float totalDist;
    private float moveTimeStep;
    private float currTime = 1f;
    //private float delayBetweenMoves;

    private void Awake()
    {
        if (usePlatformPosAsFirstWayPoint)
        {
            initialWayPoints.Insert(0, transform);
        }

        //transforms move as we move the platform, so
        //just cache the points right away
        foreach (Transform wp in initialWayPoints)
        {
            wayPoints.Add(wp.position);
        }

        if (ErrorCheckForWayPointsSize())
        {
            return;
        }

        totalDist = CalcTotalDist();
        //cache this so we don't calculate per step
        moveTimeStep = Time.fixedDeltaTime / moveTime;
        GetNextWP();


        MovePlatform();
    }

    private float CalcTotalDist()
    {
        wpWrappers.Clear();

        float total = 0;
        for(int i = 0; i < wayPoints.Count; i++)
        {
            Vector3 wp = wayPoints[i];
            Vector2 pt = new Vector2(wp.x, wp.y);
            if (i != 0)
            {
                Vector3 wpPrev = wayPoints[i - 1];
                Vector2 ptPrev = new Vector2(wpPrev.x, wpPrev.y);
                total += Vector2.Distance(pt, ptPrev);
            }

            wpWrappers.Add(new WayPointWrapper(pt, total));
        }

        return total;
    }

    protected override void OnSuccessfulInteract()
    {
        base.OnSuccessfulInteract();
        MovePlatform();
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
        /*
        if (currTime < 1f)
        {
            currTime = Mathf.Min(currTime + moveTimeStep, 1f);
            currPos = Vector2.Lerp(currStart, currTarget, Mathf.SmoothStep(0f, 1f, currTime));
            transform.position = new Vector3(currPos.x, currPos.y, 0);
            Debug.Log(string.Format("currPos = {0}, currTarget = {1}, currTime = {2}", currPos, currTarget, Mathf.SmoothStep(0f, 1f, currTime)));
        }//"I take serious supplements! I take a fucking multivitamin or something... lol
        */
        WPFixedUpdate();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            MovePlatform();
        }
    }

    private float currWPSlope;
    private float currWPIntercept;

    private Vector2 currAPoint;
    private Vector2 currBPoint;

    private void GetNextWP()
    {
        //reset
        if (currWPIndex == wpWrappers.Count -1)//count)
        {
            currWPIndex = 0;
            //reverse points so we go in opposite direction now
            wayPoints.Reverse();
            CalcTotalDist();
        }

        float prevWPDist = 0f;

        if (currWPIndex == 0)//currWP == null)
        {
            currAPoint = wpWrappers[currWPIndex].pos;
        }
        else
        {
            currAPoint = currBPoint;
        }
        currWPIndex++;
        currBPoint = wpWrappers[currWPIndex].pos;

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

        currTime = Mathf.Min(currTime + moveTimeStep, 1f);
        float currSmoothTime = Mathf.SmoothStep(0f, 1f, currTime);
        float localTimeStep = Mathf.Min(currWPSlope * currSmoothTime + currWPIntercept, 1f);
        transform.position = Vector2.Lerp(currAPoint, currBPoint, localTimeStep);
        if (localTimeStep == 1f)
        {
            GetNextWP();
        }
    }

    private bool ErrorCheckForWayPointsSize()
    {
        if (wayPoints.Count < 2)
        {
            Debug.LogError(string.Format("There are only {0} way points to use for movable platform object {1}, need more!", wayPoints.Count, name));
            return true;
        }

        return false;
    }
}
