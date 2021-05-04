using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: make astro anims not react if coming from under platform

public class BI_MovingPlatform : BasicInteractive
{
    [Header("Moving Platform")]
    [SerializeField]
    private bool triggerOnSuccessInteract = true;

    //TODO: disable if all dev tools off
    [SerializeField]
    private KeyCode devToolsMoveHotKey;

    [SerializeField]
    private Transform movingPlatform;

    [SerializeField]
    private float moveTime = 3f;

    [SerializeField]
    private float moveDelay = 0f;

    [SerializeField]
    private List<SO_Reaction> onMoveStartReactions;

    [SerializeField]
    private List<SO_Reaction> onMoveEndReactions;

    [Header("Audio Parameters")]

    [SerializeField]
    private GameObject platform3DSource;


    [SerializeField]
    private SoundOffsetWrapper onMoveStartSound;

    [SerializeField]
    private SoundOffsetWrapper onMoveEndSound;

    [Serializable]
    private class SoundOffsetWrapper
    {
        [SerializeField]
        private AK.Wwise.Event soundEvent;
        public AK.Wwise.Event SoundEvent => soundEvent;

        [SerializeField]
        private float soundOffset;
        public float SoundOffset => soundOffset;
    }

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
    private Coroutine delayedMoveCR;
    private Coroutine delayedStartSound;
    private Coroutine delayedEndSound;


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
        TimeTravel_ChangedState = true;
        if (ErrorCheckForWayPointsSize())
        {
            return;
        }

        //if in progress, don't interrupt
        if (TweenInProgress())
        {
            return;
        }

        ExecuteAllReactions(onMoveStartReactions);

        if (onMoveStartSound?.SoundEvent != null)
        {
            if (onMoveStartSound.SoundOffset < 0)
            {
                Debug.LogError(String.Format("On move start sound offset from object {0} and parent object {1} can't be negative!", gameObject.name, transform.parent.gameObject.name));
            }
            else
            {
                //THE SOUND OBJECT NEEDS TO BE THE SAME FOR THE START AND STOP IN ORDER FOR THE STOP TO WORK IN WWISE
                delayedStartSound = StartCoroutine(DelayPlaySound(onMoveStartSound.SoundOffset, onMoveStartSound.SoundEvent));
            }
        }

        if (onMoveEndSound?.SoundEvent != null)
        {
            float onEndDelay = moveTime + onMoveEndSound.SoundOffset + moveDelay;
            if (onEndDelay <= 0)
            {
                Debug.LogError(String.Format("On move end sound offset from object {0} and parent object {1} is too small with moveTime!", gameObject.name, transform.parent.gameObject.name));
            }

            else
            {
                delayedEndSound = StartCoroutine(DelayPlaySound(onEndDelay, onMoveEndSound.SoundEvent));
            }
        }

        delayedMoveCR = StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        if (moveDelay > 0f)
        {
            yield return new WaitForSeconds(moveDelay);
        }
        currTime = 0f;
    }

    protected bool TweenInProgress()
    {
        //if in progress, don't interrupt
        return currTime < 1f || delayedMoveCR != null;
    }

    protected IEnumerator DelayPlaySound(float delay, AK.Wwise.Event soundEvent)
    {
        yield return new WaitForSeconds(delay);

        if (platform3DSource == null)
        {
            Debug.LogError(String.Format("No sound source obj specified for: {0} from class: {1}", soundEvent, this));
        }

        soundEvent.Post(platform3DSource);
    }

    private void FixedUpdate()
    {
        WPFixedUpdate();
        if (DEVTOOLS_movePlatformHotKeyEnabled && Input.GetKeyDown(devToolsMoveHotKey))
        {
            MovePlatform();
        }
    }

    private void ResetAndReverseWP()
    {
        StopDelayedMoveCRs();
        ExecuteAllReactions(onMoveEndReactions);
        wayPointsPos.Reverse();
        wayPointsRot.Reverse();
        ResetAndChangeWPs(wayPointsPos, wayPointsRot);
    }

    private void StopDelayedMoveCRs()
    {
        if (delayedMoveCR != null)
        {
            StopCoroutine(delayedMoveCR);
        }
        delayedMoveCR = null;

        //TODO: incorperate into new system
        /*
        if (delayedStartSound != null)
        {
            StopCoroutine(delayedStartSound);
        }
        delayedStartSound = null;

        if (delayedEndSound != null)
        {
            StopCoroutine(delayedEndSound);
        }
        delayedEndSound = null;
        */
    }

    private void ResetAndChangeWPs(List<Vector3> positions, List<float> rotations)
    {
        wayPointsPos = positions;
        wayPointsRot = rotations;
        CalcTotalDist();
        ResetPlatformToOrigin();
    }

    private void ResetPlatformToOrigin()
    {
        currWPIndex = 0;
        currTime = 1f;
        GetNextWP();

        movingPlatform.position = currAPoint;
        movingPlatform.rotation = Quaternion.Euler(0, 0, currARotation);
    }

    private void GetNextWP()
    {
        currAPoint = wpWrappers[currWPIndex].pos;
        currARotation = wpWrappers[currWPIndex].rot;
        float prevWPDist = wpWrappers[currWPIndex].startDist;

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
            ResetAndReverseWP();
            return;
        }

        float currSmoothTime = Mathf.SmoothStep(0f, 1f, currTime);
        float localTimeStep = currWPSlope * currSmoothTime + currWPIntercept;

        int failSafe = 0;
        //GetNextWP gaurantees increase in currWPIndex or that currWPIndex = wpWrappers.Count - 1
        while (localTimeStep >= 1f && failSafe < 100)
        {
            failSafe++;
            if (currWPIndex >= wpWrappers.Count - 1)
            {
                ResetAndReverseWP();
                return;
            }

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


    public class MovingPlatformTTD : ITimeTravelData
    {
        public List<Vector3> wayPointsPositions;
        public List<float> wayPointsRotations;

        public ITimeTravelData MakeDeepCopy()
        {
            MovingPlatformTTD deepCopy = new MovingPlatformTTD();
            deepCopy.wayPointsPositions = new List<Vector3>();
            deepCopy.wayPointsRotations = new List<float>();

            for(int i = 0; i < wayPointsPositions.Count; i++)
            {
                deepCopy.wayPointsPositions.Add(wayPointsPositions[i]);
                deepCopy.wayPointsRotations.Add(wayPointsRotations[i]);
            }

            return deepCopy;
        }
    }

    protected override ITimeTravelData ComposeNewTTD()
    {
        MovingPlatformTTD specifiedData = new MovingPlatformTTD();
        if (TweenInProgress())
        {
            ResetAndReverseWP();
        }
        else
        {
            ResetPlatformToOrigin();
        }

        specifiedData.wayPointsPositions = wayPointsPos;
        specifiedData.wayPointsRotations = wayPointsRot;
        
        return specifiedData;
    }

    protected override bool TryParseNewTTD(ITimeTravelData ttd)
    {
        if (ttd is MovingPlatformTTD mpttd)
        {
            ResetAndChangeWPs(mpttd.wayPointsPositions, mpttd.wayPointsRotations);
            return true;
        }

        return false;
    }

    // Don't need GetParentInteractive because this is the first custom data we're saving
}
