using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LunarnautShit;


public class astroPlayer : MonoBehaviour
{


    Rigidbody2D rb;

    Animator anim;

    public LunInput input;
    public int blah;

    Vector2 direction;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        input = new LunInput();

        //TODO: try setting up with direction var
        input.Enable();
        input.AstroPlayer.move.performed += val => testMethod(val.ReadValue<Vector2>());//direction = val.ReadValue<Vector2>();
        input.AstroPlayer.jump.performed += nada => testButton();

    }

    void testButton()
    {
        Debug.Log("sup");
    }

    void testMethod(Vector2 blah)
    {
        Debug.Log(blah);
    }

    // Update is called once per frame
    void Update()
    {

        //Debug.Log(direction);
        
    }

    void processHazards()
    {

    }

    void processMoveInput()
    {
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
