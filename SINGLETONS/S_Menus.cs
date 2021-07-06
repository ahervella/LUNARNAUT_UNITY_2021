using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Menus : Singleton<S_Menus>
{
    //make some pause delegates
    //reset delegates
    //make a pause scene
    public event System.Action Paused = delegate { };
    public event System.Action Unpaused = delegate { };
    public event System.Action ResetLevel = delegate { };
    public event System.Action ReturnToMainMenu = delegate { };
}
