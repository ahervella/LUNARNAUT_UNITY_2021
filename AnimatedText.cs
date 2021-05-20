//Excelent presentation on how to effectively use canvases:
//https://www.youtube.com/watch?v=_wxitgdx-UI&t=1408s&ab_channel=Unity

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnimatedText : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textMesh;

    [Serializable]
    public class ATDetails
    {
        //TODO: need to add code to make these dictionaries immutable. Tried using the immutable dll from microsoft but the hokeypokey with unity is a little wack
        //and will probably give me more trouble than I would with just making a utilities method that gives back a copy of the dictionary whenever called.
        //Tried following these guides:
        //https://docs.microsoft.com/en-us/visualstudio/gamedev/unity/unity-scripting-upgrade
        //https://stackoverflow.com/questions/56019411/how-to-install-infer-net-to-unity-2018-3

        public const float ANIMATE_TIME = 2f;
        public const float DEANIMATE_TIME = 0.5f;
        public const float DEFAULT_DISPLAY_TIME = 3.0f;

        public static readonly Vector3 ASTRO_TEXT_POS_OFFSET = new Vector3(5, 10, 0);

        public enum AT_COLOR { INFO, SUCCESS, WARNING, FAILURE }
        public static readonly Dictionary<AT_COLOR, Color> colorDict = new Dictionary<AT_COLOR, Color>
        {
            { AT_COLOR.FAILURE, Color.red },
            { AT_COLOR.INFO, Color.white },
            { AT_COLOR.SUCCESS, Color.green },
            { AT_COLOR.WARNING, Color.yellow }
        };
        //.ToImmutableDictionary();

        public enum AT_SIZE { SMALL, MED, LARGE, HUGE }
        public static readonly Dictionary<AT_SIZE, float> sizeDict = new Dictionary<AT_SIZE, float>
        {
            { AT_SIZE.SMALL, 10f },
            { AT_SIZE.MED, 10f },
            { AT_SIZE.LARGE, 10f },
            { AT_SIZE.HUGE, 10f }
        };
        //.ToImmutableDictionary();

        public enum AT_ANIM_TIME { SLOW, NORM, FAST }
        public static readonly Dictionary<AT_ANIM_TIME, float> animTimeDict = new Dictionary<AT_ANIM_TIME, float>
        {
            { AT_ANIM_TIME.SLOW, 0.5f },
            { AT_ANIM_TIME.NORM, 2f },
            { AT_ANIM_TIME.FAST, 4f }
        };
        //.ToImmutableDictionary();

        public enum AT_ANCHOR { LOCAL_POS, ASTRO_LEFT, ASTRO_RIGHT, ASTRO_FRONT, ASTRO_BEHIND, TOP_RIGHT, TOP_LEFT, TOP_CENTER, CENTER, BOTTOM_CENTER, BOTTOM_LEFT, BOTTOM_RIGHT }
        public enum AT_ANIM_DIR { RIGHT, LEFT, MIDDLE }
        public enum AT_ALIGNMENT { LEFT, CENTER, RIGHT }
        public enum AT_DURATION { INDEFINITE, DEFAULT, CUSTOM }

        [SerializeField]
        private string text;
        public string Text => text;

        [SerializeField]
        private AT_COLOR textColor = AT_COLOR.INFO;
        public AT_COLOR TextColor => textColor;

        [SerializeField]
        private AT_SIZE textSize = AT_SIZE.MED;
        public AT_SIZE TextSize => textSize;

        [SerializeField]
        private AT_ANIM_TIME animSpeed = AT_ANIM_TIME.NORM;
        public AT_ANIM_TIME AnimSpeed => animSpeed;

        [SerializeField]
        private AT_ANCHOR anchor = AT_ANCHOR.LOCAL_POS;
        public AT_ANCHOR Anchor => anchor;

        [SerializeField]
        private AT_ANIM_DIR animDirection = AT_ANIM_DIR.RIGHT;
        public AT_ANIM_DIR AnimDirection => animDirection;

        [SerializeField]
        private AT_ALIGNMENT alignment = AT_ALIGNMENT.LEFT;
        public AT_ALIGNMENT Alignment => alignment;

        [SerializeField]
        private AT_DURATION displayTime = AT_DURATION.DEFAULT;
        public AT_DURATION DisplayTime => displayTime;

        [SerializeField]
        private float customDisplayTime = DEFAULT_DISPLAY_TIME;
        public float CustomDisplayTime => customDisplayTime;

        [SerializeField]
        private bool goToLastTextOnFinish = false;
        public bool GoToLastTextOnFinish = false;
    }

    private string currText;
    private float currAnimTime;
    private Transform currAnchor;
    private float currDisplayTime;
    private bool indefDisplayTime = false;
    private bool goToLastTextOnFinish = false;

    private ATDetails currATD;
    private ATDetails previousATD;

    public void AnimateAndSetText(ATDetails atd)
    {
        SetFromATDetails(atd);
        StartTextAnimation();
    }

    private void SetFromATDetails(ATDetails atd)
    {
        previousATD = currATD;
        currATD = atd;

        currText = atd.Text;
        textMesh.color = ATDetails.colorDict[atd.TextColor];
        textMesh.fontSize = ATDetails.sizeDict[atd.TextSize];
        currAnimTime = ATDetails.animTimeDict[atd.AnimSpeed];
        //SetATDAnchor(atd.Anchor);

        switch (atd.Alignment)
        {
            case ATDetails.AT_ALIGNMENT.CENTER:
                textMesh.alignment = TextAlignmentOptions.Center;
                break;
            case ATDetails.AT_ALIGNMENT.LEFT:
                textMesh.alignment = TextAlignmentOptions.Left;
                break;
            case ATDetails.AT_ALIGNMENT.RIGHT:
                textMesh.alignment = TextAlignmentOptions.Right;
                break;
        }

        indefDisplayTime = false;

        switch (atd.DisplayTime)
        {
            case ATDetails.AT_DURATION.CUSTOM:
                currDisplayTime = atd.CustomDisplayTime;
                break;
            case ATDetails.AT_DURATION.DEFAULT:
                currDisplayTime = ATDetails.DEFAULT_DISPLAY_TIME;
                break;
            case ATDetails.AT_DURATION.INDEFINITE:
                indefDisplayTime = true;
                break;
        }

        goToLastTextOnFinish = atd.GoToLastTextOnFinish;
    }


    private Coroutine animatingCR;
    private Coroutine deanimatingCR;

    private void StartTextAnimation()
    {
        StopTextCR(deanimatingCR);
        StopTextCR(animatingCR);
        float delayPerCharacter = currAnimTime / ((float) currText.Length);
        animatingCR = StartCoroutine(StartTextAnimationCR(delayPerCharacter, 0));
    }

    private void StopTextCR(Coroutine animationCR)
    {
        if (animationCR != null)
        {
            StopCoroutine(animationCR);
        }
    }
    //private int characterIndex = 0f;

    private IEnumerator StartTextAnimationCR(float delayPerCharacter, int characterIndex, bool deanimate = false)
    {
        if (deanimate)
        {
            if (characterIndex == 0)
            {
                yield break;
            }
            characterIndex--;
        }
        else
        {
            if (characterIndex == currText.Length)
            {
                yield return HoldTextAnimationCR();
            }
            characterIndex++;
        }

        yield return new WaitForSeconds(delayPerCharacter);
        textMesh.text = currText.Substring(0, characterIndex);
        yield return StartTextAnimationCR(delayPerCharacter, characterIndex);
    }

    private IEnumerator HoldTextAnimationCR()
    {
        if (indefDisplayTime)
        {
            yield break;
        }

        yield return new WaitForSeconds(currDisplayTime);

        if (goToLastTextOnFinish)
        {
            AnimateAndSetText(previousATD);
        }
        else
        {
            DeanimateText();
        }
    }

    public void DeanimateText()
    {
        if (deanimatingCR != null)
        {
            Debug.Log("Tried deanimating an AnimatedText that was already denanimating.");
            return;
        }

        StopTextCR(animatingCR);
        float delayPerCharacter = ATDetails.DEANIMATE_TIME / ((float)textMesh.text.Length);
        deanimatingCR = StartCoroutine(StartTextAnimationCR(delayPerCharacter, textMesh.text.Length, true));
    }




    ////TODO: once we know the component we want to use for text
    ////make sure we check for only one of those components were gameobject,
    ////or should we have multiple of those components and each AT has its own
    ////component?

    ////TODO:
    ////private [component we plan to use] textComponent...

    //private GameObject textGO;

    //public void Awake()
    //{
    //    SetInitialFields();
    //}

    //public void SetInitialFields()
    //{
    //    CurrText = initialText;
    //    CurrColor = initialColor;
    //    CurrSize = initialSize;
    //    CurrSpeed = initialSpeed;
    //    CurrAnchor = initialAnchor;
    //    CurrAnimDirection = initialAnimDirection;
    //    CurrAlignment = initialAlignment;
    //    CurrDisplayTime = initialDisplayTime;
    //    CurrCustomDisplayTime = initialCustomDisplayTime;
    //}

    //public bool IsAnchorAstro()
    //{
    //    return CurrAnchor == AT_ANCHOR.ASTRO_LEFT
    //        || CurrAnchor == AT_ANCHOR.ASTRO_RIGHT
    //        || CurrAnchor == AT_ANCHOR.ASTRO_FRONT
    //        || CurrAnchor == AT_ANCHOR.ASTRO_BEHIND
    //        || CurrAnchor == AT_ANCHOR.ASTRO_CUSTOM;
    //}

    //public bool IsAnchorCamera()
    //{
    //    return CurrAnchor == AT_ANCHOR.TOP_RIGHT
    //        || CurrAnchor == AT_ANCHOR.TOP_LEFT
    //        || CurrAnchor == AT_ANCHOR.TOP_CENTER
    //        || CurrAnchor == AT_ANCHOR.CENTER
    //        || CurrAnchor == AT_ANCHOR.BOTTOM_CENTER
    //        || CurrAnchor == AT_ANCHOR.BOTTOM_LEFT
    //        || CurrAnchor == AT_ANCHOR.BOTTOM_RIGHT;
    //}

    //public void StartAnimBasedOnAnchor(A_Interactive interactiveOwner = null)
    //{
    //    if (IsAnchorAstro())
    //    {
    //        if (interactiveOwner == null)
    //        {
    //            Debug.LogError(string.Format("An interactive owner is needed for queuing this animated text '{0}' to the interactive system", CurrText));
    //            return;
    //        }
    //        S_AstroInteractiveQueue.Current.QueueInteractiveText(interactiveOwner, this, textGO.transform.localPosition);
    //    }

    //    else if (IsAnchorCamera())
    //    {
    //        StartAnim(/*TODO: set parent to camera*/);
    //    }
    //    else //LOCAL_POS 
    //    {
    //        StartAnim();
    //    }
    //}

    ////TODO: lock changing text if animating?
    //public void StartAnim(Transform newParent = null)
    //{
    //    if (newParent != null)
    //    {
    //        CurrParent = newParent;
    //    }

    //    //TODO: check to make sure parent is camera or astro based on anchor

    //    if (textGO == null)
    //    {
    //        Debug.LogError(string.Format("No gameobject exists for the text '{0}' because it was never assigned a parent gameobject :(", initialText));
    //        return;
    //    }

    //    //TODO: start anim
    //    Debug.Log(string.Format("started typing text '{0}'", CurrText));
    //    S_AstroInteractiveQueue.Current.StartCoroutine(SudoWaitForTime(ANIMATE_TIME, string.Format("ended typing text '{0}'", CurrText)));
    //}

    //private IEnumerator SudoWaitForTime(float animTime, string consoleText)
    //{
    //    yield return new WaitForSeconds(animTime);
    //    Debug.Log(consoleText);
    //}

    //public float StopAndClearAnim(bool deanimate)
    //{
    //    //TODO: implement
    //    if (!deanimate)
    //    {
    //        Debug.Log(string.Format("cleared text '{0}'", CurrText));
    //        return 0f;
    //    }
    //    Debug.Log(string.Format("started deanimating text '{0}'", CurrText));
    //    S_AstroInteractiveQueue.Current.StartCoroutine(SudoWaitForTime(DEANIMATE_TIME, string.Format("ended deanimating text {0}", CurrText)));
    //    return 0f;
    //}

    //public void ChangeATAndRestart(string newText, AT_COLOR newColor, AT_SIZE newSize, AT_DURATION newDuration)
    //{
    //    //TODO: implement
    //}

    //public void ChangeAnchor(AT_ANCHOR newAnchor, Vector2 customLocalPos = new Vector2())
    //{
    //    //TODO: implement
    //}

    //public void AdjustLocalPos(Vector2 newPos)
    //{
    //    //TODO: implement
    //}
}
