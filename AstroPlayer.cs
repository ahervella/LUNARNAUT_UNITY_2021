﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    private const float MAX_SPEED = 160f / 2f;
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

    private float horizDirectoin = 0;
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

        S_DeveloperTools.Current.EnableDevToolsChanged -= S_DeveloperTools_EnableChanged;
        S_DeveloperTools.Current.EnableDevToolsChanged += S_DeveloperTools_EnableChanged;
        S_DeveloperTools.Current.AstroPlayerDevToolsChanged -= S_DeveloperTools_EnableChanged;
        S_DeveloperTools.Current.AstroPlayerDevToolsChanged += S_DeveloperTools_EnableChanged;

        S_AstroInputManager.Current.ControlsEnabledChanged -= S_AstroInputManager_ControlsEnabeldChanged;
        S_AstroInputManager.Current.ControlsEnabledChanged += S_AstroInputManager_ControlsEnabeldChanged;
    }

    /// <summary>
    /// CHANGE SHIT HERE FOR ADDING NEW DEV TOOLS
    /// </summary>
    private void S_DeveloperTools_EnableChanged()
    {
        S_DeveloperTools.Current.CurrHealthChanged -= S_DeveloperTools_CurrHealthChanged;
        S_DeveloperTools.Current.EnableUnlimtedJumpChanged -= S_DeveloperTools_EnableUnlimtedJumpChanged;
        S_DeveloperTools.Current.KillAstro -= S_DeveloperTools_KillAstro;
        S_DeveloperTools.Current.ShowPrintAstroVelLinesChanged -= S_DeveloperTools_ShowPrintAstroVelLinesChanged;
        S_DeveloperTools.Current.PrintRawPlayerInputsChanged -= S_DeveloperTools_PrintRawPlayerInputsChanged;

        if (S_DeveloperTools.Current.EnableDevTools && S_DeveloperTools.Current.AstroPlayerDevTools)
        {
            S_DeveloperTools.Current.CurrHealthChanged += S_DeveloperTools_CurrHealthChanged;
            S_DeveloperTools.Current.EnableUnlimtedJumpChanged += S_DeveloperTools_EnableUnlimtedJumpChanged;
            S_DeveloperTools.Current.KillAstro += S_DeveloperTools_KillAstro;
            S_DeveloperTools.Current.GravityMultiplyerChanged += S_DeveloperTools_GravityMultiplyerChanged;
            S_DeveloperTools.Current.ShowPrintAstroVelLinesChanged += S_DeveloperTools_ShowPrintAstroVelLinesChanged;
            S_DeveloperTools.Current.PrintRawPlayerInputsChanged += S_DeveloperTools_PrintRawPlayerInputsChanged;
        }

        S_DevloperTools_ManualUpdate();
    }

    private void S_DevloperTools_ManualUpdate()
    {
        S_DeveloperTools_CurrHealthChanged();
        S_DeveloperTools_EnableUnlimtedJumpChanged();
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
    #endregion


    #region INPUT_MANAGER

    private bool INPUT_controlsEnabled = true;
    private void S_AstroInputManager_ControlsEnabeldChanged()
    {
        INPUT_controlsEnabled = S_AstroInputManager.Current.ControlsEnabled;

        lock (_inputLock)
        {
            jumping = false;
            leftInput_STATE = false;
            rightInput_STATE = false;
        }
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
        
        animController.AnimLogicFixedUpdate(grounded, jumping, vel.y < 0, (int)horizDirectoin);
    }

    #region Input Management
    private void InputUpdate()
    {
        if (!INPUT_controlsEnabled)
        {
            return;
        }

        lock (_inputLock)
        {
            KeyUpdate(JUMP_INPUT_KEY, readDown: true, ref jumpInput_DOWN);
            KeyUpdate(JUMP_INPUT_KEY, readDown: false, ref jumpInput_UP);

            KeyUpdate(RIGHT_INPUT_KEY, readDown: true, ref rightInput_DOWN);
            KeyUpdate(RIGHT_INPUT_KEY, readDown: false, ref rightInput_UP);

            KeyUpdate(LEFT_INPUT_KEY, readDown: true, ref leftInput_DOWN);
            KeyUpdate(LEFT_INPUT_KEY, readDown: false, ref leftInput_UP);

            KeyUpdate(TIME_TRAVEL_KEY, readDown: true, ref timeTravelInput_DOWN);

        }

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
                    if (DEVTOOLS_printRawPlayerInputs)
                    {
                        Debug.Log("got key down for: " + kc.ToString());
                    }
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
                    if (DEVTOOLS_printRawPlayerInputs)
                    {
                        Debug.Log("got key up for: " + kc.ToString());
                    }
                    return;
                }
            }
        }
    }
    private readonly object _inputLock = new object();
    private void InputFixedUpdate()
    {
        //do this here to make sure we don't skip one frame presses
        //reset the one frame inputs to false once processed in first fixed update frame

        //need to wrap in a lock so we don't have to use a lock
        //because update could change the value of jumpInput_DOWN to true
        //in between lines
        lock (_inputLock)
        {
            bool jumpInputDown = false;
            if (jumpInput_DOWN)
            {
                jumpInputDown = true;
                jumpInput_DOWN = false;
            }

            bool jumpInputUp = false;
            if (jumpInput_UP)
            {
                jumpInputUp = true;
                jumpInput_UP = false;
            }


            if (rightInput_DOWN)
            {
                rightInput_STATE = true;
                rightInput_DOWN = false;
            }
            if (rightInput_UP)
            {
                rightInput_STATE = false;
                rightInput_UP = false;
            }


            if (leftInput_DOWN)
            {
                leftInput_STATE = true;
                leftInput_DOWN = false;
            }
            if (leftInput_UP)
            {
                leftInput_STATE = false;
                leftInput_UP = false;
            }

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

            if (timeTravelInput_DOWN)
            {
                if (inFuture.IsTrue())
                {
                    goToPast.Execute();
                }
                else
                {
                    goToFuture.Execute();
                }

                timeTravelInput_DOWN = false;
            }
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

    private void exitedGround()
    {
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
