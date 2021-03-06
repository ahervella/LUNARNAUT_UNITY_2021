using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AstroAnim : Pausable
{
    [SerializeField]
    private SO_AstroAnimSettings settings = null;

    private const bool PRINT_ANIM_NAMES = false;

    const string ANIMS_PATH = "Assets/ANIMATION/ASTRO/";
    public enum PLAYER_STATE { END1, END2, FALL, JUMP, LAND, RUN, STAND, START, DEATH, NONE }
    public enum SUIT { GGG, GGR, GRR, RGG, RRG, RRR }
    private const int GAME_FPS = 60;
    private Animator anim;
    private AnimatorOverrideController animOC;
    private string animIndex;
    //private Dictionary<SUIT, Dictionary<PLAYER_STATE, AnimationClip>> animDict = new Dictionary<SUIT, Dictionary<PLAYER_STATE, AnimationClip>>();

    private Dictionary<PLAYER_STATE, int[]> footstepHeelSoundDict = new Dictionary<PLAYER_STATE, int[]>()
    {
        //frames that step happens for RUN anim (first frame = 0)
        {PLAYER_STATE.RUN, new int[]{15, 38} },
        {PLAYER_STATE.END1, new int[]{6} },
        {PLAYER_STATE.END2, new int[]{6} }
    };

    private Dictionary<PLAYER_STATE, int[]> footstepToeSoundDict = new Dictionary<PLAYER_STATE, int[]>()
    {
        {PLAYER_STATE.RUN, new int[]{19, 42} },
        {PLAYER_STATE.END1, new int[]{9} },
        {PLAYER_STATE.END2, new int[]{9} }
    };

    private Dictionary<PLAYER_STATE, int[]> footstepScrapeSoundDict = new Dictionary<PLAYER_STATE, int[]>()
    {
        {PLAYER_STATE.RUN, new int[]{26, 3} }
    };

    int lastFramedPlayed;

    private SUIT currSuit = SUIT.GGG;
    private bool blinkToggle = true;
    private SUIT blinkOn = SUIT.GGG;
    private SUIT blinkOff = SUIT.GGG;
    private const float NORM_BLINK_TIME = 1.5f;
    private const float FAST_BLINK_TIME = 0.75f;
    private float blinkTime = NORM_BLINK_TIME;
    private Coroutine blinkCR;

    private PLAYER_STATE currState = PLAYER_STATE.STAND;
    private bool _facingRight;
    public bool FacingRight
    {
        get => _facingRight;
        private set
        {
            _facingRight = value;
            UpdateOrientation();
        }
    }
    private int astroHealth;
    //private bool canChangePlayerState = true;
    private Coroutine switchAnimCR;

    public static event System.Action<PLAYER_STATE> OnAnimationStarted = delegate { };
    public static event System.Action<PLAYER_STATE> OnAnimationEnded = delegate { };
    public static event System.Action<float> OnOrientationUpdate = delegate { };
    public static event System.Action<SUIT> SuitChanged = delegate { };

    [Header("Audio Properties")]
    [SerializeField]
    private GameObject wwiseCollider;
    [SerializeField]
    private AK.Wwise.Event jumpSoundEvent;
    [SerializeField]
    private AK.Wwise.Event landSoundEvent;
    [SerializeField]
    private AK.Wwise.Event footstepHeelSoundEvent;
    [SerializeField]
    private AK.Wwise.Event footstepToeSoundEvent;
    [SerializeField]
    private AK.Wwise.Event footstepScrapeSoundEvent;

    [SerializeField]
    private AK.Wwise.Event suitBeepSoundEvent;

    protected override void OnAwake()
    {
        S_DeveloperTools_Delegates();
        S_TimeTravel_Delegates();
        S_TimeTravel_Init();

        //TODO (from 2022): AstroColorChanger doesn't seem to be working properly for now so
        //I made an astro anim settings, connected it to the blink method
    }

    #region Astro Dev Tools Integration
    private void S_DeveloperTools_Delegates()
    {
        S_DeveloperTools_EnableChanged();

        S_DeveloperTools.Current.EnableDevToolsChanged -= S_DeveloperTools_EnableChanged;
        S_DeveloperTools.Current.EnableDevToolsChanged += S_DeveloperTools_EnableChanged;
        S_DeveloperTools.Current.AstroPlayerDevToolsChanged -= S_DeveloperTools_EnableChanged;
        S_DeveloperTools.Current.AstroPlayerDevToolsChanged += S_DeveloperTools_EnableChanged;
    }

    private void S_DeveloperTools_EnableChanged()
    {
        S_DeveloperTools.Current.PrintAstroAnimsChanged -= S_DeveloperTools_PrintAstroAnimsChanged;

        if (S_DeveloperTools.Current.EnableDevTools && S_DeveloperTools.Current.AstroPlayerDevTools)
        {
            S_DeveloperTools.Current.PrintAstroAnimsChanged += S_DeveloperTools_PrintAstroAnimsChanged;
        }

        S_DevloperTools_ManualUpdate();
    }

    private void S_DevloperTools_ManualUpdate()
    {
        S_DeveloperTools_PrintAstroAnimsChanged();
    }

    private bool DEVTOOLS_printAstroAnims = false;
    private void S_DeveloperTools_PrintAstroAnimsChanged()
    {
        if (!S_DeveloperTools.Current.DevToolsEnabled_ASTRO_PLAYER())
        {
            DEVTOOLS_printAstroAnims = false;
            return;
        }

        DEVTOOLS_printAstroAnims = S_DeveloperTools.Current.PrintAstroAnims;
    }
    #endregion

    #region TIME_TRAVEL
    private void S_TimeTravel_Delegates()
    {

        S_TimeTravel.Current.ParseAstroTTD -= S_TimeTravel_ParseAstroTTD;
        S_TimeTravel.Current.ParseAstroTTD += S_TimeTravel_ParseAstroTTD;
    }

    private void S_TimeTravel_Init()
    {
        pastFacingRight = pastStartFacingRight;
        futureFacingRight = futureStartFacingRight;
        FacingRight = S_TimeTravel.Current.InFuture() ? futureFacingRight : pastFacingRight;
    }

    //these are set initially in awake
    [SerializeField]
    private bool pastStartFacingRight;
    private bool pastFacingRight;

    [SerializeField]
    private bool futureStartFacingRight;
    private bool futureFacingRight;

    private void S_TimeTravel_ParseAstroTTD()
    {
        if (S_TimeTravel.Current.InFuture())
        {
            pastFacingRight = FacingRight;
            FacingRight = futureFacingRight;
        }
        else
        {
            futureFacingRight = FacingRight;
            FacingRight = pastFacingRight;
        }
    }

    #endregion

    private void Start()
    {
        anim = GetComponent<Animator>();
        animOC = new AnimatorOverrideController(anim.runtimeAnimatorController);
        anim.runtimeAnimatorController = animOC;
        animIndex = animOC.animationClips[0].name;

        astroHealth = 3;//AstroPlayer.MAX_HEALTH;
        SuitUpdate();
        AstroPlayer.HealthUpdated += AstroPlayer_HealthUpdated;
        blinkCR = StartCoroutine(BlinkLoop());
    }


    private void AstroPlayer_HealthUpdated(int newHealth)
    {
        astroHealth = newHealth;
        SuitUpdate();
    }

    private void SuitUpdate()
    {
        if (FacingRight)
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
        currSuit = blinkToggle ? blinkOn : blinkOff;
        SuitChanged(currSuit);
    }

    private IEnumerator BlinkLoop()
    {  
        yield return new WaitForSeconds(blinkTime);

        if (blinkToggle)
        {
            currSuit = blinkOn;
            suitBeepSoundEvent.Post(wwiseCollider);
        }
        else
        {
            currSuit = blinkOff;
        }

        blinkToggle = !blinkToggle;
        SwitchAnimSuit();

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
                //canChangePlayerState = true;
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
                int currFrame = GetCurrFrame(currSuit, PLAYER_STATE.RUN, anim);
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
                //canChangePlayerState = true;
                if (currXDir != 0)
                {
                    SwitchAnimState(PLAYER_STATE.RUN);
                }
                else
                {
                    //end1 has the same frames from frames 1-6 as land, so lets no repeat those
                    SwitchAnimState(PLAYER_STATE.END1, 7);
                }
                break;

            case PLAYER_STATE.END1:
            case PLAYER_STATE.END2:
                //canChangePlayerState = true;
                SwitchAnimState(PLAYER_STATE.STAND);
                break;
        }

        OnAnimationEnded(animState);

    }

    private int currXDir = 0;

    private bool grounded;
    private bool jumping;
    private bool falling;
    private int xDir;

    public void AnimLogicFixedUpdate(bool grounded, bool jumping, bool falling, int xDir)
    {
        this.grounded = grounded;
        this.jumping = jumping;
        this.falling = falling;
        this.xDir = xDir;
    }

    public void AnimLogicLateUpdate()
    {
        bool tempFacingRight = FacingRight;
        currXDir = xDir;

        if (xDir != 0)
        {
            tempFacingRight = xDir > 0;
        }

        if (tempFacingRight != FacingRight)
        {
            FacingRight = tempFacingRight;
            SuitUpdate();
            //SwitchAnimSuit();
        }

        //if (grounded)

        if (grounded)
        {
            if (jumping && !falling && currState != PLAYER_STATE.JUMP)
            {
                DefaultInitAction(PLAYER_STATE.JUMP);
                jumpSoundEvent.Post(wwiseCollider);
            }

            else if (falling && currState == PLAYER_STATE.FALL)
            {
                DefaultInitAction(PLAYER_STATE.LAND);


                //Plays the Wwise audio event with the corresponding string name (arg. 1) on the object (arg. 2).
                //See my documentation for audio names (This is in progress)
                landSoundEvent.Post(wwiseCollider);
            }

            //so we don't run in mid air lol
            //also need to let the land animation finish
            else if (jumping || currState == PLAYER_STATE.LAND)
            {
                return;
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

        else if (falling && currState != PLAYER_STATE.FALL)
        {
            DefaultInitAction(PLAYER_STATE.FALL);
        }
        else if (!falling && currState != PLAYER_STATE.JUMP)
        {
            DefaultInitAction(PLAYER_STATE.JUMP);
            jumpSoundEvent.Post(wwiseCollider);
        }
    }

    private void UpdateOrientation()
    {
        float multiplyer = FacingRight ? 1 : -1;
        transform.localScale = new Vector3(Math.Abs(transform.localScale.x) * multiplyer, transform.localScale.y, transform.localScale.z);
        OnOrientationUpdate(multiplyer);
    }


    //TODO: optimize by just putting in the animation frames? Is that faster?
    private void LateUpdate()
    {
        if (S_MENUS_gamePaused)
        {
            return;
        }

        //TODO: do we still want to keep these in the fixed update?
        //Should be fine because we already save for all animation things
        // to change end of frame? What about for getting the curr frame?
        AnimLogicLateUpdate();
        UpdatePlayFootstepSound();
    }

    //Used in animation trigger
    private void UpdatePlayFootstepSound()
    {

        int currFrame = GetCurrFrame(currSuit, currState, anim);
        //Debug.LogFormat("The current Astro frame: {0}", currFrame);

        if (lastFramedPlayed == currFrame)
        {
            return;
        }

        lastFramedPlayed = currFrame;
        //TODO: hook this up in dev tools
        //Debug.Log(String.Format("CurrAnim = {0}, CurrFrame{1}", currState.ToString(), currFrame));


        if (footstepHeelSoundEvent == null)
        {
            return;
        }

        if (!footstepHeelSoundDict.ContainsKey(currState))
        {
            return;
        }

        if (footstepHeelSoundDict.ContainsKey(currState))
        {
            int[] frames = footstepHeelSoundDict[currState];
            foreach (int frame in frames)
            {
                if (frame == currFrame)
                {
                    footstepHeelSoundEvent.Post(wwiseCollider);
                    return;
                }
            }

        }

        if (footstepToeSoundDict.ContainsKey(currState))
        {
            int[] frames = footstepToeSoundDict[currState];
            foreach (int frame in frames)
            {
                if (frame == currFrame)
                {
                    footstepToeSoundEvent.Post(wwiseCollider);
                    return;
                }
            }
        }
        
        if (footstepScrapeSoundDict.ContainsKey(currState))
        {
            int[] frames = footstepScrapeSoundDict[currState];
            foreach (int frame in frames)
            {
                if (frame == currFrame)
                {
                    footstepScrapeSoundEvent.Post(wwiseCollider);
                    return;
                }
            }

        }
        
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
        //canChangePlayerState = false;
    }

    void SwitchAnimState(PLAYER_STATE state, int startFrame = 1)
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
        //we always want any previously triggered animation change on this frame
        //to take priority b/c this method of switching anims ONLY changes suit states
        if (switchAnimCR != null)
        {
            return;
        }
        AnimationClip ac = settings.GetAstroAnim(currSuit, currState);
        switchAnimCR =  StartCoroutine(SwitchAnim(currState, -1, true, ac, true));

    }

    IEnumerator SwitchAnim(PLAYER_STATE state, int startFrame = 0, bool isSuitUpdate = false, AnimationClip ac = null, bool continueOnSameFrame = false)
    {


        //fuccccck this was such a bitch, need this specifically end of frame
        //or else playing from 0 wont happen at the same time as changing anim
        yield return new WaitForEndOfFrame();

        if (!isSuitUpdate)
        {
            OnAnimationStarted(state);
            ac = settings.GetAstroAnim(currSuit, state);
        }
        
        int currHashState = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        

        if (DEVTOOLS_printAstroAnims) { Debug.Log(state.ToString()); }

        //We do not do % 1 on time here so that we can notice when we're not setting animations to loop on ones we expect to (for debugging)
        float startNormTime = continueOnSameFrame ? anim.GetCurrentAnimatorStateInfo(0).normalizedTime : GetNormTimeFromFrame(ac, anim, startFrame);
        anim.Play(currHashState, 0, startNormTime);
        animOC[animIndex] = ac;
        switchAnimCR = null;
    }


    //frame utilities

    private int GetCurrFrame(SUIT suit, PLAYER_STATE state, Animator animator)
    {
        AnimationClip animClip = settings.GetAstroAnim(suit, state);
        //modulos b/c integer part of nubmer represents number of loops, float percent of loop
        float normTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;
        float animLength = animClip.length;
        float animFrameRate = animClip.frameRate;
        //need to round up to get the frame it's currently on (ex. time 0 = frame 1)
        return (int) Mathf.Ceil(normTime * animLength * animFrameRate);
    }

    float GetTimeFromFrames(AnimationClip animClip, int frameCount)
    {
        return animClip.frameRate / GAME_FPS * frameCount;
    }

    float GetNormTimeFromFrame(AnimationClip animClip, Animator animator, int frame)
    {
        float animLength = animClip.length;
        float animFrameRate = animClip.frameRate;
        //Minus 1 so that frame 1 lets start at time 0
        return (frame-1) / (animLength * animFrameRate);
    }

    private void OnDestroy()
    {
        OnAnimationStarted = delegate { };
        OnAnimationEnded = delegate { };
        OnOrientationUpdate = delegate { };
        SuitChanged = delegate { };
    }
}
