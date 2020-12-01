using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AstroAnim : MonoBehaviour
{
    public enum ANIM { END1, END2, FALL, JUMP, LAND, RUN, STAND, START, DEATH, NONE }
    const int GAME_FPS = 60;
    Animator anim;
    AnimatorOverrideController animOC;
    string animIndex;
    Dictionary<string, AnimationClip> animDict = new Dictionary<string, AnimationClip>();


    public static event System.Action<ANIM> onAnimationStarted = delegate { };

    private void Start()
    {
        anim = GetComponent<Animator>();
        animOC = new AnimatorOverrideController(anim.runtimeAnimatorController);
        animIndex = animOC.animationClips[0].name;
    }



    IEnumerator switchAnim(ANIM state, int startFrame = 0)
    {
        onAnimationStarted(state);


        string stateString = state.ToString();


        int currState = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        float startNormTime = getNormTimeFromFrame(animDict[stateString], anim, startFrame);


        anim.Play(currState, 0, startNormTime);


        //fuccccck this was such a bitch, need this specifically end of frame
        //or else playing from 0 wont happen at the same time as changing anim
        yield return new WaitForEndOfFrame();


        animOC[animIndex] = animDict[stateString];

    }


    void onAnimEnd(AnimationClip animClip)
    {
        ANIM animState = ANIM.NONE;

        foreach (ANIM st in System.Enum.GetValues(typeof(ANIM)))
        {
            if (animClip.name == st.ToString())
            {
                animState = st;
                break;
            }
        }

        if (gameIsOver() && animState != ANIM.DEATH)
        {
            //initDeath();
            //onAnimationEnded(animState);
            return;
        }

        switch (animState)
        {
            case ANIM.DEATH:
                return;

            case ANIM.JUMP:
                switchAnimState(PLAYER_STATE.LAND_G);
                //controls feel better this way
                canChange = true;
                if (!canSprint && sprintCoolDownCoroutine == null)
                {
                    sprintCoolDownCoroutine = StartCoroutine(sprintCoolDown());
                }
                break;


            case PLAYER_STATE.LAND_G:
                canChange = true;
                switchAnimState(PLAYER_STATE.RUN);
                break;


            case PLAYER_STATE.DODGE_R:
            case PLAYER_STATE.ROLL:
                switchAnimState(PLAYER_STATE.RUN, 7);
                canChange = true;
                if (!canSprint && sprintCoolDownCoroutine == null)
                {
                    sprintCoolDownCoroutine = StartCoroutine(sprintCoolDown());
                }
                break;


            default:
                switchAnimState(PLAYER_STATE.RUN);
                canChange = true;
                if (!canSprint && sprintCoolDownCoroutine == null)
                {
                    sprintCoolDownCoroutine = StartCoroutine(sprintCoolDown());
                }
                break;
        }


        onAnimationEnded(animState);

    }


    void switchAnimState(ANIM an)
    {

    }


    int getCurrFrame(AnimationClip animClip, Animator animator)
    {
        float normTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        float animLength = animClip.length;
        float animFrameRate = animClip.frameRate;
        return (int)(normTime * animLength * animFrameRate);
    }

    float getTimeFromFrames(AnimationClip animClip, int frameCount)
    {
        return animClip.frameRate / GAME_FPS * frameCount;
    }

    float getNormTimeFromFrame(AnimationClip animClip, Animator animator, int frame)
    {
        float normTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        float animLength = animClip.length;
        float animFrameRate = animClip.frameRate;
        return (float)frame / (animLength * animFrameRate);
    }
}
