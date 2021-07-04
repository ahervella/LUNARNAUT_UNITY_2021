using System;
using UnityEngine;

[Serializable]
public class AnimatedTextContainer
{
    [SerializeField]
    private SO_AnimatedTextTemplate useTemplate;
    public SO_AnimatedTextTemplate UseTemplate => useTemplate;
    [SerializeField]
    private bool animateOnlyOnce = false;
    public bool AnimateOnlyOnce => animateOnlyOnce;
    private bool wasAnimated = false;
    [SerializeField]
    private AnimatedText.ATDetails details;
    public AnimatedText.ATDetails Details => details;
    [SerializeField]
    private Transform customParent;
    public Transform CustomParent => customParent;

    public bool CanAnimate()
    {
        if (UseTemplate != null)
        {
            if (!UseTemplate.Details.ShouldAnimate())
            {
                return false;
            }
        }

        else if (!Details.ShouldAnimate())
        {
            return false;
        }

        if (!AnimateOnlyOnce)
        {
            return true;
        }

        return !wasAnimated;
    }

    public void MarkAnimated()
    {
        wasAnimated = true;
    }
}
