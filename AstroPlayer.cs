using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
//using LunarnautShit;

//Tutorial I used to implement this awesome shit:
//https://learn.unity.com/tutorial/live-session-2d-platformer-character-controller#5c7f8528edbc2a002053b68e


public class AstroPlayer : MonoBehaviour
{
    private const string MOVING_PLATFORM_TAG = "MOVING_PLATFORM";
    private AstroAnim animController;

    private const float MIN_MOVE_DIST = 0.001f;
    //if this is min dist too small, game can break
    private const float GROUNDED_MIN_DIST = 1f;//0.25f;
    private const float GROUNDED_MIN_COLL_DIST = GROUNDED_MIN_DIST / 2;
    private const float MIN_GROUND_DEG_ANG = 55f;
    private const float SLIDE_FRIC_THRESHOLD = 0.6f;


    private const float ACCEL = 3f;
    private const float AIR_ACCEL = ACCEL / 2f;
    private const float MAX_SPEED = 180f / 2f;
    //TODO: implement max air speed
    private const float MAX_AIR_SPEED = MAX_SPEED;
    private const float GRAVITY = 180f / 2f;
    private const float TERMINAL_VEL = 200f / 2f;

    private const float JUMP_VERT_SPEED = 150f / 2f;
    private const float MAX_JUMP_TIME = 0.6f;

    public const int MAX_HEALTH = 4;

    //readonly allows for constants that need to be set at runtime
    private readonly KeyCode[] JUMP_INPUT_KEY = { KeyCode.UpArrow, KeyCode.W, KeyCode.Space };
    private readonly KeyCode[] RIGHT_INPUT_KEY = { KeyCode.RightArrow, KeyCode.D };
    private readonly KeyCode[] LEFT_INPUT_KEY = { KeyCode.LeftArrow, KeyCode.A };
    private readonly KeyCode[] TIME_TRAVEL_KEY = { KeyCode.Q };

    private bool jumpInput_DOWN = false;
    private bool jumpInput_UP = false;

    private bool rightInput_DOWN = false;
    private bool rightInput_UP = false;
    private bool rightInput_STATE = false;

    private bool leftInput_DOWN = false;
    private bool leftInput_UP = false;
    private bool leftInput_STATE = false;

    private bool timeTravelInput_DOWN = false;

    private SO_RA_GoToFuture goToFuture;
    private SO_RA_GoToPast goToPast;
    private SO_BA_InFuture inFuture;

    private Rigidbody2D rb;

    private CapsuleCollider2D shape;

    private float horizDirection = 0;
    private bool jumping = false;
    private float jumpTimeCounter = 0f;
    private Vector2 vel = new Vector2(0, 0);

    public float GravMultiplyer { get; set; } = 1f;

    private Vector2 groundNorm = new Vector2(0, 0);
    private bool grounded = false;
    private bool onWall = false;

    private Vector2? movingPlatform = null;

    private int health = MAX_HEALTH;
    public int Health
    {
        get => health;
        set
        {
            health = value;
            HealthUpdated(value);
        }
    }

    public static event System.Action<int> HealthUpdated = delegate { };

    private ContactFilter2D cf;

    //initialized with max number of collisions 
    //able to store per frame
    private const int MAX_COLLISIONS = 16;

