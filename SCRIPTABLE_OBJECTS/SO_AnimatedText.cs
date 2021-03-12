using UnityEngine;
using System.Collections;

//TODO: make a styalizing system?
[CreateAssetMenu(fileName = "new_animatedText", menuName = "ScriptableObjects/AnimatedText", order = 1)]
public class SO_AnimatedText : ScriptableObject
{
    public const float ANIMATE_TIME = 2f;
    public const float DEANIMATE_TIME = 0.5f;
    public enum AT_COLOR { INFO, SUCCESS, WARNING, FAILURE }
    public enum AT_SIZE { SMALL, MED, LARGE, HUGE }
    public enum AT_ANIM_TIME { SLOW, NORM, FAST }
    public enum AT_ANCHOR { LOCAL_POS, ASTRO_LEFT, ASTRO_RIGHT, ASTRO_FRONT, ASTRO_BEHIND, ASTRO_CUSTOM, TOP_RIGHT, TOP_LEFT, TOP_CENTER, CENTER, BOTTOM_CENTER, BOTTOM_LEFT, BOTTOM_RIGHT }
    public enum AT_ANIM_DIR { RIGHT, LEFT, MIDDLE }
    public enum AT_ALIGNMENT { LEFT, CENTER, RIGHT }
    public enum AT_DURATION { INDEFINITE, DEFAULT, CUSTOM }
    public const float DEFAULT_DURATION = 3.0f;

    //Need to do this pater with getters and what not so that the SO does not get its values changed during runtime

    public Transform CurrParent
    {
        get { return textGO.transform.parent; }
        set
        {
            /*
            if (newParent == null)
            {
                Debug.LogError(string.Format("We don't ever want to make AnimatedTexts (namely '{0}') not have a parent!", initialText));
                return;
            }*/

            if (textGO == null)
            {
                textGO = new GameObject();
            }

            textGO.transform.parent = value;
            textGO.transform.localPosition = Vector3.zero;
            textGO.transform.localEulerAngles = Vector3.zero;
        }
    }

    private string _currText;
    //TODO: implement with planed text component
    public string CurrText { get => _currText; set { _currText = value; } }
    [SerializeField]
    private string initialText;

    private AT_COLOR _currColor;
    public AT_COLOR CurrColor { get => _currColor; set { _currColor = value; } }
    [SerializeField]
    private AT_COLOR initialColor = AT_COLOR.INFO;

    private AT_SIZE _currSize;
    public AT_SIZE CurrSize { get => _currSize; set { _currSize = value; } }
    [SerializeField]
    private AT_SIZE initialSize = AT_SIZE.MED;

    private AT_ANIM_TIME _currSpeed;
    public AT_ANIM_TIME CurrSpeed { get => _currSpeed; set { _currSpeed = value; } }
    [SerializeField]
    private AT_ANIM_TIME initialSpeed = AT_ANIM_TIME.NORM;

    private AT_ANCHOR _currAnchor;
    public AT_ANCHOR CurrAnchor { get => _currAnchor; set { _currAnchor = value; } }
    [SerializeField]
    private AT_ANCHOR initialAnchor = AT_ANCHOR.BOTTOM_CENTER;

    private AT_ANIM_DIR _currAnimDirection;
    public AT_ANIM_DIR CurrAnimDirection { get => _currAnimDirection; set { _currAnimDirection = value; } }
    [SerializeField]
    private AT_ANIM_DIR initialAnimDirection = AT_ANIM_DIR.RIGHT;

    private AT_ALIGNMENT _currAlignment;
    public AT_ALIGNMENT CurrAlignment { get => _currAlignment; set { _currAlignment = value; } }
    [SerializeField]
    private AT_ALIGNMENT initialAlignment = AT_ALIGNMENT.LEFT;

    private AT_DURATION _currDisplayTime;
    public AT_DURATION CurrDisplayTime { get => _currDisplayTime; set { _currDisplayTime = value; } }
    [SerializeField]
    private AT_DURATION initialDisplayTime = AT_DURATION.DEFAULT;

    private float _currCustomDisplayTime;
    public float CurrCustomDisplayTime { get => _currCustomDisplayTime; set { _currCustomDisplayTime = value; } }
    [SerializeField]
    private float initialCustomDisplayTime = 0f;

