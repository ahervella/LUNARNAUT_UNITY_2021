using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;

public class AstroAnim : MonoBehaviour
{
    const string ANIMS_PATH = "Assets/ANIMATION/ASTRO/";
    public enum PLAYER_STATE { END1, END2, FALL, JUMP, LAND, RUN, STAND, START, DEATH, NONE }
    public enum SUIT { GGG, GGR, GRR, RGG, RRG, RRR }
    private const int GAME_FPS = 60;
    private Animator anim;
    private AnimatorOverrideController animOC;
    private string animIndex;
    private Dictionary<SUIT, Dictionary<PLAYER_STATE, AnimationClip>> animDict = new Dictionary<SUIT, Dictionary<PLAYER_STATE, AnimationClip>>();

    [Serializable]
    private class SuitPlayerStateEntry
    {
        [SerializeField]
        public SUIT suit;
        [SerializeField]
        public List<PlayerStateEntry> pse;
    }

    [Serializable]
    private class PlayerStateEntry
    {
        [SerializeField]
        public PLAYER_STATE ps;
        [SerializeField]
        public AnimationClip ac;
    }


    private SUIT currSuit = SUIT.GGG;
    private bool blinkToggle = true;
    private SUIT blinkOn = SUIT.GGG;
    private SUIT blinkOff = SUIT.GGG;
    private const float NORM_BLINK_TIME = 1.5f;
    private const float FAST_BLINK_TIME = 0.75f;
    private float blinkTime = NORM_BLINK_TIME;
    private Coroutine blinkCR;

    private PLAYER_STATE currState = PLAYER_STATE.STAND;
    private bool facingRight = true;
    private int astroHealth;
    private bool canChangePlayerState = true;
    private Coroutine switchAnimCR;

    public static event System.Action<PLAYER_STATE> OnAnimationStarted = delegate { };
    public static event System.Action<PLAYER_STATE> OnAnimationEnded = delegate { };

    private void Start()
    {
        anim = GetComponent<Animator>();
        animOC = new AnimatorOverrideController(anim.runtimeAnimatorController);
        anim.runtimeAnimatorController = animOC;
        animIndex = animOC.animationClips[0].name;

        animDict.Clear();
        foreach (SUIT suit in (SUIT[])Enum.GetValues(typeof(SUIT)))
        {
            string suitString = suit.ToString();
            Dictionary<PLAYER_STATE, AnimationClip> suitEntry = new Dictionary<PLAYER_STATE, AnimationClip>();

            foreach (PLAYER_STATE ps in (PLAYER_STATE[])Enum.GetValues(typeof(PLAYER_STATE)))
            {
                if (ps == PLAYER_STATE.DEATH)
                {
                    if (suit == SUIT.RRR)
                    {
                        string deathAnimPath = ANIMS_PATH + "/ASTRO_DEATH.anim";
                        suitEntry.Add(ps, (AnimationClip)AssetDatabase.LoadAssetAtPath(deathAnimPath, typeof(AnimationClip)));
                    }

                    continue;
                }

                string psString = ps.ToString();//nameof(ps);
                string path = ANIMS_PATH + suitString + "/ASTRO_" + suitString + "_" + psString + ".anim";

                suitEntry.Add(ps, (AnimationClip)AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)));
            }

