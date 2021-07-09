using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPause : MonoBehaviour
{
    public void ResumeGame()
    {
        S_Menus.Current.TogglePauseMenu();
    }

    public void RestartCurrentLevel()
    {
        S_Menus.Current.ReloadCurrentScene();
    }

    public void QuitToMainMenu()
    {
        S_Menus.Current.LoadGameScene(S_Menus.GAME_SCENE.MAIN_MENU);
    }
}
