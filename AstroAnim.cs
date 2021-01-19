using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AstroAnim : MonoBehaviour
{
    public enum PLAYER_STATE { END1, END2, FALL, JUMP, LAND, RUN, STAND, START, DEATH, NONE }
    private const int GAME_FPS = 60;
    private Animator anim;
    private AnimatorOverrideController animOC;
    private string animIndex;
    private Dictionary<string, AnimationClip> animDict = new Dictionary<string, AnimationClip>();
    private PLAYER_STATE currState = PLAYER_STATE.NONE;
    private bool canChangePlayerState = true;

    public static event System.Action<PLAYER_STATE> OnAnimationStarted = delegate { };
    public static event System.Action<PLAYER_STATE> OnAnimationEnded = delegate { };

    private void Start()
    {
        anim = GetComponent<Animator>();
        animOC = new AnimatorOverrideController(anim.runtimeAnimatorController);
        animIndex = animOC.animationClips[0].name;
    }


    private void OnAnimEnd(AnimationClip animClip)
    {
        PLAYER_STATE animState = PLAYER_STATE.NONE;

        foreach (PLAYER_STATE st in System.Enum.GetValues(typeof(PLAYER_STATE)))
        {
            if (animClip.name == st.ToString())
            {
                animState = st;
                break;
            }
        }

        if (GameIsOver() && animState != PLAYER_STATE.DEATH)
        {
            InitDeath();
            OnAnimationEnded(animState);
            return;
        }

        switch (animState)
        {
            case PLAYER_STATE.DEATH:
                return;

            case PLAYER_STATE.RUN:
                int currFrame = GetCurrFrame(animDict["RUN"], anim);
                if (currFrame > 40 || (currFrame > 0 && currFrame < 20))
                {
                    SwitchAnim(PLAYER_STATE.END1);
                }
                else
                {
                    SwitchAnim(PLAYER_STATE.END2);
                }
                
                break;

            case PLAYER_STATE.JUMP:
                SwitchAnimState(PLAYER_STATE.LAND);
                //controls feel better this way
                canChangePlayerState = true;
                break;


            case PLAYER_STATE.LAND:
                canChangePlayerState = true;
                SwitchAnimState(PLAYER_STATE.RUN);
                break;

            case PLAYER_STATE.END1:
            case PLAYER_STATE.END2:
                canChangePlayerState = true;
                SwitchAnimState(PLAYER_STATE.STAND);
                break;
        }

        OnAnimationEnded(animState);

    }
    

    private bool GameIsOver()
    {
        return false;
    }

    private void InitDeath()
    {
        DefaultInitAction(PLAYER_STATE.DEATH);
    }

    public void Jump()
    {
        DefaultInitAction(PLAYER_STATE.JUMP);
    }

    public void Idle()
    {
        DefaultInitAction(PLAYER_STATE.STAND);
    }

    public void Run()
    {
        DefaultInitAction(PLAYER_STATE.RUN);
    }

    private void DefaultInitAction(PLAYER_STATE state)
    {
        SwitchAnimState(state);
        canChangePlayerState = false;
    }

    void SwitchAnimState(PLAYER_STATE state, int startFrame = 0)
    {
        currState = state;
        //PlayAAudioWrapper(state);
        StartCoroutine(SwitchAnim(state, startFrame));
    }

    IEnumerator SwitchAnim(PLAYER_STATE state, int startFrame = 0)
    {
        OnAnimationStarted(state);


        string stateString = state.ToString();


        int currState = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        float startNormTime = GetNormTimeFromFrame(animDict[stateString], anim, startFrame);


        anim.Play(currState, 0, startNormTime);


        //fuccccck this was such a bitch, need this specifically end of frame
        //or else playing from 0 wont happen at the same time as changing anim
        yield return new WaitForEndOfFrame();


        animOC[animIndex] = animDict[stateString];

    }


    //frame utilities

    private int GetCurrFrame(AnimationClip animClip, Animator animator)
    {
        float normTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        float animLength = animClip.length;
        float animFrameRate = animClip.frameRate;
        return (int)(normTime * animLength * animFrameRate);
    }

    float GetTimeFromFrames(AnimationClip animClip, int frameCount)
    {
        return animClip.frameRate / GAME_FPS * frameCount;
    }

    float GetNormTimeFromFrame(AnimationClip animClip, Animator animator, int frame)
    {
        float normTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        float animLength = animClip.length;
        float animFrameRate = animClip.frameRate;
        return (float)frame / (animLength * animFrameRate);
    }
}
