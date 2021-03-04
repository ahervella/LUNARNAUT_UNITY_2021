using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "new_animatedText", menuName = "ScriptableObjects/AnimatedText", order = 1)]
public class SO_AnimatedText : ScriptableObject
{
    public enum AT_COLOR { INFO, SUCCESS, WARNING, FAILURE }
    public enum AT_SIZE { SMALL, MED, LARGE, HUGE }
    public enum AT_ANIM_TIME { SLOW, NORM, FAST }
    public enum AT_ANCHOR { LOCAL_POS, TOP_RIGHT, TOP_LEFT, TOP_CENTER, CENTER, BOTTOM_CENTER, BOTTOM_LEFT, BOTTOM_RIGHT }
    public enum AT_ANIM_DIR { RIGHT, LEFT, MIDDLE }
    public enum AT_ALIGNMENT { LEFT, CENTER, RIGHT }
    public enum AT_DURATION { INDEFINITE, DEFAULT, CUSTOM }
    public const float DEFAULT_DURATION = 3.0f;

    [SerializeField]
    private GameObject initialParent;

    [SerializeField]
    private string initialText;

    [SerializeField]
    private AT_COLOR initialColor = AT_COLOR.INFO;

    [SerializeField]
    private AT_SIZE initialSize = AT_SIZE.MED;

    [SerializeField]
    private AT_ANIM_TIME initialSpeed = AT_ANIM_TIME.NORM;

    [SerializeField]
    private AT_ANCHOR initialAnchor = AT_ANCHOR.BOTTOM_CENTER;

    [SerializeField]
    private AT_ANIM_DIR initialAnimDirection = AT_ANIM_DIR.RIGHT;

    [SerializeField]
    private AT_ALIGNMENT initialAlignment = AT_ALIGNMENT.LEFT;

    [SerializeField]
    private AT_DURATION initialDisplayTime = AT_DURATION.DEFAULT;

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
        SetParent(initialParent);
        //TODO: set all initial variables to current text gameobject
    }

    public void SetParent(GameObject newParent)
    {
        if (newParent == null)
        {
            Debug.LogError(string.Format("We don't ever want to make AnimatedTexts (namely '{0}') not have a parent!", initialText));
            return;
        }

        if (textGO == null)
        {
            textGO = new GameObject();
        }

        textGO.transform.parent = newParent.transform;
        textGO.transform.localPosition = Vector3.zero;
        textGO.transform.localEulerAngles = Vector3.zero;
    }

    public void StartAnim(GameObject newParent = null)
    {
        if (newParent != null)
        {
            SetParent(newParent);
        }

        if (textGO == null)
        {
            Debug.LogError(string.Format("No gameobject exists for the text '{0}' because it was never assigned a parent gameobject :(", initialText));
            return;
        }
    }

    public float StopAndClearAnim(bool deanimate)
    {
        //TODO: implement
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
}
