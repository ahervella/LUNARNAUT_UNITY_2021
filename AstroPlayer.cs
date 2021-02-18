using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using LunarnautShit;

//Tutorial I used to implement this awesome shit:
//https://learn.unity.com/tutorial/live-session-2d-platformer-character-controller#5c7f8528edbc2a002053b68e


public class AstroPlayer : MonoBehaviour
{

    private AstroAnim animController;

    const float MIN_MOVE_DIST = 0.001f;
    //if this is min dist too small, game can break
    const float GROUNDED_MIN_DIST = 1f;//0.25f;
    const float GROUNDED_MIN_COLL_DIST = GROUNDED_MIN_DIST / 2;
    const float MIN_GROUND_DEG_ANG = 40f;
    const float SLIDE_FRIC_THRESHOLD = 0.6f;


    const float ACCEL = 3f;
    const float AIR_ACCEL = ACCEL / 2f;
    const float MAX_SPEED = 160f / 2f;
    //TODO: implement max air speed
    const float MAX_AIR_SPEED = MAX_SPEED;
    const float GRAVITY = 180f / 2f;
    const float TERMINAL_VEL = 200f / 2f;

    const float JUMP_VERT_SPEED = 150f / 2f;
    const float MAX_JUMP_TIME = 0.6f;

    public const int MAX_HEALTH = 4;

    //readonly allows for constants that need to be set at runtime
    private readonly KeyCode[] JUMP_INPUT_KEY = { KeyCode.UpArrow, KeyCode.W, KeyCode.Space };
    private readonly KeyCode[] RIGHT_INPUT_KEY = { KeyCode.RightArrow, KeyCode.D };
    private readonly KeyCode[] LEFT_INPUT_KEY = { KeyCode.LeftArrow, KeyCode.A };

    bool jumpInput_DOWN = false;
    bool jumpInput_UP = false;

    bool rightInput_DOWN = false;
    bool rightInput_UP = false;
    bool rightInput_STATE = false;

    bool leftInput_DOWN = false;
    bool leftInput_UP = false;
    bool leftInput_STATE = false;

    Rigidbody2D rb;

    CapsuleCollider2D shape;

    float horizDirectoin = 0;
    bool jumping = false;
    float jumpTimeCounter = 0f;
    Vector2 vel = new Vector2(0, 0);

    public float GravMultiplyer { get; set; } = 1f;

    Vector2 groundNorm = new Vector2(0, 0);
    bool grounded = false;
    bool onWall = false;

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

    ContactFilter2D cf;

    //initialized with max number of collisions 
    //able to store per frame
    const int MAX_COLLISIONS = 16;

    //Use this class to add Wwise Events to different player actions
    [Serializable]
    private class PlayerSounds
    {
        [SerializeField]
        public string jumpEventName;
        [SerializeField]
        public string landEventName;
    }

    [SerializeField]
    private PlayerSounds playerSounds;

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
    private void Awake()
    {
        S_DeveloperTools_EnableChanged();

        S_DevloperTools.Current.EnableDevToolsChanged -= S_DeveloperTools_EnableChanged;
        S_DevloperTools.Current.EnableDevToolsChanged += S_DeveloperTools_EnableChanged;
        S_DevloperTools.Current.AstroPlayerDevToolsChanged -= S_DeveloperTools_EnableChanged;
        S_DevloperTools.Current.AstroPlayerDevToolsChanged += S_DeveloperTools_EnableChanged;
    }

    /// <summary>
    /// CHANGE SHIT HERE FOR ADDING NEW DEV TOOLS
    /// </summary>
    private void S_DeveloperTools_EnableChanged()
    {
        S_DevloperTools.Current.CurrHealthChanged -= S_DeveloperTools_CurrHealthChanged;
        S_DevloperTools.Current.EnableUnlimtedJumpChanged -= S_DeveloperTools_EnableUnlimtedJumpChanged;
        S_DevloperTools.Current.KillAstro -= S_DeveloperTools_KillAstro;
        S_DevloperTools.Current.ShowPrintAstroVelLinesChanged -= S_DeveloperTools_ShowPrintAstroVelLinesChanged;

        if (S_DevloperTools.Current.EnableDevTools && S_DevloperTools.Current.AstroPlayerDevTools)
        {
            S_DevloperTools.Current.CurrHealthChanged += S_DeveloperTools_CurrHealthChanged;
            S_DevloperTools.Current.EnableUnlimtedJumpChanged += S_DeveloperTools_EnableUnlimtedJumpChanged;
            S_DevloperTools.Current.KillAstro += S_DeveloperTools_KillAstro;
            S_DevloperTools.Current.GravityMultiplyerChanged += S_DeveloperTools_GravityMultiplyerChanged;
            S_DevloperTools.Current.ShowPrintAstroVelLinesChanged += S_DeveloperTools_ShowPrintAstroVelLinesChanged;
        }

        S_DevloperTools_ManualUpdate();
    }

    private void S_DevloperTools_ManualUpdate()
    {
        S_DeveloperTools_CurrHealthChanged();
        S_DeveloperTools_EnableUnlimtedJumpChanged();
        S_DeveloperTools_GravityMultiplyerChanged();
    }

    private void S_DeveloperTools_CurrHealthChanged()
    {
        Health = S_DevloperTools.Current.CurrHealth;
    }

    private bool DEVTOOLS_unlimitedJump = false;
    private void S_DeveloperTools_EnableUnlimtedJumpChanged()
    {
        DEVTOOLS_unlimitedJump = S_DevloperTools.Current.DevToolsEnabled_ASTRO_PLAYER() && S_DevloperTools.Current.EnableUnlimtedJump;
    }

    private void S_DeveloperTools_KillAstro()
    {
        Health = 0;
    }