    private void Awake()
    {
        S_DeveloperTools_Delegates();
        S_InputManager_Delegates();
        S_TimeTravel_Delegates();
        S_TimeTravel_Awake();

        SetProperTextPosScale();
        AstroAnim.OnOrientationUpdate += AstroAnim_OnOrientationUpdate;
    }

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();
        shape = GetComponent<CapsuleCollider2D>();
        animController = GetComponent<AstroAnim>();
    }

    #region Astro Dev Tools Integration
    /// <summary>
    /// DO NOT TOUCH THIS
    /// </summary>
    private void S_DeveloperTools_Delegates()
    {
        S_DeveloperTools_EnableChanged();

        S_DeveloperTools.Current.EnableDevToolsChanged -= S_DeveloperTools_EnableChanged;
        S_DeveloperTools.Current.EnableDevToolsChanged += S_DeveloperTools_EnableChanged;
        S_DeveloperTools.Current.AstroPlayerDevToolsChanged -= S_DeveloperTools_EnableChanged;
        S_DeveloperTools.Current.AstroPlayerDevToolsChanged += S_DeveloperTools_EnableChanged;
        S_DeveloperTools.Current.TimeTravelDevToolsChanged -= S_DeveloperTools_TimeTravelDevToolsChanged;
        S_DeveloperTools.Current.TimeTravelDevToolsChanged += S_DeveloperTools_TimeTravelDevToolsChanged;
    }

    /// <summary>
    /// CHANGE SHIT HERE FOR ADDING NEW DEV TOOLS
    /// </summary>
    private void S_DeveloperTools_EnableChanged()
    {
        S_DeveloperTools_AstroPlayerDevToolsChanged();
        S_DeveloperTools_TimeTravelDevToolsChanged();
    }

    private void S_DeveloperTools_AstroPlayerDevToolsChanged()
    {
        S_DeveloperTools.Current.CurrHealthChanged -= S_DeveloperTools_CurrHealthChanged;
        S_DeveloperTools.Current.EnableUnlimtedJumpChanged -= S_DeveloperTools_EnableUnlimtedJumpChanged;
        S_DeveloperTools.Current.KillAstro -= S_DeveloperTools_KillAstro;
        S_DeveloperTools.Current.GravityMultiplyerChanged += S_DeveloperTools_GravityMultiplyerChanged;
        S_DeveloperTools.Current.ShowPrintAstroVelLinesChanged -= S_DeveloperTools_ShowPrintAstroVelLinesChanged;
        S_DeveloperTools.Current.PrintRawPlayerInputsChanged -= S_DeveloperTools_PrintRawPlayerInputsChanged;

        if (S_DeveloperTools.Current.DevToolsEnabled_ASTRO_PLAYER())
        {
            S_DeveloperTools.Current.CurrHealthChanged += S_DeveloperTools_CurrHealthChanged;
            S_DeveloperTools.Current.EnableUnlimtedJumpChanged += S_DeveloperTools_EnableUnlimtedJumpChanged;
            S_DeveloperTools.Current.KillAstro += S_DeveloperTools_KillAstro;
            S_DeveloperTools.Current.GravityMultiplyerChanged += S_DeveloperTools_GravityMultiplyerChanged;
            S_DeveloperTools.Current.ShowPrintAstroVelLinesChanged += S_DeveloperTools_ShowPrintAstroVelLinesChanged;
            S_DeveloperTools.Current.PrintRawPlayerInputsChanged += S_DeveloperTools_PrintRawPlayerInputsChanged;
        }

        S_DevloperTools_AstroDevToolsManualUpdate();
    }

    private void S_DeveloperTools_TimeTravelDevToolsChanged()
    {
        S_DeveloperTools.Current.SetTTSpawnsAtCurrPosChanged -= S_DeveloperTools_SetTTSpawnsAtCurrPosChanged;

        if (S_DeveloperTools.Current.DevToolsEnabled_TIME_TRAVEL())
        {
            S_DeveloperTools.Current.SetTTSpawnsAtCurrPosChanged += S_DeveloperTools_SetTTSpawnsAtCurrPosChanged;
        }

        S_DeveloperTools_SetTTSpawnsAtCurrPosChanged();
    }

    private void S_DevloperTools_AstroDevToolsManualUpdate()
    {
        S_DeveloperTools_CurrHealthChanged();
        S_DeveloperTools_EnableUnlimtedJumpChanged();
        //on purpose left out kill astro changed here
        S_DeveloperTools_GravityMultiplyerChanged();
        S_DeveloperTools_ShowPrintAstroVelLinesChanged();
        S_DeveloperTools_PrintRawPlayerInputsChanged();
    }

    private void S_DeveloperTools_CurrHealthChanged()
    {
        Health = S_DeveloperTools.Current.CurrHealth;
    }

    private bool DEVTOOLS_unlimitedJump = false;
    private void S_DeveloperTools_EnableUnlimtedJumpChanged()
    {
        DEVTOOLS_unlimitedJump = S_DeveloperTools.Current.DevToolsEnabled_ASTRO_PLAYER() && S_DeveloperTools.Current.EnableUnlimtedJump;
    }

    private void S_DeveloperTools_KillAstro()
    {
        Health = 0;
    }

    private float DEVTOOLS_gravityMultiplyer = 1f;
    private void S_DeveloperTools_GravityMultiplyerChanged()
    {
        if (!S_DeveloperTools.Current.DevToolsEnabled_ASTRO_PLAYER())
        {
            DEVTOOLS_gravityMultiplyer = 1f;
            return;
        }

        DEVTOOLS_gravityMultiplyer = S_DeveloperTools.Current.GravityMultiplyer;
    }

    private bool DEVTOOLS_showPrintAstroVelLines = false;
    private void S_DeveloperTools_ShowPrintAstroVelLinesChanged()
    {
        if (!S_DeveloperTools.Current.DevToolsEnabled_ASTRO_PLAYER())
        {
            DEVTOOLS_showPrintAstroVelLines = false;
            return;
        }

        DEVTOOLS_showPrintAstroVelLines = S_DeveloperTools.Current.ShowPrintAstroVelLines;
    }

    private bool DEVTOOLS_printRawPlayerInputs = false;
    private void S_DeveloperTools_PrintRawPlayerInputsChanged()
    {
        if (!S_DeveloperTools.Current.DevToolsEnabled_ASTRO_PLAYER())
        {
            DEVTOOLS_printRawPlayerInputs = false;
            return;
        }

        DEVTOOLS_printRawPlayerInputs = S_DeveloperTools.Current.PrintRawPlayerInputs;
    }

    private bool DEVTOOLS_setTTSpawnsAtCurrPos = false;
    private void S_DeveloperTools_SetTTSpawnsAtCurrPosChanged()
    {
        if (!S_DeveloperTools.Current.DevToolsEnabled_TIME_TRAVEL())
        {
            DEVTOOLS_setTTSpawnsAtCurrPos = false;
            return;
        }

        DEVTOOLS_setTTSpawnsAtCurrPos = S_DeveloperTools.Current.SetTTSpawnsAtCurrPos;
    }
    #endregion


    #region INPUT_MANAGER
    private void S_InputManager_Delegates()
    {
        S_InputManager.Current.ControlsEnabledChanged -= S_InputManager_ControlsEnabeldChanged;
        S_InputManager.Current.ControlsEnabledChanged += S_InputManager_ControlsEnabeldChanged;
    }

    private bool INPUT_controlsEnabled = true;
    private void S_InputManager_ControlsEnabeldChanged()
    {
        INPUT_controlsEnabled = S_InputManager.Current.ControlsEnabled;
        if (!INPUT_controlsEnabled)
        {
            horizDirection = 0;
            jumping = false;
        }
    }

    public static event Action<bool> OnInteractInput = delegate { };

    public void OnMoveInputUpdate(InputAction.CallbackContext val)
    {
        if (!INPUT_controlsEnabled)
        {
            return;
        }
        horizDirection = val.ReadValue<Vector2>().x;
        Debug.LogFormat("new horz x direction: {0}", horizDirection);
    }

    public void OnJumpInputUpdate(InputAction.CallbackContext val)
    {
        if (!INPUT_controlsEnabled)
        {
            return;
        }
        jumping = val.performed && (grounded || DEVTOOLS_unlimitedJump);
    }

    public void OnTimeTravelInputUpdate(InputAction.CallbackContext val)
    {
        if (!INPUT_controlsEnabled || !val.performed)
        {
            return;
        }

        if (TIME_TRAVEL_enabled)
        {
            if (inFuture.IsTrue())
            {
                goToPast.Execute();
            }
            else
            {
                goToFuture.Execute();
            }
        }
        else
        {
            Debug.Log("Player tried to time travel but time traveling not enabled.");
        }
    }

    public void OnInteractInputUpdate(InputAction.CallbackContext val)
    {
        if (!INPUT_controlsEnabled)
        {
            return;
        }
        OnInteractInput(val.performed);
    }

    #endregion

    #region TIME_TRAVEL

    private void S_TimeTravel_Delegates()
    {
        S_TimeTravel.Current.PlayerTimeTravelEnabledChanged -= S_TimeTravel_PlayerTimeTravelEnabled;
        S_TimeTravel.Current.PlayerTimeTravelEnabledChanged += S_TimeTravel_PlayerTimeTravelEnabled;
        S_TimeTravel_PlayerTimeTravelEnabled();

        S_TimeTravel.Current.ComposeAstroTTD -= S_TimeTravel_ComposeAstroTTD;
        S_TimeTravel.Current.ComposeAstroTTD += S_TimeTravel_ComposeAstroTTD;

        S_TimeTravel.Current.ParseAstroTTD -= S_TimeTravel_ParseAstroTTD;
        S_TimeTravel.Current.ParseAstroTTD += S_TimeTravel_ParseAstroTTD;
    }

    private void S_TimeTravel_Awake()
    {
        goToFuture = new SO_RA_GoToFuture();
        goToPast = new SO_RA_GoToPast();
        inFuture = new SO_BA_InFuture();

        if (pastStartPos == null || futureStartPos == null)
        {
            Debug.LogError("Astro doesn't have a start position for the past or future!");
        }
        else
        {
            pastTTD = DEVTOOLS_setTTSpawnsAtCurrPos ? new AstroTimeTravelData(transform) : new AstroTimeTravelData(pastStartPos);
            futureTTD = DEVTOOLS_setTTSpawnsAtCurrPos ? new AstroTimeTravelData(transform) : new AstroTimeTravelData(futureStartPos);
            astroCollider = GetComponent<CapsuleCollider2D>();
            //shortcut to setting astro at start position
            S_TimeTravel_ParseAstroTTD();
        }
    }

    private bool TIME_TRAVEL_enabled = false;
    private void S_TimeTravel_PlayerTimeTravelEnabled()
    {
        TIME_TRAVEL_enabled = S_TimeTravel.Current.PlayerTimeTravelEnabled;
    }


    private class AstroTimeTravelData
    {
        public AstroTimeTravelData(Transform startTransform)
        {
            startPos = new Vector2(startTransform.position.x, startTransform.position.y);
        }
        private Vector2 startPos;
        public GameObject groundObject;
        //if we switch and haven't had a chance to cache it before, use designated start pos
        public Vector2 GroundObjectPos => groundObject == null? startPos : new Vector2(groundObject.transform.position.x, groundObject.transform.position.y);
        public Vector2 astroLocalGroundObjectPos = Vector2.zero;
        public int health;
    }

    //set in awake to new instances
    //TODO: change to live in singleton for when we get to multiple levels
    private AstroTimeTravelData pastTTD;
    private AstroTimeTravelData futureTTD;

    [SerializeField]
    private Transform pastStartPos;
    [SerializeField]
    private Transform futureStartPos;

    [SerializeField]
    private Transform textPosRight;
    [SerializeField]
    private Transform textPosLeft;

    public Transform GetTextTransformSide(bool right)
    {
        return right ? textPosRight : textPosLeft;
    }

    public Transform GetTextTransformDirection(bool front)
    {
        if (front)
        {
            return animController.FacingRight ? textPosRight : textPosLeft;
        }

        return animController.FacingRight ? textPosLeft : textPosRight;
    }

    private void SetProperTextPosScale()
    {
        //restore the OG scale of the text
        Vector3 newScale = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z);
        textPosLeft.localScale = newScale;
        textPosRight.localScale = newScale;
    }

    private void AstroAnim_OnOrientationUpdate(float multiplyer)
    {
        //to keep same direction always
        textPosLeft.localScale = new Vector3(Math.Abs(textPosLeft.localScale.x) * multiplyer, textPosLeft.localScale.y, textPosLeft.localScale.z);
        textPosLeft.localPosition = new Vector3(Math.Abs(textPosLeft.localPosition.x) * -multiplyer, textPosLeft.localPosition.y, textPosLeft.localPosition.z);

        textPosRight.localScale = new Vector3(Math.Abs(textPosRight.localScale.x) * multiplyer, textPosRight.localScale.y, textPosRight.localScale.z);
        textPosRight.localPosition = new Vector3(Math.Abs(textPosRight.localPosition.x) * multiplyer, textPosRight.localPosition.y, textPosRight.localPosition.z);

    }

    private CapsuleCollider2D astroCollider;

    private void S_TimeTravel_ComposeAstroTTD()
    {
        AstroTimeTravelData ttd = S_TimeTravel.Current.InFuture() ? pastTTD : futureTTD;
        RaycastHit2D closestGroundSpot = GetClosestGroundCollision(rb, cf);
        ttd.groundObject = closestGroundSpot.collider.gameObject;
        //TODO: add a method to the universal utilities singleton so that we don't have to manually make up for scale when adjusting position / getting the size of the capsule
        ttd.astroLocalGroundObjectPos = closestGroundSpot.point - ttd.GroundObjectPos + new Vector2(0, astroCollider.size.y * transform.localScale.y / 2f);
        ttd.health = health;
    }

    private void S_TimeTravel_ParseAstroTTD()
    {

        AstroTimeTravelData ttd = S_TimeTravel.Current.InFuture() ? futureTTD : pastTTD;
        
        //TODO: set only in FixedUpdate? Is it possible for timetravel to happen on a non FixedUpdate frame?
        Vector2 newAstroPos = ttd.astroLocalGroundObjectPos + ttd.GroundObjectPos;
        transform.position = new Vector3(newAstroPos.x, newAstroPos.y, transform.position.z);
        //Specifically want to set the private health here and not the public field Health because technically health has not changed
        health = ttd.health;
    }

    public static RaycastHit2D GetClosestGroundCollision(Rigidbody2D rb, ContactFilter2D cf)
    {
        RaycastHit2D[] results = new RaycastHit2D[MAX_COLLISIONS];

        //TODO: calculate trajectory parabola to get more accurate final location when in air? lol
        int contactCount = rb.Cast(Vector2.down, cf, results);
        if (contactCount == 0)
        {
            Debug.LogError("FUCK, there's no ground ever at all when we tried time traveling for Astro, does that mean we died from falling?");
            return new RaycastHit2D();
        }

        //TODO: can we assume the clossest is always the first?
        RaycastHit2D closestGroundColl = results[0];
        for (int i = 1; i < contactCount; i++)
        {
            if (closestGroundColl.distance > results[i].distance)
            {
                closestGroundColl = results[i];
            }
        }
        return closestGroundColl;
    }

    #endregion

    
    private void Start()
    {
        InitContactFilterSettings(cf, gameObject);
    }

    public static void InitContactFilterSettings(ContactFilter2D cf, GameObject gameObject)
    {
        //TODO: add to future utilities singleton
        //TODO:impelemnt this shit in the project settings

        cf.useTriggers = false; //ignore trigger colliders (b/c they gonna act as area 2ds)
        //set the layer mask to the one this game object has
        cf.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        cf.useLayerMask = true; //filter via layer mask in project settings
    }

    private void FixedUpdate()
    {
        //NOTE! : Time.deltaTime == Time.fixedDeltaTime when in FixedUpdate
        //(I just tested this)

        if (grounded) { jumpTimeCounter = 0f; }

        //get new x and y velocity\
        float xVel = vel.x;

        float accel = grounded ? ACCEL : AIR_ACCEL;
        float dirSign = horizDirection * vel.x;
        float speed = onWall ? MAX_SPEED / 4 : MAX_SPEED;
        if (dirSign <= 0 || (dirSign > 0 && MAX_SPEED > vel.x))
        {
            xVel = Mathf.Lerp(vel.x, horizDirection * speed, Time.deltaTime * accel);//ACCEL);//vel.x + accel * Time.fixedDeltaTime;
        }


        //makes sense to multiply by deltaTime here and later
        //because grav is acceleration
        float yVel = vel.y - (GRAVITY * GravMultiplyer * DEVTOOLS_gravityMultiplyer * Time.fixedDeltaTime);



        //clamp limits

        float maxX = Mathf.Max(Mathf.Abs(vel.x), MAX_SPEED);
        xVel = Mathf.Clamp(xVel, -maxX, maxX);
        yVel = Mathf.Clamp(yVel, -TERMINAL_VEL, TERMINAL_VEL);

        if (jumping)
        {
            if ((jumpTimeCounter <= MAX_JUMP_TIME) || DEVTOOLS_unlimitedJump)
            {
                jumpTimeCounter += Time.fixedDeltaTime;
                yVel = JUMP_VERT_SPEED;// * Time.fixedDeltaTime * 4;

            }
        }

        vel = new Vector2(xVel, yVel);

        vel = MoveRB3Update(vel, Time.fixedDeltaTime, Vector2.down, (jumpTimeCounter == 0));
        
        animController.AnimLogicFixedUpdate(grounded, jumping, vel.y < 0, (int)horizDirection);
    }


    private Vector2 MoveRB3Update(Vector2 velocity, float frameTime, Vector2 gravityDir, bool snap, bool stopOnSlope = true, float maxFloorAng = MIN_GROUND_DEG_ANG, float slideThreshold = SLIDE_FRIC_THRESHOLD)
    {
        gravityDir.Normalize();

        //the amount to move during this frame 
        Vector2 frameDeltaPos = velocity * frameTime;

        //where collision data will be stored after rb.Cast
        //maxes out at max collision
        RaycastHit2D[] hitBufferGrav = new RaycastHit2D[MAX_COLLISIONS];
        RaycastHit2D[] hitBuffer = new RaycastHit2D[MAX_COLLISIONS];

        int groundCheck = grounded && snap ? rb.Cast(gravityDir, cf, hitBufferGrav) : rb.Cast(gravityDir, cf, hitBufferGrav, GROUNDED_MIN_DIST * 10);

        int collCount = rb.Cast(frameDeltaPos, cf, hitBuffer, frameDeltaPos.magnitude);

        //for debugging and drawing lines
        Vector2 bottomOfShape = rb.position - new Vector2(0, shape.size.y / 2f * transform.localScale.y);
        if (DEVTOOLS_showPrintAstroVelLines) { Debug.DrawLine(bottomOfShape, bottomOfShape + frameDeltaPos / Time.deltaTime, Color.white); }


        bool prevGroundState = grounded;
        Vector2? prevMovingPlatform = movingPlatform == null ? null : movingPlatform;

        grounded = false;
        movingPlatform = null;
        onWall = false;
        bool useGroundForColl = false;
        RaycastHit2D closestGroundColl = new RaycastHit2D();
        float groundDegIncline = 0f;


        if (groundCheck > 0)
        {
            closestGroundColl = hitBufferGrav[0];
            for (int i = 1; i < collCount; i++)
            {
                closestGroundColl = closestGroundColl.distance < hitBufferGrav[i].distance ? closestGroundColl : hitBufferGrav[i];
            }

            groundDegIncline = 180 - Vector2.Angle(closestGroundColl.normal, gravityDir);


            grounded = closestGroundColl.distance <= GROUNDED_MIN_DIST && groundDegIncline <= MIN_GROUND_DEG_ANG;

            if (grounded)
            {
                if (closestGroundColl.collider.CompareTag(MOVING_PLATFORM_TAG))
                {
                    movingPlatform = (Vector2?) new Vector2 (closestGroundColl.collider.transform.position.x, closestGroundColl.collider.transform.position.y);
                }
            }

            onWall = closestGroundColl.distance <= GROUNDED_MIN_DIST && groundDegIncline > MIN_GROUND_DEG_ANG;
            useGroundForColl = closestGroundColl.distance <= GROUNDED_MIN_COLL_DIST;

        }




        if (!prevGroundState && grounded) { enteredGround(); }
        else if (prevGroundState && !grounded) { exitedGround(); }

        //The angles of the actual frameDeltaPos vector may be too short
        //or parallel to surface to render any hit detection with ground
        //so in any case this fails, use the floor collisions to calculate shit
        if (collCount == 0 && useGroundForColl)
        {
            collCount = groundCheck;
            hitBuffer = hitBufferGrav;
        }

        RaycastHit2D closestColl = new RaycastHit2D();

        //if there were collisions with surface
        if (collCount > 0)
        {
            //get the closest collision
            closestColl = hitBuffer[0];
            for (int i = 1; i < collCount; i++)
            {
                closestColl = closestColl.distance < hitBuffer[i].distance ? closestColl : hitBuffer[i];
            }

            //Its makes more sense if you draw this out:
            //basically re calculates the proper velocity if running into a slope/
            //As the velocity becomes more perpendicular with the surface, the
            //new velocity gets closer to zero (such as when standing still)
            float projMag = Vector2.Dot(frameDeltaPos, closestColl.normal);
            Vector2 subtractingVect = projMag * closestColl.normal.normalized;

            //debugging
            if (DEVTOOLS_showPrintAstroVelLines) { Debug.DrawLine(bottomOfShape, bottomOfShape + subtractingVect / Time.deltaTime, Color.yellow); }

            //If normal is greater than 90 deg from vel, dot product will be negative
            if (projMag < 0)
            {
                //This compensates setting the y velocity
                //(or grav direction vel) to zero when grounded
                frameDeltaPos -= subtractingVect;
            }
        }



        //Logic for sticking to the surface and not flying off slope
        //also for not sliding on slope
        Vector2 snapVect = Vector2.zero;
        Vector2 fricVect = Vector2.zero;

        if (groundCheck > 0)
        {

            if (groundDegIncline <= MIN_GROUND_DEG_ANG)
            {
                //TODO: what about distance from horiz. movement? Need to cast after horiz movement.
                if (snap) { snapVect = gravityDir * closestGroundColl.distance; }

                var ST = Mathf.Lerp(0, slideThreshold, groundDegIncline / MIN_GROUND_DEG_ANG);
                if (useGroundForColl && stopOnSlope && frameDeltaPos.magnitude < ST) { fricVect = frameDeltaPos * -1; }

            }
        }



        if (DEVTOOLS_showPrintAstroVelLines)
        {
            Debug.Log("fdp: " + frameDeltaPos.ToString("F10"));
            Debug.Log("snapV: " + snapVect.ToString("F10"));
            Debug.Log("fricVect: " + fricVect.ToString("F10"));
            Debug.DrawLine(bottomOfShape, bottomOfShape + frameDeltaPos / Time.deltaTime, Color.green);
        }

        Vector2 movingPlatformVect = Vector2.zero;
        //move the character to the new position
        if (prevMovingPlatform != null && movingPlatform != null)
        {
            movingPlatformVect = movingPlatform.Value - prevMovingPlatform.Value ;
        }

        //TODO: make this a dev feature to print velocity
        //Debug.Log(movingPlatformVect);
        rb.MovePosition(rb.position + frameDeltaPos + snapVect + fricVect + movingPlatformVect);


        //return the new real velocity (not including snap velocity)
        return frameDeltaPos / Time.deltaTime;
    }
    private void enteredGround()
    {
        jumpTimeCounter = 0f;
    }

    private void exitedGround(){}
    
    private void OnDestroy()
    {
        HealthUpdated = delegate{};
    }
}
