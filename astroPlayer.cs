using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LunarnautShit;

//Tutorial I used to implement this awesome shit:
//https://learn.unity.com/tutorial/live-session-2d-platformer-character-controller#5c7f8528edbc2a002053b68e


public class astroPlayer : MonoBehaviour
{
    const float MIN_MOVE_DIST = 0.001f;
    const float SHELL_RADIUS = 0.5f;//0.01f;
    const float MIN_GROUND_DEG_ANG = 60f;
    float MIN_GROUND_NORM = Mathf.Sin(Mathf.Deg2Rad * MIN_GROUND_DEG_ANG);

    const float SPEED = 10f;
    const float MAX_SPEED = 10f;
    const float GRAVITY = 3f;
    const float TERMINAL_VEL = 5f;

    Rigidbody2D rb;
    Animator anim;

    public LunInput input;

    public Vector2 direction = new Vector2(0, 0);
    public Vector2 vel = new Vector2(0, 0);
    public Vector2 groundNorm = new Vector2(0, 0);
    public bool grounded = false;

    ContactFilter2D cf;

    //initialized with max number of collisions
    //able to store per frame
    const int MAX_COLLISIONS = 16;
    RaycastHit2D[] hitBuffer = new RaycastHit2D[MAX_COLLISIONS];
    List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(MAX_COLLISIONS);

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
        //direction = input.AstroPlayer.move.ReadValue<Vector2>();

        //NOTE! : Time.deltaTime == Time.fixedDeltaTime when in FixedUpdate
        //(I just tested this)

        direction = input.AstroPlayer.move.ReadValue<Vector2>();

        //get new x and y velocity
        float xVel = direction.x * SPEED;//Mathf.Lerp(vel.x, direction.x * SPEED, Time.fixedDeltaTime);

        //makes sense to multiply by deltaTime here and later
        //because grav is acceleration
        float yVel = vel.y - GRAVITY * Time.fixedDeltaTime;

        //clamp limits
        xVel = Mathf.Clamp(xVel, -MAX_SPEED, MAX_SPEED);
        yVel = Mathf.Clamp(yVel, -TERMINAL_VEL, TERMINAL_VEL);

        vel = new Vector2(xVel, yVel);

        


        Vector2 deltaPos = vel * Time.fixedDeltaTime;

        Vector2 movePerp2Ground = new Vector2(groundNorm.y, -groundNorm.x);

        Vector2 move = movePerp2Ground * deltaPos.x;

        

        //first move in parallel to ground (horizontal)
        moveRB(move, false);

        move = Vector2.up * deltaPos.y;

        //then apply gravity
        moveRB(move, true);
       


    }

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
                    grounded = true;
                    if (yMove)
                    {
                        //save for next frame to move along ground
                        //perpendicular to this normal
                        groundNorm = currNorm;

                        //so they don't slide b/c it's considered ground
                        currNorm.x = 0;
                    }
                    vel.y = 0;
                }


                //float projMag = Vector2.Dot(vel, currNorm);

                //if normal against velocity, then alter velocity
                //to simulate a collision
                /*
                if (projMag < 0)
                {
                    //apply to velocity for next frame
                    vel -= projMag * currNorm;
                }
                */

                float newDist = hb.distance;//- SHELL_RADIUS;

                //as we're looping through collision ray casts,
                //find the shortest distance of all the collisions
                //from our collision shapes unto the world,
                //which is what we'll move in the end
                moveDist = newDist < moveDist ? newDist : moveDist;
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
