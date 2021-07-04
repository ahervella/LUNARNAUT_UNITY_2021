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
    public TextMeshProUGUI TextMesh => textMesh;

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
        public const float DEFAULT_TEXT_BOX_WIDTH = 200f;

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
            { AT_ANIM_TIME.SLOW, 4f },
            { AT_ANIM_TIME.NORM, 1f },
            { AT_ANIM_TIME.FAST, 0.5f }
        };
        //.ToImmutableDictionary();

        public enum AT_ANCHOR { LOCAL_POS_RIGHT, LOCAL_POS_LEFT, LOCAL_POS_CENTER, ASTRO_LEFT, ASTRO_RIGHT, ASTRO_FRONT, ASTRO_BEHIND, TOP_RIGHT, TOP_LEFT, TOP_CENTER, CENTER, BOTTOM_CENTER, BOTTOM_LEFT, BOTTOM_RIGHT }
        public enum AT_ANCHOR_VERT { MIDDLE, ABOVE, BELOW }
        public enum AT_ANIM_DIR { RIGHT, LEFT, MIDDLE, FLUSH }
        //public enum AT_ALIGNMENT { LEFT, CENTER, RIGHT }
        public enum AT_DURATION { INDEFINITE, DEFAULT, CUSTOM }
        public enum AT_TEXT_BOX_WIDTH { PREFAB_DEFAULT, CUSTOM }

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
        private AT_ANCHOR_VERT anchorVertical = AT_ANCHOR_VERT.MIDDLE;
        public AT_ANCHOR_VERT AnchorVertical => anchorVertical;

        [SerializeField]
        private bool fixedSizeInCam = true;
        public bool FixedSizeInCam => fixedSizeInCam;

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
        private AT_TEXT_BOX_WIDTH textBoxWidth = AT_TEXT_BOX_WIDTH.CUSTOM;
        public AT_TEXT_BOX_WIDTH TextBoxWidth => textBoxWidth;

        [SerializeField]
        private float customTextBoxWidth = DEFAULT_TEXT_BOX_WIDTH;
        public float CustomTexBoxWidth => customTextBoxWidth;

        [SerializeField]
        private bool goToLastTextOnFinish = false;
        public bool GoToLastTextOnFinish => goToLastTextOnFinish;

        public ATDetails(string newText, ATDetails oldATD)
        {
            text = newText;
            textColor = oldATD.textColor;
            textSize = oldATD.textSize;
            animSpeed = oldATD.animSpeed;
            anchor = oldATD.Anchor;
            animDirection = oldATD.AnimDirection;
            displayTime = oldATD.DisplayTime;
            customDisplayTime = oldATD.CustomDisplayTime;
            goToLastTextOnFinish = oldATD.GoToLastTextOnFinish;
        }

        public bool ShouldAnimate()
        {
            return text != String.Empty;
        }
    }

    private const float UNDERSCORE_BLINK_TIME = 1f;
    private string currText;
    private float currSize;
    private float currAnimTime;
    private float currDisplayTime;
    private float currAnimDir;
    private bool indefDisplayTime = false;
    private bool goToLastTextOnFinish = false;

    public Transform FixedCamAnchor { get; set; }
    private bool followFixedCamAnchor = false;
    private Vector3 cachedLocalPosOffset;
    private float cachedLineCount;

    private ATDetails currATD;
    private ATDetails previousATD;

    private void Awake()
    {
        //should be fine using singleton here cause these are never instanced on game start
        textMesh.canvas.worldCamera = S_Global.Current.GetCamera();
    }

    public void StopFixedCamUpdates()
    {
        followFixedCamAnchor = false;
    }

    public void AnimateAndSetText(ATDetails atd)
    {
        StopUnderscoreCR();
        textMesh.text = "";
        SetFromATDetails(atd);
        //SetCamSettings();
        SetTextBoxWidth(atd);
        SetAnchorOffset();
        StartTextAnimation();
    }

    private Vector3 GetFixedCamAnchorPos()
    {
        if (FixedCamAnchor != null)
        {
            return FixedCamAnchor.position;
        }
       return textMesh.canvas.transform.parent.position;
    }

    private void FixedUpdate()
    {
        if (!followFixedCamAnchor) { return; }
        //SetAnchorOffset();
        textMesh.transform.position = GetFixedCamAnchorPos();//new Vector3(mahSpot.x, mahSpot.y - cachedLocalPosOffset.y / cachedLineCount, 0);
        textMesh.transform.localPosition = new Vector3(textMesh.transform.localPosition.x, textMesh.transform.localPosition.y, 10);
        textMesh.rectTransform.anchoredPosition += new Vector2(cachedLocalPosOffset.x, cachedLocalPosOffset.y - cachedLocalPosOffset.y / cachedLineCount);
    }

    private void SetFromATDetails(ATDetails atd)
    {
        previousATD = currATD;
        currATD = atd;

        //for doing proper new lines with wrapping words
        currText = atd.Text.Replace("_", "_ ");
        textMesh.color = ATDetails.colorDict[atd.TextColor];
        currSize = ATDetails.sizeDict[atd.TextSize];
        textMesh.fontSize = currSize;
        currAnimTime = ATDetails.animTimeDict[atd.AnimSpeed];
        followFixedCamAnchor = atd.FixedSizeInCam;// && !IsAstroAnchor(atd);

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



        //decide how to do sthis shiiiiiiiiit
        //when to actually compensate for typing direction:
        //TODO: restrictinos with checking camera fix button for astro, and camera ones?

        switch (atd.AnimDirection)
        {
            case ATDetails.AT_ANIM_DIR.LEFT:
                textMesh.alignment = TextAlignmentOptions.TopRight;
                currAnimDir = -1f;
                break;

            case ATDetails.AT_ANIM_DIR.RIGHT:
                textMesh.alignment = TextAlignmentOptions.TopLeft;
                currAnimDir = 1f;
                break;

            case ATDetails.AT_ANIM_DIR.MIDDLE:
                textMesh.alignment = TextAlignmentOptions.Top;
                currAnimDir = 0f;
                break;

            case ATDetails.AT_ANIM_DIR.FLUSH:
                currAnimDir = 0f;
                textMesh.alignment = TextAlignmentOptions.Top;
                break;
        }

        goToLastTextOnFinish = atd.GoToLastTextOnFinish;
    }

    public Vector2 AnchorOffSetMultiplyer = new Vector2(0, 0);//0f;

    private void SetTextBoxWidth(ATDetails atd)
    {
        float currHeight = textMesh.rectTransform.rect.height;
        float newWidth = atd.TextBoxWidth == ATDetails.AT_TEXT_BOX_WIDTH.CUSTOM ? atd.CustomTexBoxWidth : ATDetails.DEFAULT_TEXT_BOX_WIDTH;
        textMesh.rectTransform.sizeDelta = new Vector2(newWidth, currHeight);
    }

    private void SetAnchorOffset()
    {
        textMesh.transform.localPosition = Vector3.zero;
        textMesh.rectTransform.anchoredPosition = Vector3.zero;

        Vector3 textDim = GetCurrTextDimensions();

        float currTextBoxHeight = textMesh.rectTransform.rect.height;
        textMesh.rectTransform.sizeDelta = new Vector2(textDim.x + 1, currTextBoxHeight);

        float trueCenterOffset = GetCurrTextCenterOffset(textDim.x);

        cachedLocalPosOffset = new Vector2(textDim.x * AnchorOffSetMultiplyer.x + trueCenterOffset, textDim.y * AnchorOffSetMultiplyer.y);

        cachedLineCount = textDim.y / textDim.z;
        //adding because we set the animation direction. Should only be done once when getting new text
        //textMesh.rectTransform.anchoredPosition += new Vector2(cachedLocalPosOffset.x, cachedLocalPosOffset.y - textDim.z);
    }

    private Coroutine animatingCR;
    private Coroutine deanimatingCR;
    private Coroutine underscoreCR;

    private bool underscoreOn = false;

    private Vector3 GetCurrTextDimensions()
    {
        //this helped find the trick!
        //https://stackoverflow.com/questions/49262758/get-number-of-rows-of-a-text

        //need to know height of one line because TMP starts anchor below first line
        //also will need so we can figure out number of lines text has
        string temp = textMesh.text;
        textMesh.text = "_";
        textMesh.ForceMeshUpdate();

        float singleLineHeight = textMesh.renderedHeight;

        textMesh.text = currText + "_";
        textMesh.ForceMeshUpdate();

        float maxWidth = textMesh.renderedWidth;
        float maxHeight = textMesh.renderedHeight;

        textMesh.text = temp;
        textMesh.ForceMeshUpdate();
        Debug.LogFormat("width, height, line height: {0}, {1}, {2}", maxWidth, maxHeight, singleLineHeight);
        //just using a vect3 to save the singleLineHeight, nothing to do with z axis
        return new Vector3(maxWidth, maxHeight, singleLineHeight);
    }

    private float GetCurrTextCenterOffset(float textWidth)
    {
        float maxWidth = textMesh.rectTransform.rect.width;
        return (maxWidth - textWidth) / 2f * currAnimDir;
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