            animDict.Add(suit, suitEntry);
        }

        astroHealth = AstroPlayer.STARTING_HEALTH;
        AstroPlayer.HealthUpdated += AstroPlayer_HealthUpdated;
        BlinkLoop();
    }


    private void AstroPlayer_HealthUpdated(int newHealth)
    {
        astroHealth = newHealth;
        SuitUpdate();
    }

    private void SuitUpdate()
    {
        if (facingRight)
        {
            switch (astroHealth)
            {
                case 4:
                    blinkOn = SUIT.GGG;
                    blinkOff = SUIT.GGG;
                    break;

                case 3:
                    blinkOn = SUIT.GGG;
                    blinkOff = SUIT.GGR;
                    break;

                case 2:
                    blinkOn = SUIT.GGR;
                    blinkOff = SUIT.GRR;
                    break;

                case 1:
                    blinkOn = SUIT.GRR;
                    blinkOff = SUIT.RRR;
                    break;

                case 0:
                    blinkOn = SUIT.RRR;
                    blinkOff = SUIT.RRR;
                    break;
            }
        }
        else
        {
            switch (astroHealth)
            {
                case 4:
                    blinkOn = SUIT.GGG;
                    blinkOff = SUIT.GGG;
                    break;

                case 3:
                    blinkOn = SUIT.RGG;
                    blinkOff = SUIT.GGG;
                    break;

                case 2:
                    blinkOn = SUIT.RGG;
                    blinkOff = SUIT.RRG;
                    break;

                case 1:
                    blinkOn = SUIT.RRG;
                    blinkOff = SUIT.RRR;
                    break;

                case 0:
                    blinkOn = SUIT.RRR;
                    blinkOff = SUIT.RRR;
                    break;
            }
        }
    }

    private IEnumerator BlinkLoop()
    {
        currSuit = blinkToggle ? blinkOn : blinkOff;
        blinkToggle = !blinkToggle;
        //TODO: Play sound here?
        yield return new WaitForSeconds(blinkTime);
        blinkCR = StartCoroutine(BlinkLoop());
    }

    private void OnAnimEnd(PLAYER_STATE animState)
    {
        if (GameIsOver() && animState != PLAYER_STATE.DEATH)
        {
            InitDeath();
            OnAnimationEnded(animState);
            return;
        }

        switch (animState)
        {
            case PLAYER_STATE.START:
                canChangePlayerState = true;
                if (currXDir != 0)
                {
                    SwitchAnimState(PLAYER_STATE.RUN);
                }
                else
                {
                    SwitchAnimState(PLAYER_STATE.END1);
                }
                break;

            case PLAYER_STATE.DEATH:
                return;

            case PLAYER_STATE.RUN:
                int currFrame = GetCurrFrame(animDict[currSuit][PLAYER_STATE.RUN], anim);
                if (currFrame > 40 || (currFrame > 0 && currFrame < 20))
                {
                    SwitchAnimState(PLAYER_STATE.END1);
                }
                else
                {
                    SwitchAnimState(PLAYER_STATE.END2);
                }
                
                break;

            case PLAYER_STATE.JUMP:
                //SwitchAnimState(PLAYER_STATE.LAND);
                //controls feel better this way
                //canChangePlayerState = true;
                break;


            case PLAYER_STATE.LAND:
                canChangePlayerState = true;
                if (currXDir != 0)
                {
                    SwitchAnimState(PLAYER_STATE.RUN);
                }
                else
                {
                    SwitchAnimState(PLAYER_STATE.END1);
                }
                break;

            case PLAYER_STATE.END1:
            case PLAYER_STATE.END2:
                canChangePlayerState = true;
                SwitchAnimState(PLAYER_STATE.STAND);
                break;
        }

        OnAnimationEnded(animState);

    }

    private int currXDir = 0;

    public void AnimLogicUpdate(bool grounded, bool jumping, bool falling, int xDir)
    {
        bool tempFacingRight = facingRight;
        currXDir = xDir;

        if (xDir != 0)
        {
            tempFacingRight = xDir > 0;
        }

        if (tempFacingRight != facingRight)
        {
            facingRight = tempFacingRight;
            UpdateOrientation();
            SuitUpdate();
        }


        if (grounded)
        {
            if (jumping && !falling && currState != PLAYER_STATE.JUMP)
            {
                DefaultInitAction(PLAYER_STATE.JUMP);
            }

            else if (falling && currState == PLAYER_STATE.FALL)
            {
                DefaultInitAction(PLAYER_STATE.LAND);
            }

            else if (xDir != 0 && (currState != PLAYER_STATE.RUN && currState != PLAYER_STATE.START))
            {
                DefaultInitAction(PLAYER_STATE.START);
            }
            else if (xDir == 0 && currState != PLAYER_STATE.STAND && currState != PLAYER_STATE.START && currState != PLAYER_STATE.END1 && currState != PLAYER_STATE.END2 )
            {
                OnAnimEnd(PLAYER_STATE.RUN);
            }
        }

        else if (jumping && falling && currState != PLAYER_STATE.FALL)
        {
            DefaultInitAction(PLAYER_STATE.FALL);
        }

    }

    private void UpdateOrientation()
    {
        float multiplyer = facingRight ? 1 : -1;
        transform.localScale = new Vector3(Math.Abs(transform.localScale.x) * multiplyer, transform.localScale.y, transform.localScale.z);
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
        SwitchAnimState(PLAYER_STATE.STAND);
    }

    public void Run()
    {
        SwitchAnimState(PLAYER_STATE.RUN);
    }

    private void DefaultInitAction(PLAYER_STATE state)
    {
        SwitchAnimState(state);
        canChangePlayerState = false;
    }

    void SwitchAnimState(PLAYER_STATE state, int startFrame = 0)
    {
        if (currState == state)
        {
            return;
        }
        currState = state;
        //PlayAAudioWrapper(state);
        CheckForActiveSwitchAnimCR();
        switchAnimCR = StartCoroutine(SwitchAnim(state, startFrame));
    }

    private void CheckForActiveSwitchAnimCR()
    {
        if (switchAnimCR != null)
        {
            StopCoroutine(switchAnimCR);
        }
    }

    private void SwitchAnimSuit()
    {
        if (switchAnimCR != null)
        {
            return;
        }
        //CheckForActiveSwitchAnimCR()
        AnimationClip ac = animDict[currSuit][currState];
        /*switchAnimCR = */ StartCoroutine(SwitchAnim(currState, GetCurrFrame(ac, anim), true, ac));

    }

    IEnumerator SwitchAnim(PLAYER_STATE state, int startFrame = 0, bool isSuitUpdate = false, AnimationClip ac = null)
    {
        

        if (!isSuitUpdate)
        {
            OnAnimationStarted(state);
            ac = animDict[currSuit][state];
        }
        
        int currHashState = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        float startNormTime = GetNormTimeFromFrame(ac, anim, startFrame);


        
        Debug.Log(state.ToString());

        //fuccccck this was such a bitch, need this specifically end of frame
        //or else playing from 0 wont happen at the same time as changing anim
        yield return new WaitForEndOfFrame();
        anim.Play(currHashState, 0, startNormTime);
        animOC[animIndex] = ac;
        switchAnimCR = null;
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
