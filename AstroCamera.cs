using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Camera))]
public class AstroCamera : MonoBehaviour
{
    public enum ZOOM { NORM, CLOSE, WIDE, EXTRA_WIDE }
    public enum SHAKE { LIGHT, MED, HEAVY, COLLOSAL }

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


    [SerializeField]
    private float shakeOffsetChangeMaxTime = 0.5f;
    [SerializeField]
    private float shakeSmoothSpeed = 3f;
    private float currSmoothSpeed;
    [SerializeField]
    private List<ShakeKeyVal> shakeValues;

    private List<ShakeWrapper> shakeStack = new List<ShakeWrapper>();
    private CameraShakeArea shakeAreaBeforeTT = null;
    private Dictionary<SHAKE, float> shakeDict = new Dictionary<SHAKE, float>();
    private Coroutine shakeCR;
    private Vector3 shakeOffset3D;
    private Coroutine delayedReactionShakeCR;
    private Coroutine delayedEndEaseFadeCR;

    [Serializable]
    private class ShakeKeyVal
    {
        [SerializeField]
        private SHAKE shakeType;
        public SHAKE ShakeType => shakeType;

        [SerializeField]
        private float shakeVal;
        public float ShakeVal => shakeVal;
    }

    private class ShakeWrapper
    {
        public ShakeWrapper(CameraShakeArea shakeArea, SHAKE shakeType, float duration, float easeInTime, float easeOutTime)
        {
            ShakeArea = shakeArea;
            ShakeType = shakeType;
            Duration = duration;
            EaseInTime = easeInTime;
            EaseOutTime = easeOutTime;
        }

        public CameraShakeArea ShakeArea { get; private set; }
        public SHAKE ShakeType { get; private set; }
        public float Duration { get; private set; }
        public float EaseInTime { get; private set; }
        public float EaseOutTime { get; private set; }
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
    public float TTFadeOut => ttFadeOutTime;
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
        currSmoothSpeed = smoothSpeed;

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
        SetShakeDict();
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

        ZoomArea.EnteredZoomArea -= ZoomArea_EnteredZoomArea;
        ZoomArea.EnteredZoomArea += ZoomArea_EnteredZoomArea;

        ZoomArea.ExitedZoomArea -= ZoomArea_ExitedZoomArea;
        ZoomArea.ExitedZoomArea += ZoomArea_ExitedZoomArea;

        CameraShakeArea.EnteredShakeArea -= CameraShakeArea_EnteredShakeArea;
        CameraShakeArea.EnteredShakeArea += CameraShakeArea_EnteredShakeArea;

        CameraShakeArea.ExitedShakeArea -= CameraShakeArea_ExitedShakeArea;
        CameraShakeArea.ExitedShakeArea += CameraShakeArea_ExitedShakeArea;

        SO_RA_CameraShake.NewReactionShake -= SO_RA_CameraShake_NewReactionShake;
        SO_RA_CameraShake.NewReactionShake += SO_RA_CameraShake_NewReactionShake;
    }

    
    private void Start()
    {
        InstantSetCameraToAstro();
    }
    private void S_TimeTravel_UpdateCamera()
    {
        InstantSetCameraToAstro();

        nextZoomSetInstant = true;
        StartCoroutine(ResetZoomSetInstant());
    }

