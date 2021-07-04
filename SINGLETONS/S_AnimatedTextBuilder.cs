using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AnimatedText;

public class S_AnimatedTextBuilder : Singleton<S_AnimatedTextBuilder>
{
    [SerializeField]
    private AnimatedText animatedTextPrefab;

    private AnimatedText InstanceAnimatedText(Transform parent)
    {
        if (animatedTextPrefab == null)
        {
            Debug.LogError("Tried to instance a copy of the animated text prefab but it is not assigned to the singleton!");
            return null;
        }
        return Instantiate(animatedTextPrefab, parent);
    }


    public AnimatedText StartNewTextAnimation(ATDetails atd, Transform customParent, AnimatedText specificATToReuse )
    {
        if (specificATToReuse != null)
        {
            //need to stop this update before we change the FixedCamAnchor & reset to what default prefab would be
            specificATToReuse.StopFixedCamUpdates();
            specificATToReuse.FixedCamAnchor = null;
            specificATToReuse.transform.parent = customParent;
            specificATToReuse.transform.localPosition = Vector3.zero;
            specificATToReuse.TextMesh.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            specificATToReuse.TextMesh.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        }

        switch (atd.Anchor)
        {
            case ATDetails.AT_ANCHOR.LOCAL_POS_RIGHT:
            case ATDetails.AT_ANCHOR.LOCAL_POS_LEFT:
            case ATDetails.AT_ANCHOR.LOCAL_POS_CENTER:
                SetLocalAnchor(ref specificATToReuse, atd, customParent);
                break;

            case ATDetails.AT_ANCHOR.ASTRO_LEFT:
            case ATDetails.AT_ANCHOR.ASTRO_RIGHT:
            case ATDetails.AT_ANCHOR.ASTRO_FRONT:
            case ATDetails.AT_ANCHOR.ASTRO_BEHIND:
                customParent = SetAndGetAstroAnchor(ref specificATToReuse, atd);
                break;

            case ATDetails.AT_ANCHOR.BOTTOM_LEFT:
            case ATDetails.AT_ANCHOR.BOTTOM_CENTER:
            case ATDetails.AT_ANCHOR.BOTTOM_RIGHT:
            case ATDetails.AT_ANCHOR.TOP_LEFT:
            case ATDetails.AT_ANCHOR.TOP_CENTER:
            case ATDetails.AT_ANCHOR.TOP_RIGHT:
            case ATDetails.AT_ANCHOR.CENTER:
                SetCameraAnchor(ref specificATToReuse, atd);
                break;
        }

        if (atd.FixedSizeInCam)
        {
            SetCameraAsParent(ref specificATToReuse);
            //need to do after this method so that we don't have a null at
            specificATToReuse.FixedCamAnchor = customParent;
        }

        specificATToReuse.AnimateAndSetText(atd);
        return specificATToReuse;
    }

    //"If you're talking advice wise... working on something, you could work on the final,
    //greatest thing you'll ever do, but as soon as you're done you need to move on and realize you can do better."
    //-JC

    private void AssignOffset(ref AnimatedText at, ATDetails details, int side)
    {
        if (side > 0)
        {
            at.AnchorOffSetMultiplyer.x = 0.5f;
        }
        else if (side < 0)
        {
            at.AnchorOffSetMultiplyer.x = -0.5f;
        }
        else
        {
            at.AnchorOffSetMultiplyer.x = 0f;
        }

        switch (details.AnchorVertical)
        {
            case ATDetails.AT_ANCHOR_VERT.BELOW:
                at.AnchorOffSetMultiplyer.y = 0f;
                break;

            case ATDetails.AT_ANCHOR_VERT.ABOVE:
                at.AnchorOffSetMultiplyer.y = 1f;
                break;

            case ATDetails.AT_ANCHOR_VERT.MIDDLE:
                at.AnchorOffSetMultiplyer.y = 0.5f;
                break;
        }
    }

    private void SetLocalAnchor(ref AnimatedText at, ATDetails details, Transform customParent)
    {
        if (at == null)
        {
            at = InstanceAnimatedText(customParent);
        }

        switch (details.Anchor)
        {
            case ATDetails.AT_ANCHOR.LOCAL_POS_RIGHT:
                AssignOffset(ref at, details, side: 1);
                break;

            case ATDetails.AT_ANCHOR.LOCAL_POS_LEFT:
                AssignOffset(ref at, details, side: -1);
                break;

            case ATDetails.AT_ANCHOR.LOCAL_POS_CENTER:
                AssignOffset(ref at, details, side: 0);
                break;
        }
    }

