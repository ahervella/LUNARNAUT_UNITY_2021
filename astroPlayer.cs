using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LunarnautShit;
using System;

//Tutorial I used to implement this awesome shit:
//https://learn.unity.com/tutorial/live-session-2d-platformer-character-controller#5c7f8528edbc2a002053b68e


public class astroPlayer : MonoBehaviour
{
    //enum AstroAnim.ANIM { END1, END2, FALL, JUMP, LAND, RUN, STAND, START, DEATH}
    enum SUIT { GGG, GGR, GRR, RGG, RRG, RRR}

    SUIT suitCode = SUIT.GGG;
    AstroAnim.ANIM animCode = AstroAnim.ANIM.END1;//AstroAnim.ANIM.END1;
    AnimatorStateInfo currAnimInfo;
    AnimationClip currAnimClip;

    List<IEnumerator> changeAnimCoroutines = new List<IEnumerator>();

    public bool debugVectorLines = false;

    const float MIN_MOVE_DIST = 0.001f;
    //if this is min dist too small, game can break
    const float GROUNDED_MIN_DIST = 0.25f;
    const float GROUNDED_MIN_COLL_DIST = GROUNDED_MIN_DIST / 2;
    const float MIN_GROUND_DEG_ANG = 40f;
    const float SLIDE_FRIC_THRESHOLD = 0.6f;


    const float ACCEL = 3f;
    const float AIR_ACCEL = ACCEL / 2f;
    const float MAX_SPEED = 160f / 2f;
    const float MAX_AIR_SPEED = MAX_SPEED;
    const float GRAVITY = 180f / 2f;
    const float TERMINAL_VEL = 200f / 2f;

    const float JUMP_VERT_SPEED = 150f/2f;
    const float MAX_JUMP_TIME = 0.6f;

    Rigidbody2D rb;
    Animator anim;
    
    CapsuleCollider2D shape;

    public LunInput input;

    Vector2 direction = new Vector2(0, 0);
    bool jumping = false;
    bool canJump = false;
    float jumpTimeCounter = 0f;
    Vector2 vel = new Vector2(0, 0);

    Vector2 groundNorm = new Vector2(0, 0);
    bool grounded = false;
    bool onWall = false;

    

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

    
    //Happens before onEnable, only once ever 
    void Awake()
    {
        input = new LunInput();
        input.Enable();


        //input.AstroPlayer.move. += val => processMoveInput(val.ReadValue<Vector2>());
        //input.AstroPlayer.jump.performed += nada => moveJump();
        
    }

