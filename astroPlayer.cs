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
    const float GROUNDED_MIN_DIST = 0.05f;
    const float SHELL_RADIUS = GROUNDED_MIN_DIST * 10;
    const float MIN_GROUND_DEG_ANG = 60f;
    float MIN_GROUND_NORM = Mathf.Sin(Mathf.Deg2Rad * MIN_GROUND_DEG_ANG);

    const float SPEED = 10f;
    const float MAX_SPEED = 10f;
    const float GRAVITY = 3f;
    const float TERMINAL_VEL = 5f;

    Rigidbody2D rb;
    Animator anim;
    CapsuleCollider2D shape;

    public LunInput input;

    public Vector2 direction = new Vector2(0, 0);
    public bool jumping = false;
    public bool canJump = false;
    public Vector2 vel = new Vector2(0, 0);
    public Vector2 velGrav = new Vector2(0, 0);
    public Vector2 velMove = new Vector2(0, 0);

    public Vector2 groundNorm = new Vector2(0, 0);
    public bool grounded = false;

    //just for debugging
    public float hbd = 0f;
    public float projMagnitude = 0f;
    public int collisionCount = 0;

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
        else if (grounded)
        {
            canJump = true;
        }

        
        //get new x and y velocity
        float xVel = Mathf.Lerp(vel.x, direction.x * SPEED, Time.fixedDeltaTime);

        //makes sense to multiply by deltaTime here and later
        //because grav is acceleration
        float yVel = vel.y - (GRAVITY * Time.fixedDeltaTime);

        

        //clamp limits
        xVel = Mathf.Clamp(xVel, -MAX_SPEED, MAX_SPEED);
        yVel = Mathf.Clamp(yVel, -TERMINAL_VEL, TERMINAL_VEL);

        if (jumping && canJump && grounded)
        {
            yVel += 10f;
            canJump = false;
        }

        vel = new Vector2(xVel, yVel);

        

        //get delta position for this frame (b/c vel is dist/sec)
        Vector2 frameDeltaPos = vel * Time.fixedDeltaTime;

        //Debug.Log("Init vel: " + vel);
        //Debug.Log("Init deltaPos: " + frameDeltaPos);

        vel = moveRB3(vel, Time.fixedDeltaTime, Vector2.down, !jumping);
        //vel = moveRB2(frameDeltaPos, Vector2.down, false);






    }




    Vector2 moveRB3(Vector2 velocity, float frameTime, Vector2 gravityDir, bool snap)
    {
        //the amount to move during this frame
        Vector2 frameDeltaPos = velocity * frameTime;

        //where collision data will be stored after rb.Cast
        //maxes out at max collision
        RaycastHit2D[] hitBufferGrav = new RaycastHit2D[MAX_COLLISIONS];
        RaycastHit2D[] hitBuffer = new RaycastHit2D[MAX_COLLISIONS];

        int groundCheck = grounded && snap? rb.Cast(gravityDir, cf, hitBufferGrav) : rb.Cast(gravityDir, cf, hitBufferGrav, GROUNDED_MIN_DIST);
        
        int collCount = rb.Cast(frameDeltaPos, cf, hitBuffer, frameDeltaPos.magnitude);
       
        //for debugging and drawing lines
        Vector2 bottomOfShape = rb.position - new Vector2(0, shape.size.y / 2f * transform.localScale.y);
        Debug.DrawLine(bottomOfShape, bottomOfShape + frameDeltaPos / Time.deltaTime, Color.white);
        collisionCount = collCount;


        //RaycastHit2D closestGroundColl;

        //if()

        //Logic for sticking to the surface and not flying off slope
        Vector2 snapVect = Vector2.zero;

        if (groundCheck > 0)
        {
            RaycastHit2D closestGroundColl = hitBufferGrav[0];
            for (int i = 1; i < collCount; i++)
            {
                closestGroundColl = closestGroundColl.distance < hitBufferGrav[i].distance ? closestGroundColl : hitBufferGrav[i];
            }

            //will always return acuter angle
            float groundDegIncline = 180 - Vector2.Angle(closestGroundColl.normal, gravityDir);

            if (snap && groundDegIncline <= MIN_GROUND_DEG_ANG)
            {
                //TODO: what about distance from horiz. movement? Need to cast after horiz movement.
                snapVect = gravityDir.normalized * closestGroundColl.distance;
            }
        }




        bool prevGroundState = grounded;
        grounded = groundCheck > 0;


        if (!prevGroundState && grounded) { enteredGround(); }
        else if (prevGroundState && !grounded) { exitedGround(); }

        //The angles of the actual frameDeltaPos vector may be too short
        //or parallel to surface to render any hit detection with ground
        //so in any case this fails, use the floor collisions to calculate shit
        if (collCount == 0)
        {
            collCount = groundCheck;
            hitBuffer = hitBufferGrav;
        }


        //if there were collisions with surface
        if (collCount > 0)
        {
            //get the closest collision
            RaycastHit2D closestColl = hitBuffer[0];
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



        //move the character to the new position
        rb.MovePosition(rb.position + frameDeltaPos + snapVect);

        Debug.DrawLine(bottomOfShape, bottomOfShape + frameDeltaPos / Time.deltaTime, Color.green);

        //return the new real velocity (not including snap velocity)
        //TODO: include snap velocity when returning?
        return frameDeltaPos / Time.deltaTime;
    }

    void enteredGround()
    {

    }

    void exitedGround()
    {

    }


    /*
    Vector2 moveRB2(Vector2 frameDeltaPos, Vector2 gravityDir, bool snap = false)
    {
        //cast in the direction of movement to check for floor. If nothing found, cast in direction of grav.
        var gravCollCount = snap? 0 : rb.Cast(gravityDir, cf, hitBufferGrav, SHELL_RADIUS);

        var moveCollCount = rb.Cast(frameDeltaPos, cf, hitBufferMove, frameDeltaPos.magnitude);
       

        grounded = false;

        //if collided, need to change how it slides against collision
        if (gravCollCount > 0)
        {
            //get shortest length collision
            RaycastHit2D shortestGravHit = hitBufferGrav[0];

            for (int i = 1; i < gravCollCount; i++)
            {
                var hb = hitBufferGrav[i];
                shortestGravHit = hb.distance < shortestGravHit.distance ? hb : shortestGravHit;
            }

            //just for debugging


            //check if its min dist. to be grounded
            if (shortestGravHit.distance <= GROUNDED_MIN_DIST)
            {
                grounded = true;

                //TODO: change to be parallel. to gravityDir
                frameDeltaPos.y = 0;
            }


        }



        if (moveCollCount > 0)
        {
            //get shortest length collision
            RaycastHit2D shortestMoveHit = hitBufferMove[0];

            for (int i = 1; i < moveCollCount; i++)
            {
                var hb = hitBufferMove[i];
                shortestMoveHit = hb.distance < shortestMoveHit.distance ? hb : shortestMoveHit;
            }


            //hbd = shortestHit.distance;
            Debug.Log("hbd: " + hbd);
            Debug.Log("grounded" + grounded);

            //only move along collider parallel if grounded and x direction is not zero
            //TODO: change to be perp. to gravityDir
            if (frameDeltaPos.x != 0)
            {
                Vector2 perpVect = Vector2.Perpendicular(shortestMoveHit.normal);

                perpVect = perpVect * Mathf.Sign(frameDeltaPos.x);

               // Debug.Log("perpVect: " + perpVect);
               // Debug.Log("normal: " + shortestHit.normal);

                float horizDist = Vector2.Dot(new Vector2(frameDeltaPos.x, 0), perpVect);

               // Debug.Log("horizDist: " + horizDist);

                Vector2 newHorzVect = perpVect * horizDist;

                //Debug.Log("newHorzVect: " + newHorzVect);

                frameDeltaPos.x = 0;
                frameDeltaPos += newHorzVect;
            }


        }

        //Debug.Log("final vel: " + vel);
        //Debug.Log("final deltaPos: " + frameDeltaPos);

        //apply move and return altered velocity
        rb.MovePosition(rb.position + frameDeltaPos);

        return frameDeltaPos / Time.deltaTime;


    }

    /*
    void moveRB(Vector2 move, bool yMove)
    {
        float moveDist = move.magnitude;

        if (moveDist > MIN_MOVE_DIST)
        {
            //collisions info stored in hitBuffer variable
            //maxes out using variable array size
            int collCount = 0;
            if (yMove)
            {
                collCount = rb.Cast(move, cf, hitBuffer, SHELL_RADIUS);
            }

            else
            {
                collCount = rb.Cast(move, cf, hitBuffer, moveDist);
            }
            

            hitBufferList.Clear();

            //not using linq shit here b/c accord. Microsoft:
            //"LINQ syntax is typically less efficient than a foreach loop."

            grounded = false;

            for (int i = 0; i < collCount; i++)
            {
                RaycastHit2D hb = hitBuffer[i];
                //store incase needed
                hitBufferList.Add(hb);

                Vector2 currNorm = hb.normal;

                //basically if norm greater than a certain angle,
                //then grounded.
                if (currNorm.y > MIN_GROUND_NORM)
                {
                    //save for next frame to move along ground
                    //perpendicular to this normal
                    if (yMove)
                    {
                        groundNorm = currNorm;

                        Debug.Log(hb.distance);

                        if (hb.distance <= GROUNDED_MIN_DIST)
                        {
                            

                            grounded = true;

                            //so they don't slide b/c it's considered ground
                            currNorm.x = 0;

                            vel.y = 0;
                            
                        }
                    }
                        
                    
                }


                //float projMag = Vector2.Dot(vel, currNorm);

                //if normal against velocity, then alter velocity
                //to simulate a collision
                
                if (projMag < 0)
                {
                    //apply to velocity for next frame
                    vel -= projMag * currNorm;
                }
                

                float newDist = hb.distance;//- SHELL_RADIUS;

                //as we're looping through collision ray casts,
                //find the shortest distance of all the collisions
                //from our collision shapes unto the world,
                //which is what we'll move in the end
                if (newDist < moveDist)
                {
                    hbd = hb.distance;
                    moveDist = newDist;
                }

                //moveDist = newDist < moveDist ? newDist : moveDist;
            }


        }

        if (!yMove)
        {
            savedHorzMove = move.normalized * moveDist;
        }
        else
        {
            rb.MovePosition(rb.position + (move.normalized * moveDist) + savedHorzMove);
        }
        //advance position 
        
    }
    */

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