    private Transform SetAndGetAstroAnchor(ref AnimatedText at, ATDetails details)
    {
        AstroPlayer astroPlayer = S_Global.Current.GetAstroPlayer();
        Transform posTrans;

        switch (details.Anchor)
        {
            case ATDetails.AT_ANCHOR.ASTRO_RIGHT:
                posTrans = astroPlayer.GetTextTransformSide(right: true);
                break;
            case ATDetails.AT_ANCHOR.ASTRO_LEFT:
                posTrans = astroPlayer.GetTextTransformSide(right: false);
                break;
            case ATDetails.AT_ANCHOR.ASTRO_FRONT:
                posTrans = astroPlayer.GetTextTransformDirection(front: true);
                break;
            default://case ATDetails.AT_ANCHOR.ASTRO_BEHIND:
                posTrans = astroPlayer.GetTextTransformDirection(front: false);
                break;
        }

        SetParent(ref at, posTrans);

        //cheap trick to see which of the two sides we got
        AssignOffset(ref at, details, side: (int) (posTrans.localPosition.x * posTrans.localScale.x));

        return posTrans;
    }




    //TODO: move code to move all of local text positioning when animation starts so we know how much text we are dealing with.




    private void SetCameraAnchor(ref AnimatedText at, ATDetails details)
    {
        SetCameraAsParent(ref at);

        at.transform.localPosition = Vector3.zero;
        at.AnchorOffSetMultiplyer = Vector2.zero;

        switch (details.Anchor)
        {
            case ATDetails.AT_ANCHOR.BOTTOM_CENTER:
                //need to set the textmesh position b/c can't move at local position if it's canvas is set to render on cam
                at.TextMesh.rectTransform.anchorMax = new Vector2(0.5f, 0);
                at.TextMesh.rectTransform.anchorMin = new Vector2(0.5f, 0);
                AssignOffset(ref at, details, side: 0);
                break;

            case ATDetails.AT_ANCHOR.BOTTOM_LEFT:
                at.TextMesh.rectTransform.anchorMax = new Vector2(0, 0);
                at.TextMesh.rectTransform.anchorMin = new Vector2(0, 0);
                AssignOffset(ref at, details, side: 1);
                break;

            case ATDetails.AT_ANCHOR.BOTTOM_RIGHT:
                at.TextMesh.rectTransform.anchorMax = new Vector2(1, 0);
                at.TextMesh.rectTransform.anchorMin = new Vector2(1, 0);
                AssignOffset(ref at, details, side: -1);
                break;

            case ATDetails.AT_ANCHOR.TOP_CENTER:
                at.TextMesh.rectTransform.anchorMax = new Vector2(0.5f, 1);
                at.TextMesh.rectTransform.anchorMin = new Vector2(0.5f, 1);
                AssignOffset(ref at, details, side: 0);
                break;

            case ATDetails.AT_ANCHOR.TOP_LEFT:
                at.TextMesh.rectTransform.anchorMax = new Vector2(0, 1);
                at.TextMesh.rectTransform.anchorMin = new Vector2(0, 1);
                AssignOffset(ref at, details, side: 1);
                break;

            case ATDetails.AT_ANCHOR.TOP_RIGHT:
                at.TextMesh.rectTransform.anchorMax = new Vector2(1, 1);
                at.TextMesh.rectTransform.anchorMin = new Vector2(1, 1);
                AssignOffset(ref at, details, side: -1);
                break;

            case ATDetails.AT_ANCHOR.CENTER:
                at.TextMesh.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                at.TextMesh.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                AssignOffset(ref at, details, side: 0);
                break;
        }

        at.TextMesh.transform.localPosition = Vector3.zero;
    }

    private void SetCameraAsParent(ref AnimatedText at)
    {
        Camera gameCam = S_Global.Current.GetCamera();

        //camera may use multiple text in multiple locations so no need to worry about having multiple of them
        SetParent(ref at, gameCam.transform);

        at.TextMesh.canvas.renderMode = RenderMode.ScreenSpaceCamera;
    }

    private void SetParent(ref AnimatedText at, Transform parentTrans)
    {
        if (at == null)
        {
            at = InstanceAnimatedText(parentTrans);
        }
        else
        {
            at.transform.parent = parentTrans;
        }
    }
}