    //Happens every time object is instanced, after awake, before start
    void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        shape = GetComponent<CapsuleCollider2D>();
    }

    //Happens after Awake and OnEnable, once all objects have been initialized really
    private void Start()
    {
        //TODO:impelemnt this shit in the project settings

        cf.useTriggers = false; //ignore trigger colliders (b/c they gonna act as area 2ds)
        //set the layer mask to the one this game object has
        cf.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        cf.useLayerMask = true; //filter via layer mask in project settings

        setAnim(AstroAnim.ANIM.RUN);
        setSuitCode(SUIT.RRR);
    }

    void FixedUpdate()
    {
        //NOTE! : Time.deltaTime == Time.fixedDeltaTime when in FixedUpdate
        //(I just tested this) 

        direction = input.AstroPlayer.move.ReadValue<Vector2>();

        if (jumping && grounded)
        {
            jumping = false;
        }

        if (input.AstroPlayer.jump.ReadValue<float>() > 0)
        {
            jumping = true;
        }
        else
        {
            canJump = grounded;
        }

        if (grounded) { jumpTimeCounter = 0f; }


        //get new x and y velocity\
        float xVel = vel.x;

        float accel = grounded ? ACCEL : AIR_ACCEL;
        float dirSign = direction.x * vel.x;
        float speed = onWall ? MAX_SPEED / 4 : MAX_SPEED;
        if (dirSign <= 0 || (dirSign > 0 && MAX_SPEED > vel.x))
        {
            xVel = Mathf.Lerp(vel.x, direction.x * speed, Time.deltaTime * accel);//ACCEL);//vel.x + accel * Time.fixedDeltaTime;
        }
        

        //makes sense to multiply by deltaTime here and later
        //because grav is acceleration
        float yVel = vel.y - (GRAVITY * Time.fixedDeltaTime);

        

        //clamp limits

        float maxX = Mathf.Max(Mathf.Abs(vel.x), MAX_SPEED);
        xVel = Mathf.Clamp(xVel, -maxX, maxX);
        yVel = Mathf.Clamp(yVel, -TERMINAL_VEL, TERMINAL_VEL);

        if (jumping && canJump)
        {
            if (jumpTimeCounter <= MAX_JUMP_TIME)
            {
                jumpTimeCounter += Time.fixedDeltaTime;
                yVel = JUMP_VERT_SPEED;// * Time.fixedDeltaTime * 4;

            }
        }

        vel = new Vector2(xVel, yVel);

        vel = moveRB3(vel, Time.fixedDeltaTime, Vector2.down, (jumpTimeCounter == 0));

        animLogic();
        //animChange();
        //getFrame();
        //animChange();
    }




    Vector2 moveRB3(Vector2 velocity, float frameTime, Vector2 gravityDir, bool snap, bool stopOnSlope = true, float maxFloorAng = MIN_GROUND_DEG_ANG, float slideThreshold = SLIDE_FRIC_THRESHOLD)
    {
        gravityDir.Normalize();

        //the amount to move during this frame 
        Vector2 frameDeltaPos = velocity * frameTime;

        //where collision data will be stored after rb.Cast
        //maxes out at max collision
        RaycastHit2D[] hitBufferGrav = new RaycastHit2D[MAX_COLLISIONS];
        RaycastHit2D[] hitBuffer = new RaycastHit2D[MAX_COLLISIONS];

        int groundCheck = grounded && snap? rb.Cast(gravityDir, cf, hitBufferGrav) : rb.Cast(gravityDir, cf, hitBufferGrav, GROUNDED_MIN_DIST*10);
        
        int collCount = rb.Cast(frameDeltaPos, cf, hitBuffer, frameDeltaPos.magnitude);
       
        //for debugging and drawing lines
        Vector2 bottomOfShape = rb.position - new Vector2(0, shape.size.y / 2f * transform.localScale.y);
        if (debugVectorLines) { Debug.DrawLine(bottomOfShape, bottomOfShape + frameDeltaPos / Time.deltaTime, Color.white); }


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
            if (debugVectorLines) { Debug.DrawLine(bottomOfShape, bottomOfShape + subtractingVect / Time.deltaTime, Color.yellow); }

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


        
        if (debugVectorLines)
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

    void animLogic()
    {
        if (grounded && jumping && !isAnim(suitCode, AstroAnim.ANIM.JUMP))
        {
            setAnim(AstroAnim.ANIM.JUMP);
        }

        else if (grounded && (isAnim(suitCode, AstroAnim.ANIM.JUMP) || isAnim(suitCode, AstroAnim.ANIM.FALL)))
        {
            setAnim(AstroAnim.ANIM.LAND);
        }

        else if (grounded && !isAnim(suitCode, AstroAnim.ANIM.LAND))
        {
            if (direction.x == 0)
            {
                if (isAnim(suitCode, AstroAnim.ANIM.RUN))
                {
                    if (getFrame() >= 41) { setAnim(AstroAnim.ANIM.END1); }
                    if (getFrame() >= 20) { setAnim(AstroAnim.ANIM.END2); }
                    if (getFrame() >= 1) { setAnim(AstroAnim.ANIM.END1); }
                }

                if (isAnim(suitCode, AstroAnim.ANIM.START)) { setAnim(AstroAnim.ANIM.END1); }
            }
        }
    }

    void enteredGround()
    {
        jumpTimeCounter = 0f;
        //Plays the Wwise audio event with the corresponding string name (arg. 1) on the object (arg. 2).
        //See my documentation for audio names (This is in progress)
        AkSoundEngine.PostEvent(playerSounds.landEventName, gameObject);
    }

    void exitedGround()
    {
        AkSoundEngine.PostEvent(playerSounds.jumpEventName, gameObject);
    }

    void setSuitCode(SUIT code)
    {
        suitCode = code;
        changeAnimCoroutines.Add(animChange(true));// = animChange(true);
        StartCoroutine(changeAnimCoroutines[changeAnimCoroutines.Count - 1]);
    }

    void setAnim(AstroAnim.ANIM anim)
    {
        animCode = anim;
        changeAnimCoroutines.Add(animChange(false));
        StartCoroutine(changeAnimCoroutines[changeAnimCoroutines.Count - 1]);
    }

    float getFrame()
    {
        //return 0f;
        float duration = currAnimClip.length;
        float frameRate = currAnimClip.frameRate;
        float currTime = currAnimInfo.normalizedTime;

        Debug.Log("Duration: " + duration.ToString());
        Debug.Log("FrameRate: " + frameRate.ToString());
        Debug.Log("CurrTime: " + currTime.ToString());
        Debug.Log("Name: " + currAnimClip.name);

        return (currTime * duration * frameRate);

    }

    bool isAnim(SUIT suit, AstroAnim.ANIM anim)
    {
        string blah = currAnimClip.name;
        string blah2 = string.Format("Astro.ASTRO_{0}_{1}", suit, anim);
        //test this shiT^^^^^
        return currAnimInfo.IsName(string.Format("Astro.ASTRO_{0}_{1}", suit, anim));
    }

    IEnumerator animChange(bool matchFrame = false)
    {
        string animName = string.Format("ASTRO_{0}_{1}", suitCode.ToString(), animCode.ToString());

        float currTime = matchFrame? currAnimInfo.normalizedTime : 0;
        anim.Play(animName, 0, currTime);
        
        AnimatorClipInfo[] blah = anim.GetCurrentAnimatorClipInfo(0);
        Debug.Log("animatorClipInfo size: " + blah.Length);
        //blah[0].clip

        
        yield return new WaitForFixedUpdate();
        //StopCoroutine("animChange");
        currAnimInfo = anim.GetCurrentAnimatorStateInfo(0);
        currAnimClip = anim.GetCurrentAnimatorClipInfo(0)[0].clip;

        foreach (IEnumerator cr in changeAnimCoroutines) { StopCoroutine(cr); }
        changeAnimCoroutines.Clear();



    }



    void onAnimFinished(AstroAnim.ANIM anim)
    {

    }

    void OnCollisionEnter2D(Collision2D col)
    {
        //Debug.Log("touching!");
        //Debug.Log(col);
    }

    void processHazards()
    {

    }

    void processMoveInput(Vector2 dir)
    {
        direction = dir;
       //if (input.AstroPlayer.move.)
    }

    void processInteractInput()
    {

    }

    void processFlashLightInput()
    {

    }

    void processMenuInput()
    {

    }

    void move()
    {

    }

    void moveJump()
    {

    }

    void moveCameraAndInteract()
    {

    }

    void moveMovableObjects()
    {

    }

    void retrictFromRope()
    {

    }


}
