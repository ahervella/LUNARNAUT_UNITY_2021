using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class AstroCamera : MonoBehaviour
{
    public enum ZOOM { NORM, CLOSE, WIDE, EXTRA_WIDE }

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

    private List<ZoomWrapper> zoomStack = new List<ZoomWrapper>();
    private ZoomArea zoomAreaBeforeTT = null;

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

    private class ZoomWrapper
    {
        public ZoomWrapper(ZoomArea zoomArea, ZOOM zoomType)
        {
            ZoomArea = zoomArea;
            ZoomType = zoomType;
        }

        public ZoomArea ZoomArea { get; private set; }
        public ZOOM ZoomType { get; private set; }
    }

    private Camera camComponent;
    private float currZoomTime = 0f;
    private ZOOM currZoomType;
    private float prevZoom = 0;
    private float targetZoom = 0;
    private bool nextZoomSetInstant = true;

    private Vector3 astroPlayerPos;

    //TODO: use the getter setter custom shit so we don't have to do this for optimization
    [SerializeField]
    private bool testUpdatingValues = false;

    [SerializeField]
    private Image blackFade = default;

    [SerializeField]
    private float ttFadeOutTime = 1;
    [SerializeField]
    private float ttFadeInDelayTime = 0.5f;
    [SerializeField]
    private float ttFadeInTime = 0.25f;

    private bool ttFadingOut = true;
    private bool ttFadingIn = true;
    private float ttFadeOutStep = 0f;
    private float ttFadeInStep = 0f;

    public static event System.Action FadeOutComplete;
    public static event System.Action FadeInComplete;

    private void Awake()
    {
        camComponent = GetComponent<Camera>();

        if (astroPlayer == null)
        {
            Debug.LogError("astro player transform is null in camera! No bueno :(");
            return;
        }


        if (blackFade == null)
        {
            Debug.LogError("The black fade on the camera was not set!");
        }

        currZoomTime = 1f;
        

        SetZoomDict();
        //InstantSetZoom(S_TimeTravel.Current.InFuture() ? futureZoom : pastZoom);

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
        S_TimeTravel.Current.InitTimeTravelFade += S_TimeTravel_InitTimeTravelFade;

        ZoomArea.EnteredZoomArea -= ZoomArea_EnteredZoom;
        ZoomArea.EnteredZoomArea += ZoomArea_EnteredZoom;

        ZoomArea.ExitedZoomArea -= ZoomArea_ExitedZoom;
        ZoomArea.ExitedZoomArea += ZoomArea_ExitedZoom;

    }

    
    private void Start()
    {
        InstantSetCameraToAstro();
    }
    private void S_TimeTravel_UpdateCamera()
    {
        InstantSetCameraToAstro();

        if (zoomStack.Count > 0)
        {
            nextZoomSetInstant = zoomAreaBeforeTT != zoomStack[zoomStack.Count - 1].ZoomArea;
        }
        else
        {
            nextZoomSetInstant = zoomAreaBeforeTT != null;
        }
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


    private void ZoomArea_EnteredZoom(ZoomArea zoomArea, ZOOM zoomType)
    {

        if (GetZoomWrapperFromStack(zoomArea) != null)
        {
            Debug.LogErrorFormat("Some shit went real wrong trying to add ZoomArea: {0} to the zoom stack but already in stack.", zoomArea.name);
            return;
        }

        zoomStack.Add(new ZoomWrapper(zoomArea, zoomType));

        ChangeCameraZoom(zoomType);
    }

    private void ZoomArea_ExitedZoom(ZoomArea zoomArea, ZOOM zoomType)
    {
        ZoomWrapper zw = GetZoomWrapperFromStack(zoomArea);
        if (zw == null)
        {
            Debug.LogErrorFormat("Tryed to remove ZoomArea: {0} from the zoom stack but not in stack.", zoomArea.name);
            return;
        }
        zoomStack.Remove(zw);

        if (zoomStack.Count > 0)
        {
            ChangeCameraZoom(zoomStack[zoomStack.Count-1].ZoomType);
        }
    }

    private ZoomWrapper GetZoomWrapperFromStack(ZoomArea zoomArea)
    {
        foreach(ZoomWrapper zw in zoomStack)
        {
            if (zw.ZoomArea == zoomArea)
            {
                return zw;
            }
        }

        return null;
    }


    private void ChangeCameraZoom(ZOOM newZoom)
    {
        //only happens on the first zoom set from starting scene
        if (nextZoomSetInstant)
        {
            nextZoomSetInstant = false;
            InstantSetZoom(newZoom);
            return;
        }

        if (newZoom == currZoomType)
        {
            //no need and let this animation play out if its in the middle of that
            nextZoomSetInstant = false;
            return;
        }

        currZoomType = newZoom;
        currZoomTime = 0f;
        prevZoom = camComponent.orthographicSize;
        targetZoom = zoomDict[newZoom];
    }

    private void InstantSetZoom(ZOOM zoom)
    {
        currZoomType = zoom;
        currZoomTime = 1f;
        prevZoom = zoomDict[currZoomType];
        targetZoom = zoomDict[currZoomType];
        camComponent.orthographicSize = targetZoom;
    }

    private void S_TimeTravel_InitTimeTravelFade()
    {
        ttFadeOutStep = Time.fixedDeltaTime / ttFadeOutTime;
        ttFadeInStep = -Time.fixedDeltaTime / ttFadeInTime;
        ttFadingOut = true;
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
        TTFadeFixedUpdate();

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
        currZoomTime = Mathf.Min(currZoomTime + Time.fixedDeltaTime / zoomTime, 1f);
        float smoothStep = Mathf.SmoothStep(0, 1f, currZoomTime);
        camComponent.orthographicSize = Mathf.Lerp(prevZoom, targetZoom, smoothStep);
    }

    private void TTFadeFixedUpdate()
    {
        if (!ttFadingOut && !ttFadingIn)
        {
            return;
        }

        if (ttFadingOut)
        {
            float nextAlpha = Mathf.Min(blackFade.color.a + ttFadeOutStep, 1f);
            blackFade.color = new Color(0, 0, 0, nextAlpha);
            if (nextAlpha >= 1f)
            {
                ttFadingOut = false;
                zoomAreaBeforeTT = zoomStack.Count > 0 ? zoomStack[zoomStack.Count - 1].ZoomArea : null;
                FadeOutComplete();
                StartCoroutine(DelayTimeTravelFadeIn());
            }
        }
        else if (ttFadingIn)
        {
            float nextAlpha = Mathf.Max(blackFade.color.a + ttFadeInStep, 0f);
            blackFade.color = new Color(0, 0, 0, nextAlpha);
            if (nextAlpha <= 0f)
            {
                ttFadingIn = false;
                FadeInComplete();
            }
        }
    }


    IEnumerator DelayTimeTravelFadeIn()
    {
        yield return new WaitForSeconds(ttFadeInDelayTime);
        ttFadingIn = true;
    }
}
