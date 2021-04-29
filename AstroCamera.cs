using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AstroCamera : MonoBehaviour
{
    public enum ZOOM {NORM, CLOSE, WIDE, EXTRA_WIDE}

    [SerializeField]
    private Transform astroPlayer;

    [SerializeField]
    private float smoothSpeed;

    [SerializeField]
    private Vector2 moveThreshold;

    [SerializeField]
    private Vector2 offsetFacingRight;

    private Vector3 offset3D;

    [SerializeField]
    private float zoomTime;

    [SerializeField]
    List<ZoomKeyVal> zoomValues;

    private Dictionary<ZOOM, float> zoomDict = new Dictionary<ZOOM, float>();


    [Serializable]
    private class ZoomKeyVal
    {
        [SerializeField]
        private ZOOM zoomType;
        public ZOOM ZoomType => zoomType;

        [SerializeField]
        private float zoomVal;
        public float ZoomVal => zoomVal;
    }

    private Camera camComponent;
    private float currTime = 0f;
    private ZOOM currZoomType;
    private float prevZoom;
    private float targetZoom;
    private Vector3 astroPlayerPos;

    [SerializeField]
    private ZOOM pastStartZoom;
    [SerializeField]
    private ZOOM futureStartZoom;

    private ZOOM pastZoom;
    private ZOOM futureZoom;

    //TODO: use the getter setter custom shit so we don't have to do this for optimization
    [SerializeField]
    private bool testUpdatingValues = false;

    private void Awake()
    {
        camComponent = GetComponent<Camera>();

        if (astroPlayer == null)
        {
            Debug.LogError("astro player transform is null in camera! No bueno :(");
            return;
        }


        currTime = 1f;

        pastZoom = pastStartZoom;
        futureZoom = futureStartZoom;

        SetZoomDict();
        InstantSetZoom(S_TimeTravel.Current.InFuture() ? futureZoom : pastZoom);

        SetOffset3D();
        UpdateCachedAstroPos();
        /*
        //verify we've filled out all the zoom values
        foreach (ZOOM zoomType in Enum.GetValues(typeof(ZOOM)))
        {
            if (zoomDict.ContainsKey(zoomType) && zoomDict[zoomType] > 0)
            {
                return;
            }

            Debug.LogError(String.Format("Value for zoom type {0} was not set or is still zero!", zoomType.ToString()));
        }
        */

        //keep the current cameras z axis pos
        AstroAnim.OnOrientationUpdate -= AstroAnim_OnOrientationUpdate;
        AstroAnim.OnOrientationUpdate += AstroAnim_OnOrientationUpdate;


        S_TimeTravel.Current.UpdateCamera -= S_TimeTravel_UpdateCamera;
        S_TimeTravel.Current.UpdateCamera += S_TimeTravel_UpdateCamera;
        //TODO: implement a subscribe to camera zoom change's static on zoom change

        InstantSetCameraToAstro();
    }

    private void S_TimeTravel_UpdateCamera()
    {
        if (S_TimeTravel.Current.InFuture())
        {
            pastZoom = currZoomType;
            InstantSetZoom(futureZoom);
        }
        else
        {
            futureZoom = currZoomType;
            InstantSetZoom(pastZoom);
        }

        InstantSetCameraToAstro();
    }

    private void InstantSetZoom(ZOOM zoom)
    {
        currZoomType = zoom;
        prevZoom = zoomDict[currZoomType];
        targetZoom = zoomDict[currZoomType];
    }

    private void InstantSetCameraToAstro()
    {
        transform.position = new Vector3(astroPlayer.position.x, astroPlayer.position.y, transform.position.z) + offset3D;
    }

    private void SetZoomDict()
    {
        foreach (ZoomKeyVal zkv in zoomValues)
        {
            if (zoomDict.ContainsKey(zkv.ZoomType))
            {
                zoomDict[zkv.ZoomType] = zkv.ZoomVal;
            }
            else
            {
                zoomDict.Add(zkv.ZoomType, zkv.ZoomVal);
            }
        }


        targetZoom = zoomDict[currZoomType];
    }

    private void SetOffset3D()
    {
        float multiplyer = Mathf.Sign(offset3D.x);
        offset3D = new Vector3(offsetFacingRight.x, offsetFacingRight.y, 0f);
        offset3D.x *= multiplyer;
    }

    private void UpdateCachedAstroPos()
    {
        astroPlayerPos = new Vector3(astroPlayer.position.x, astroPlayer.position.y, transform.position.z);
    }

    //AstroAnim.OnOrientationUpdate is invoked in a FixedUpdate call
    private void AstroAnim_OnOrientationUpdate(float directionMultiplyer)
    {
        offset3D.x = Mathf.Abs(offset3D.x) * directionMultiplyer;
    }

    private void OnCameraZoomChanged(ZOOM newZoom)
    {
        currTime = 0f;
        prevZoom = targetZoom;
        targetZoom = zoomDict[newZoom];
    }

    //Want FixedUpdate b/c playing with changing position of cam
    private void FixedUpdate()
    {
        //so we get the z axis of the camera
        if (testUpdatingValues)
        {
            SetZoomDict();
            SetOffset3D();
        }


        ZoomFixedUpdate();

        //TODO: maybe add an effect to only use these bounds if starting from rest and normal centered
        //(like not when coming back to center)
        if (OutOfMoveThreshold())
        {
            UpdateCachedAstroPos();
        }

        transform.position = Vector3.Lerp(transform.position, astroPlayerPos + offset3D, smoothSpeed * Time.fixedDeltaTime);
    }

    private bool OutOfMoveThreshold()
    {
        Vector3 center = transform.position;
        float rightBound = center.x + moveThreshold.x / 2f;
        float leftBound = center.x - moveThreshold.x / 2f;
        float upperBound = center.y + moveThreshold.y / 2f;
        float lowerBound = center.y - moveThreshold.y / 2f;
        return astroPlayer.position.x > rightBound
            || astroPlayer.position.x < leftBound
            || astroPlayer.position.y < lowerBound
            || astroPlayer.position.y > upperBound;
    }

    //TODO: still needs work
    private void ZoomFixedUpdate()
    {
        if (Mathf.Abs(targetZoom - camComponent.orthographicSize) < 0.001)
        {
            return;
        }
        currTime = Mathf.Min(currTime + Time.fixedDeltaTime / zoomTime, 1f);
        float smoothStep = Mathf.SmoothStep(0, 1f, currTime);
        camComponent.orthographicSize = Mathf.Lerp(prevZoom, targetZoom, smoothStep);
    }
}