    //TODO: once we know the component we want to use for text
    //make sure we check for only one of those components were gameobject,
    //or should we have multiple of those components and each AT has its own
    //component?

    //TODO:
    //private [component we plan to use] textComponent...

    private GameObject textGO;

    public void Awake()
    {
        SetInitialFields();
    }

    public void SetInitialFields()
    {
        CurrText = initialText;
        CurrColor = initialColor;
        CurrSize = initialSize;
        CurrSpeed = initialSpeed;
        CurrAnchor = initialAnchor;
        CurrAnimDirection = initialAnimDirection;
        CurrAlignment = initialAlignment;
        CurrDisplayTime = initialDisplayTime;
        CurrCustomDisplayTime = initialCustomDisplayTime;
    }

    public bool IsAnchorAstro()
    {
        return CurrAnchor == AT_ANCHOR.ASTRO_LEFT
            || CurrAnchor == AT_ANCHOR.ASTRO_RIGHT
            || CurrAnchor == AT_ANCHOR.ASTRO_FRONT
            || CurrAnchor == AT_ANCHOR.ASTRO_BEHIND
            || CurrAnchor == AT_ANCHOR.ASTRO_CUSTOM;
    }

    public bool IsAnchorCamera()
    {
        return CurrAnchor == AT_ANCHOR.TOP_RIGHT
            || CurrAnchor == AT_ANCHOR.TOP_LEFT
            || CurrAnchor == AT_ANCHOR.TOP_CENTER
            || CurrAnchor == AT_ANCHOR.CENTER
            || CurrAnchor == AT_ANCHOR.BOTTOM_CENTER
            || CurrAnchor == AT_ANCHOR.BOTTOM_LEFT
            || CurrAnchor == AT_ANCHOR.BOTTOM_RIGHT;
    }

    public void StartAnimBasedOnAnchor(A_Interactive interactiveOwner = null)
    {
        if (IsAnchorAstro())
        {
            if (interactiveOwner == null)
            {
                Debug.LogError(string.Format("An interactive owner is needed for queuing this animated text '{0}' to the interactive system", CurrText));
                return;
            }
            S_AstroInteractiveQueue.Current.QueueInteractiveText(interactiveOwner, this, textGO.transform.localPosition);
        }

        else if (IsAnchorCamera())
        {
            StartAnim(/*TODO: set parent to camera*/);
        }
        else //LOCAL_POS 
        {
            StartAnim();
        }
    }

    //TODO: lock changing text if animating?
    public void StartAnim(Transform newParent = null)
    {
        if (newParent != null)
        {
            CurrParent = newParent;
        }

        //TODO: check to make sure parent is camera or astro based on anchor

        if (textGO == null)
        {
            Debug.LogError(string.Format("No gameobject exists for the text '{0}' because it was never assigned a parent gameobject :(", initialText));
            return;
        }

        //TODO: start anim
        Debug.Log(string.Format("started typing text '{0}'", CurrText));
        S_AstroInteractiveQueue.Current.StartCoroutine(SudoWaitForTime(ANIMATE_TIME, string.Format("ended typing text '{0}'", CurrText)));
    }

    private IEnumerator SudoWaitForTime(float animTime, string consoleText)
    {
        yield return new WaitForSeconds(animTime);
        Debug.Log(consoleText);
    }

    public float StopAndClearAnim(bool deanimate)
    {
        //TODO: implement
        if (!deanimate)
        {
            Debug.Log(string.Format("cleared text '{0}'", CurrText));
            return 0f;
        }
        Debug.Log(string.Format("started deanimating text '{0}'", CurrText));
        S_AstroInteractiveQueue.Current.StartCoroutine(SudoWaitForTime(DEANIMATE_TIME, string.Format("ended deanimating text {0}", CurrText)));
        return 0f;
    }

    public void ChangeATAndRestart(string newText, AT_COLOR newColor, AT_SIZE newSize, AT_DURATION newDuration)
    {
        //TODO: implement
    }

    public void ChangeAnchor(AT_ANCHOR newAnchor, Vector2 customLocalPos = new Vector2())
    {
        //TODO: implement
    }

    public void AdjustLocalPos(Vector2 newPos)
    {
        //TODO: implement
    }
}
