using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AnimatedText;

public class S_AnimatedTextBuilder : Singleton<S_AnimatedTextBuilder>
{
    [SerializeField]
    private AnimatedText animatedTextPrefab;

    public AnimatedText InstanceAnimatedText(Transform parent)
    {
        if (animatedTextPrefab == null)
        {
            Debug.LogError("Tried to instance a copy of the animated text prefab but it is not assigned to the singleton!");
            return null;
        }
        return Instantiate(animatedTextPrefab, parent);
    }


    public AnimatedText StartNewTextAnimation(ATDetails atd, Transform customParent, AnimatedText specificATDToReuse )
    {
        switch (atd.Anchor)
        {
            case ATDetails.AT_ANCHOR.LOCAL_POS:
                if (specificATDToReuse == null)
                {
                    specificATDToReuse = InstanceAnimatedText(customParent);
                }
                specificATDToReuse.transform.localPosition = Vector3.zero;
                break;
            case ATDetails.AT_ANCHOR.ASTRO_LEFT:
            case ATDetails.AT_ANCHOR.ASTRO_RIGHT:
            case ATDetails.AT_ANCHOR.ASTRO_FRONT:
            case ATDetails.AT_ANCHOR.ASTRO_BEHIND:
                if (specificATDToReuse == null)
                {
                    specificATDToReuse = InstanceAnimatedText(customParent);
                }
                SetAstroAnchor(ref specificATDToReuse, atd.Anchor);
                break;
            default:
                if (specificATDToReuse == null)
                {
                    //camera may use multiple text in multiple locations so no need to worry about having multiple of them
                    specificATDToReuse = InstanceAnimatedText(S_Global.Current.GetAstroPlayer().transform);
                }
                SetAsCameraAnchor(ref specificATDToReuse, atd.Anchor);
                break;
        }

        specificATDToReuse.AnimateAndSetText(atd);
        return specificATDToReuse;
    }


    private void SetAstroAnchor(ref AnimatedText at, ATDetails.AT_ANCHOR anchor)
    {
        AstroAnim astroAnim = S_Global.Current.GetAstroAnim();

        switch (anchor)
        {
            case ATDetails.AT_ANCHOR.ASTRO_RIGHT:
                at.transform.localPosition = ATDetails.ASTRO_TEXT_POS_OFFSET;
                return;
            case ATDetails.AT_ANCHOR.ASTRO_LEFT:
                at.transform.localPosition = -ATDetails.ASTRO_TEXT_POS_OFFSET;
                return;
            case ATDetails.AT_ANCHOR.ASTRO_FRONT:
                at.transform.localPosition = astroAnim.FacingRight ? ATDetails.ASTRO_TEXT_POS_OFFSET : ATDetails.ASTRO_TEXT_POS_OFFSET;
                return;
            case ATDetails.AT_ANCHOR.ASTRO_BEHIND:
                at.transform.localPosition = !astroAnim.FacingRight ? ATDetails.ASTRO_TEXT_POS_OFFSET : ATDetails.ASTRO_TEXT_POS_OFFSET;
                return;
        }
    }

    private void SetAsCameraAnchor(ref AnimatedText at, ATDetails.AT_ANCHOR anchor)
    {
        Camera gameCam = S_Global.Current.GetCamera();
        //transform.parent = gameCam.transform;
        //align...
    }
}
