using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pausable : MonoBehaviour
{
    protected void Awake()
    {
        S_Menus_Delegates();
        OnAwake();
    }

    protected abstract void OnAwake();


    private void S_Menus_Delegates()
    {
        S_Menus.Current.PauseToggled -= S_MENUS_PauseToggled;
        S_Menus.Current.PauseToggled += S_MENUS_PauseToggled;
    }

    protected bool S_MENUS_gamePaused = false;
    private void S_MENUS_PauseToggled(bool paused)
    {
        S_MENUS_gamePaused = paused;
    }
}