    //hack so that if no new area ONTriggerEntered recieved in the frame we time traveled,
    //must mean they are the same area so turn off.
    private IEnumerator ResetZoomSetInstant()
    {
        //end of frame did not work, needed to wait for next frame (via null)
        yield return null;
        nextZoomSetInstant = false;
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

    private void SetShakeDict()
    {
        foreach (ShakeKeyVal skv in shakeValues)
        {
            if (shakeDict.ContainsKey(skv.ShakeType))
            {
                shakeDict[skv.ShakeType] = skv.ShakeVal;
            }
            else
            {
                shakeDict.Add(skv.ShakeType, skv.ShakeVal);
            }
        }
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


    private void ZoomArea_EnteredZoomArea(ZoomArea zoomArea, ZOOM zoomType)
    {

        if (GetZoomWrapperFromStack(zoomArea) != null)
        {
            Debug.LogErrorFormat("Some shit went real wrong trying to add ZoomArea: {0} to the zoom stack but already in stack.", zoomArea.name);
            return;
        }

        zoomStack.Add(new ZoomWrapper(zoomArea, zoomType));

        ChangeCameraZoom(zoomType);
    }

    private void ZoomArea_ExitedZoomArea(ZoomArea zoomArea, ZOOM zoomType)
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

    private void CameraShakeArea_EnteredShakeArea(CameraShakeArea shakeArea, SHAKE shakeType, float duration, float easeInTime, float easeOutTime)
    {

        if (GetShakeWrapperFromStack(shakeArea) != null)
        {
            Debug.LogErrorFormat("Some shit went real wrong trying to add CameraShakeArea: {0} to the shake stack but already in stack.", shakeArea.name);
            return;
        }

        ShakeWrapper newSW = new ShakeWrapper(shakeArea, shakeType, duration, easeInTime, easeOutTime);
        shakeStack.Add(newSW);

        ChangeCameraShake(newSW);
    }

    private void CameraShakeArea_ExitedShakeArea(CameraShakeArea shakeArea, SHAKE shakeType, float duration, float easeInTime, float easeOutTime)
    {
        ShakeWrapper sw = GetShakeWrapperFromStack(shakeArea);
        if (sw == null)
        {
            Debug.LogErrorFormat("Tried to remove CameraShakeArea: {0} from the shake stack but not in stack.", shakeArea.name);
            return;
        }

        shakeStack.Remove(sw);

        if (shakeStack.Count > 0)
        {
            ShakeWrapper lastSW = shakeStack[shakeStack.Count - 1];
            ChangeCameraShake(lastSW);
        }
        else if (IsShakeInfinite(sw))
        {
            StopShaking();
        }
    }

    private void SO_RA_CameraShake_NewReactionShake(SHAKE shakeType, float duration, float delay, float easeInTime, float easeOutTime)
    {
        delayedReactionShakeCR = StartCoroutine(DelayedNewReactionShake(shakeType, duration, delay, easeInTime, easeOutTime));
        //TODO: have it so we trigger a shake area we may be in? Currently we'd need to exit the shake area and come back for this to happen
    }

    private IEnumerator DelayedNewReactionShake(SHAKE shakeType, float duration, float delay, float easeInTime, float easeOutTime)
    {
        yield return new WaitForSeconds(delay);
        ChangeCameraShake(shakeType, duration, easeInTime, easeOutTime);
    }

    private ShakeWrapper GetShakeWrapperFromStack(CameraShakeArea shakeArea)
    {
        foreach (ShakeWrapper sw in shakeStack)
        {
            if (sw.ShakeArea == shakeArea)
            {
                return sw;
            }
        }

        return null;
    }

    private void ChangeCameraShake(ShakeWrapper sw)
    {
        bool infiniteShake = IsShakeInfinite(sw);
        ChangeCameraShake(sw.ShakeType, sw.Duration, sw.EaseInTime, sw.EaseOutTime, infiniteShake);
    }

    private void ChangeCameraShake(SHAKE shakeType, float duration, float easeInTime, float easeOutTime, bool infiniteShake = false)
    {
        if ((easeInTime + easeOutTime) > duration && !infiniteShake)
        {
            Debug.LogErrorFormat("There is a camera shake that has longer easing than it lasts (and is not suppose to be infinite), no bueno! Duration: {0}, EaseIn: {1}, EaseOut: {2}", duration, easeInTime, easeOutTime);
            return;
        }
        StopShaking();
        shakeEaseTime = easeInTime;
        BeginShakeEase(easingIn: true);
        delayedEndEaseFadeCR = StartCoroutine(DelayedShakeEaseOut(duration - easeInTime, easeOutTime));
        shakeCR = StartCoroutine(NextSingleCameraShake(shakeDict[shakeType], infiniteShake, duration));
        currSmoothSpeed = shakeSmoothSpeed;
    }

    private IEnumerator DelayedShakeEaseOut(float delay, float easeOutTime)
    {
        yield return new WaitForSeconds(delay);
        shakeEaseTime = easeOutTime;
        BeginShakeEase(easingIn: false);
    }

    private bool IsShakeInfinite(ShakeWrapper sw)
    {
        return sw.Duration <= 0;
    }

    private void StopShaking()
    {
        if (shakeCR != null)
        {
            StopCoroutine(shakeCR);
            shakeCR = null;
        }

        if (delayedReactionShakeCR != null)
        {
            StopCoroutine(delayedReactionShakeCR);
            delayedReactionShakeCR = null;
        }

        if (delayedEndEaseFadeCR != null)
        {
            StopCoroutine(delayedEndEaseFadeCR);
            delayedEndEaseFadeCR = null;
        }

        shakeOffset3D = Vector3.zero;
        currSmoothSpeed = smoothSpeed;
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
            SetShakeDict();
            SetOffset3D();
        }


        ZoomFixedUpdate();
        ShakeFixedUpdate();
        TTFadeFixedUpdate();

        //TODO: maybe add an effect to only use these bounds if starting from rest and normal centered
        //(like not when coming back to center)
        if (OutOfMoveThreshold())
        {
            UpdateCachedAstroPos();
        }

        transform.position = Vector3.Lerp(transform.position, astroPlayerPos + offset3D + shakeOffset3D, currSmoothSpeed * Time.fixedDeltaTime);
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

    float shakeEaseTime = 3f;
    float shakeEaseCounter = 1f;
    float shakeEaseStep = 0f;
    float shakeEaseMultiplyer = 0f;

    private void BeginShakeEase(bool easingIn)
    {
        float fadeMultiplyer = easingIn ? 1f : -1f;
        shakeEaseStep = Time.fixedDeltaTime / shakeEaseTime * fadeMultiplyer;
        shakeEaseCounter = 0f;
    }

    private void ShakeFixedUpdate()
    {
        if (shakeEaseCounter >= 1f)
        {
            return;
        }
        shakeEaseMultiplyer += shakeEaseStep;
        shakeEaseMultiplyer = Mathf.Clamp(shakeEaseMultiplyer, 0f, 1f);

    }

    private IEnumerator NextSingleCameraShake(float shakeMaxCenterOffset, bool infinite = false, float durationLeft  = 0)
    {
        float randomCenterOffset = Random.Range(0, shakeMaxCenterOffset);
        float lerpedRandomCenterOffset = Mathf.Lerp(0, randomCenterOffset, shakeEaseMultiplyer);
        float randomRadAngle = Random.Range(0, 2 * Mathf.PI);
        Vector2 randomVector = new Vector2(Mathf.Cos(randomRadAngle), Mathf.Sin(randomRadAngle)) * lerpedRandomCenterOffset;
        shakeOffset3D = new Vector3(randomVector.x, randomVector.y);

        if (!infinite && durationLeft <= 0)
        {
            StopShaking();
            yield break;
        }

        float randomChangeTime = Random.Range(Time.fixedDeltaTime, shakeOffsetChangeMaxTime);
        yield return new WaitForSeconds(randomChangeTime);
        yield return NextSingleCameraShake(shakeMaxCenterOffset, infinite, durationLeft - randomChangeTime);
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
