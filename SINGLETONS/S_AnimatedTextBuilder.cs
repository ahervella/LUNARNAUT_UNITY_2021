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
            case ATDetails.AT_ANCHOR.LOCAL_POS_RIGHT:
                if (specificATDToReuse == null)
                {
                    specificATDToReuse = InstanceAnimatedText(customParent);
                }

                AssignOffset(ref specificATDToReuse, atd, side: 1);
                break;

            case ATDetails.AT_ANCHOR.LOCAL_POS_LEFT:
                if (specificATDToReuse == null)
                {
                    specificATDToReuse = InstanceAnimatedText(customParent);
                }

                AssignOffset(ref specificATDToReuse, atd, side: -1);
                break;

            case ATDetails.AT_ANCHOR.LOCAL_POS_MIDDLE:
                if (specificATDToReuse == null)
                {
                    specificATDToReuse = InstanceAnimatedText(customParent);
                }

                AssignOffset(ref specificATDToReuse, atd, side: 0);
                break;

            case ATDetails.AT_ANCHOR.ASTRO_LEFT:
            case ATDetails.AT_ANCHOR.ASTRO_RIGHT:
            case ATDetails.AT_ANCHOR.ASTRO_FRONT:
            case ATDetails.AT_ANCHOR.ASTRO_BEHIND:
                SetAstroAnchor(ref specificATDToReuse, atd);
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

    /*
    private void SetLeftAnchor(AnimatedText specificATDToReuse, ATDetails.AT_ANCHOR anchor)
    {
        float offset = 0;
        if (anchor == ATDetails.AT_ANCHOR.LOCAL_POS_LEFT)
        {
            offset = specificATDToReuse.GetCurrTextWidth();
        }

        specificATDToReuse.transform.localPosition = new Vector3(-offset, 0, 0);
    }*/

    //"If you're talking advice wise... working on something, you could work on the final,
    //greatest thing you'll ever do, but as soon as you're done you need to move on and realize you can do better."
    //-JC

    private void AssignOffset(ref AnimatedText at, ATDetails details, int side)
    {
        if (details.AnimDirection == ATDetails.AT_ANIM_DIR.LEFT && side > 0)
        {
            at.AnchorOffSetMultiplyer = 1f;
        }
        else if (details.AnimDirection == ATDetails.AT_ANIM_DIR.RIGHT && side < 0)
        {
            at.AnchorOffSetMultiplyer = -1f;
        }
        else if (side == 0)
        {
            if (details.AnimDirection == ATDetails.AT_ANIM_DIR.RIGHT)
            {
                at.AnchorOffSetMultiplyer = -0.5f;
            }
            else if (details.AnimDirection == ATDetails.AT_ANIM_DIR.LEFT)
            {
                at.AnchorOffSetMultiplyer = 0.5f;
            }
            //middle typing text is taken care of when text animated in AnimatedText.cs
        }
    }

    private void SetAstroAnchor(ref AnimatedText at, ATDetails details)
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


        if (at == null)
        {
            at = InstanceAnimatedText(posTrans);
        }
        else // set the parent to the right one if not already
        {
            at.transform.parent = posTrans;
        }

        //cheap trick to see which of the two sides we got
        AssignOffset(ref at, details, side: (int) (posTrans.localPosition.x));
    }




    //TODO: move code to move all of local text positioning when animation starts so we know how much text we are dealing with.




    private void SetAsCameraAnchor(ref AnimatedText at, ATDetails.AT_ANCHOR anchor)
    {
        Camera gameCam = S_Global.Current.GetCamera();
        //transform.parent = gameCam.transform;
        //align...
    }
}
