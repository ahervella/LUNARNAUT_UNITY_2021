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

        public const float DEANIMATE_TIME = 0.5f;
        public const float DEFAULT_DISPLAY_TIME = 3.0f;

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
            { AT_SIZE.SMALL, 20f },
            { AT_SIZE.MED, 40f },
            { AT_SIZE.LARGE, 55f },
            { AT_SIZE.HUGE, 75f }
        };
        //.ToImmutableDictionary();

        public enum AT_ANIM_TIME { SLOW, NORM, FAST }
        public static readonly Dictionary<AT_ANIM_TIME, float> animTimeDict = new Dictionary<AT_ANIM_TIME, float>
        {
            { AT_ANIM_TIME.SLOW, 0.5f },
            { AT_ANIM_TIME.NORM, 1f },
            { AT_ANIM_TIME.FAST, 4f }
        };
        //.ToImmutableDictionary();

        public enum AT_ANCHOR { LOCAL_POS_RIGHT, LOCAL_POS_LEFT, LOCAL_POS_MIDDLE, ASTRO_LEFT, ASTRO_RIGHT, ASTRO_FRONT, ASTRO_BEHIND, TOP_RIGHT, TOP_LEFT, TOP_CENTER, CENTER, BOTTOM_CENTER, BOTTOM_LEFT, BOTTOM_RIGHT }
        public enum AT_ANIM_DIR { RIGHT, LEFT, MIDDLE }
        //public enum AT_ALIGNMENT { LEFT, CENTER, RIGHT }
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
        private AT_ANCHOR anchor = AT_ANCHOR.LOCAL_POS_RIGHT;
        public AT_ANCHOR Anchor => anchor;

        [SerializeField]
        private AT_ANIM_DIR animDirection = AT_ANIM_DIR.RIGHT;
        public AT_ANIM_DIR AnimDirection => animDirection;

        [SerializeField]
        private AT_DURATION displayTime = AT_DURATION.DEFAULT;
        public AT_DURATION DisplayTime => displayTime;

        [SerializeField]
        private float customDisplayTime = DEFAULT_DISPLAY_TIME;
        public float CustomDisplayTime => customDisplayTime;

        [SerializeField]
        private bool goToLastTextOnFinish = false;
        public bool GoToLastTextOnFinish => goToLastTextOnFinish;
    }

    private const float UNDERSCORE_BLINK_TIME = 1f;
    private const float TEXT_WIDTH_MULTIPLYER = (300f - 85.8f) / 18f;//18 / 213f;
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
        StopUnderscoreCR();
        textMesh.text = "";
        SetFromATDetails(atd);
        SetAnchorOffset();
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

        float halfWidthOfTextBox = textMesh.rectTransform.sizeDelta.x / 2f;
        textMesh.transform.localPosition = Vector3.zero;

        switch (atd.AnimDirection)
        {
            case ATDetails.AT_ANIM_DIR.LEFT:
                textMesh.transform.position += new Vector3(-halfWidthOfTextBox, 0, 0);
                //grows from right to left, so start on right side
                textMesh.alignment = TextAlignmentOptions.TopRight;
                break;

            case ATDetails.AT_ANIM_DIR.RIGHT:
                textMesh.transform.position += new Vector3(halfWidthOfTextBox, 0, 0);
                textMesh.alignment = TextAlignmentOptions.TopLeft;
                break;

            case ATDetails.AT_ANIM_DIR.MIDDLE:
                textMesh.alignment = TextAlignmentOptions.TopFlush;
                break;
        }

        goToLastTextOnFinish = atd.GoToLastTextOnFinish;
    }

    public float AnchorOffSetMultiplyer = 0f;

    private void SetAnchorOffset()
    {
        Vector3 pos = textMesh.transform.localPosition;
        textMesh.transform.localPosition += new Vector3(GetCurrTextWidth() * AnchorOffSetMultiplyer, 0, 0);
    }


    private Coroutine animatingCR;
    private Coroutine deanimatingCR;
    private Coroutine underscoreCR;

    private bool underscoreOn = false;

    private float GetCurrTextWidth()
    {
        /*
        string tempText = textMesh.text;
        textMesh.text = currText;
        float tempWidth = textMesh.renderedWidth;
        Debug.LogFormat("what is this shit: {0}", tempWidth);
        textMesh.text = tempText;
        return tempWidth;*/

        //TODO: find more accurate way to do this
        //TODO: ALSO need to accomodate for different text sizes
        return currText.Length * TEXT_WIDTH_MULTIPLYER;
    }

    private void StartTextAnimation()
    {
        //TODO: TEXT SOUNDS START HERE
        StopTextCR(ref deanimatingCR);
        StopTextCR(ref animatingCR);
        float delayPerCharacter = currAnimTime / ((float) currText.Length);
        animatingCR = StartCoroutine(StartTextAnimationCR(delayPerCharacter, 0));
    }

    private void StopTextCR(ref Coroutine animationCR)
    {
        if (animationCR != null)
        {
            StopCoroutine(animationCR);
            animationCR = null;
        }
    }

    private IEnumerator StartTextAnimationCR(float delayPerCharacter, int characterIndex, bool deanimate = false)
    {
        StopUnderscoreCR();

        if (deanimate)
        {
            if (characterIndex == 0)
            {
                yield break;
            }
            //TODO: SOUND FOR UNTYPING A LETTER HERE
            characterIndex--;
        }
        else
        {
            if (characterIndex == currText.Length)
            {
                yield return HoldTextAnimationCR();
                yield break;
              
            }
            //TODO: SOUND FOR TYPING A LETTER HERE
            characterIndex++;
        }

        yield return new WaitForSeconds(delayPerCharacter);
        textMesh.text = currText.Substring(0, characterIndex);
        yield return StartTextAnimationCR(delayPerCharacter, characterIndex, deanimate);
    }

    private IEnumerator HoldTextAnimationCR()
    {
        //TODO: SOUND FOR DONE TYPING
        underscoreCR = StartCoroutine(NextUnderscoreBlinkCR());

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


    private void StopUnderscoreCR()
    {
        if (underscoreOn)
        {
            //TODO: SOUND BLINK OFF HERE
            //this assumes we only are doing the blinking when the full text was displayed
            textMesh.text = currText;
            underscoreOn = false;
        }
        StopTextCR(ref underscoreCR);
    }

    private IEnumerator NextUnderscoreBlinkCR()
    {
        underscoreOn = !underscoreOn;
        textMesh.text = underscoreOn ? currText + "_" : currText;
        yield return new WaitForSeconds(UNDERSCORE_BLINK_TIME);
        yield return NextUnderscoreBlinkCR();
    }

    public void DeanimateText()
    {
        //TODO: SOUND FOR START UNTYPING
        if (deanimatingCR != null)
        {
            Debug.Log("Tried deanimating an AnimatedText that was already denanimating.");
            return;
        }

        StopTextCR(ref animatingCR);
        float delayPerCharacter = ATDetails.DEANIMATE_TIME / ((float)textMesh.text.Length);
        deanimatingCR = StartCoroutine(StartTextAnimationCR(delayPerCharacter, textMesh.text.Length, deanimate: true));
    }

    public void SetCustomTextWidth(float width)
    {
        textMesh.rectTransform.sizeDelta = new Vector2(width, textMesh.rectTransform.sizeDelta.y);
    }
}