    private float DEVTOOLS_gravityMultiplyer = 1f;
    private void S_DeveloperTools_GravityMultiplyerChanged()
    {
        if (!S_DevloperTools.Current.DevToolsEnabled_ASTRO_PLAYER())
        {
            DEVTOOLS_gravityMultiplyer = 1f;
            return;
        }

        DEVTOOLS_gravityMultiplyer = S_DevloperTools.Current.GravityMultiplyer;
    }

    private bool DEVTOOLS_showPrintAstroVelLines = false;
    private void S_DeveloperTools_ShowPrintAstroVelLinesChanged()
    {
        if (!S_DevloperTools.Current.DevToolsEnabled_ASTRO_PLAYER())
        {
            DEVTOOLS_showPrintAstroVelLines = false;
            return;
        }

        DEVTOOLS_showPrintAstroVelLines = S_DevloperTools.Current.ShowPrintAstroVelLines;
    }
    #endregion

    private void Start()
    {
        //TODO:impelemnt this shit in the project settings

        cf.useTriggers = false; //ignore trigger colliders (b/c they gonna act as area 2ds)
        //set the layer mask to the one this game object has
        cf.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        cf.useLayerMask = true; //filter via layer mask in project settings

    }

    private void Update()
    {
        InputUpdate();
    }

    private void FixedUpdate()
    {
        //NOTE! : Time.deltaTime == Time.fixedDeltaTime when in FixedUpdate
        //(I just tested this)

        InputFixedUpdate();

        if (grounded) { jumpTimeCounter = 0f; }

        //get new x and y velocity\
        float xVel = vel.x;

        float accel = grounded ? ACCEL : AIR_ACCEL;
        float dirSign = horizDirectoin * vel.x;
        float speed = onWall ? MAX_SPEED / 4 : MAX_SPEED;
        if (dirSign <= 0 || (dirSign > 0 && MAX_SPEED > vel.x))
        {
            xVel = Mathf.Lerp(vel.x, horizDirectoin * speed, Time.deltaTime * accel);//ACCEL);//vel.x + accel * Time.fixedDeltaTime;
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
        
        animController.AnimLogicUpdate(grounded, jumping, vel.y < 0, (int)horizDirectoin);
    }

    #region Input Management
    private void InputUpdate()
    {
        KeyUpdate(JUMP_INPUT_KEY, true, ref jumpInput_DOWN);
        KeyUpdate(JUMP_INPUT_KEY, false, ref jumpInput_UP);

        KeyUpdate(RIGHT_INPUT_KEY, true, ref rightInput_DOWN);
        KeyUpdate(RIGHT_INPUT_KEY, false, ref rightInput_UP);

        KeyUpdate(LEFT_INPUT_KEY, true, ref leftInput_DOWN);
        KeyUpdate(LEFT_INPUT_KEY, false, ref leftInput_UP);
    }

    private void KeyUpdate(KeyCode[] possibleKeys, bool readDown, ref bool cachedBool)
    {
        if (readDown)
        {
            foreach (KeyCode kc in possibleKeys)
            {
                if (Input.GetKeyDown(kc))
                {
                    cachedBool = true;
                    return;
                }
            }
        }
        else
        {
            foreach (KeyCode kc in possibleKeys)
            {
                if (Input.GetKeyUp(kc))
                {
                    cachedBool = true;
                    return;
                }
            }
        }
    }

    private void InputFixedUpdate()
    {
        //do this here to make sure we don't skip one frame presses
        //reset the one frame inputs to false once processed in first fixed update frame

        bool jumpInputDown = jumpInput_DOWN;
        bool jumpInputUp = jumpInput_UP;
        jumpInput_DOWN = false;
        jumpInput_UP = false;

        
        if (rightInput_DOWN)
        {
            rightInput_STATE = true;
        }
        if (rightInput_UP)
        {
            rightInput_STATE = false;
        }
        rightInput_DOWN = false;
        rightInput_UP = false;


        if (leftInput_DOWN)
        {
            leftInput_STATE = true;
        }
        if (leftInput_UP)
        {
            leftInput_STATE = false;
        }
        leftInput_DOWN = false;
        leftInput_UP = false;

        //prevent from double jumping
        if (jumpInputDown)
        {
            jumping = grounded || DEVTOOLS_unlimitedJump;
        }
        if (jumpInputUp)
        {
            jumping = false;
        }

        horizDirectoin = 0;
        if (rightInput_STATE)
        {
            horizDirectoin++;
        }
        if (leftInput_STATE)
        {
            horizDirectoin--;
        }
    }
#endregion

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

        grounded = false;
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

        //move the character to the new position
        rb.MovePosition(rb.position + frameDeltaPos + snapVect + fricVect);


        //return the new real velocity (not including snap velocity)
        return frameDeltaPos / Time.deltaTime;
    }
    private void enteredGround()
    {
        jumpTimeCounter = 0f;
        //Plays the Wwise audio event with the corresponding string name (arg. 1) on the object (arg. 2).
        //See my documentation for audio names (This is in progress)
        AkSoundEngine.PostEvent(playerSounds.landEventName, gameObject);
    }

    private void exitedGround()
    {
        AkSoundEngine.PostEvent(playerSounds.jumpEventName, gameObject);
    }


    private void OnCollisionEnter2D(Collision2D col)
    {
        //Debug.Log("touching!");
        //Debug.Log(col);
    }

    private void processHazards()
    {

    }


    private void processInteractInput()
    {

    }

    private void processFlashLightInput()
    {

    }

    private void processMenuInput()
    {

    }

    private void move()
    {

    }

    private void moveJump()
    {

    }

    private void moveCameraAndInteract()
    {

    }

    private void moveMovableObjects()
    {

    }

    private void retrictFromRope()
    {

    }


}
