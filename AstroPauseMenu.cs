using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AstroPauseMenu : MonoBehaviour
{
    public void OnPausedUpdate(InputAction.CallbackContext val)
    {
        if (!val.performed)
        {
            return;
        }

        S_Menus.Current.TogglePauseMenu();
    }
}
