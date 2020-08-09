using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LunarnautShit;

//Tutorial I used to implement this awesome shit:
//https://learn.unity.com/tutorial/live-session-2d-platformer-character-controller#5c7f8528edbc2a002053b68e


public class astroPlayer : MonoBehaviour
{
    const float MIN_MOVE_DIST = 0.001f;
    //if this is min dist too small, game can break
    const float GROUNDED_MIN_DIST = 1f;
    const float GROUNDED_MIN_COLL_DIST = GROUNDED_MIN_DIST / 2;
    const float MIN_GROUND_DEG_ANG = 40f;
    //float MIN_GROUND_NORM = Mathf.Sin(Mathf.Deg2Rad * MIN_GROUND_DEG_ANG);
    const float SLIDE_FRIC_THRESHOLD = 0.6f;

    //TODO: increase that shit^^^

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

    public Vector2 direction = new Vector2(0, 0);
    public bool jumping = false;
    public bool canJump = false;
    public float jumpTimeCounter = 0f;
    public Vector2 vel = new Vector2(0, 0);
    public Vector2 velGrav = new Vector2(0, 0);
    public Vector2 velMove = new Vector2(0, 0);

    public Vector2 groundNorm = new Vector2(0, 0);
    public bool grounded = false;
    public bool onWall = false;

    //just for debugging
    public float hbd = 0f;
    public float projMagnitude = 0f;
    public int collisionCount = 0;
    public float slideVectorLength = 0f;
    public Vector2 slideVect = Vector2.zero;

    ContactFilter2D cf;

    //initialized with max number of collisions 
    //able to store per frame
    const int MAX_COLLISIONS = 16;
    //RaycastHit2D[] hitBuffer = new RaycastHit2D[MAX_COLLISIONS];
    //RaycastHit2D[] hitBufferGrav = new RaycastHit2D[MAX_COLLISIONS];
    //RaycastHit2D[] hitBufferMove = new RaycastHit2D[MAX_COLLISIONS];
    //List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(MAX_COLLISIONS);

    public Vector2 savedHorzMove = new Vector2(0, 0);

    
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
        Debug.DrawLine(bottomOfShape, bottomOfShape + frameDeltaPos / Time.deltaTime, Color.white);
        collisionCount = collCount;





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

            //will always return acuter angle


            //else { frameDeltaPos -= gravityDir * Vector2.Dot(frameDeltaPos, gravityDir) * 1.5f; }

            
        }




        
        //grounded = groundCheck > 0;


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
            Debug.DrawLine(bottomOfShape, bottomOfShape + subtractingVect / Time.deltaTime, Color.yellow);
            projMagnitude = projMag;

            //If normal is greater than 90 deg from vel, dot product will be negative
            if (projMag < 0)
            {
                //This compensates setting the y velocity
                //(or grav direction vel) to zero when grounded
                frameDeltaPos -= subtractingVect;
            }


        }

        //Vector2 innerFaceNorm = frameDeltaPos.x < 0? Vector2.Perpendicular(frameDeltaPos) : Vector2.Perpendicular(frameDeltaPos) * -1;
        //slideVect = innerFaceNorm;//Sie

        //slideVectorLength = (innerFaceNorm).magnitude;


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
            else if (useGroundForColl)
            {
                //Vector2 parallel2Floor = Vector2.Perpendicular(closestColl.normal);
                //parallel2Floor = Vector2.Angle(parallel2Floor, gravityDir) < 90? parallel2Floor : parallel2Floor * -1;

                //slideVect = gravityDir * frameDeltaPos.magnitude;
            }
        }




        //move the character to the new position
        Debug.Log("fdp: " + frameDeltaPos.ToString("F10"));
        Debug.Log("snapV: " + snapVect.ToString("F10"));
        Debug.Log("fricVect: " + fricVect.ToString("F10"));
        rb.MovePosition(rb.position + frameDeltaPos + snapVect + fricVect + slideVect);

        Debug.DrawLine(bottomOfShape, bottomOfShape + frameDeltaPos / Time.deltaTime, Color.green);

        //return the new real velocity (not including snap velocity)
        //TODO: include snap velocity when returning?
        return frameDeltaPos / Time.deltaTime;
    }

    void enteredGround()
    {
        jumpTimeCounter = 0f;
    }

    void exitedGround()
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
