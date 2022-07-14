using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

//Might wanna try one of these solutions for adding scenes via the inspector (and not having to add them to the player:
//https://answers.unity.com/questions/242794/inspector-field-for-scene-asset.html

[CreateAssetMenu(fileName = "S_Menus", menuName = "ScriptableObjects/Singletons/Menus")]
public class S_Menus : Singleton<S_Menus>
{
    public enum GAME_SCENE { MAIN_MENU, DEMO_2021, PAUSE_MENU }

    public event System.Action<bool> PauseToggled = delegate { };
    public event System.Action ResetLevel = delegate { };
    public event System.Action ReturnToMainMenu = delegate { };
    private bool pauseToggle = false;

    [SerializeField]
    private List<GameSceneWrapper> sceneDict = new List<GameSceneWrapper>();

    [Serializable]
    private class GameSceneWrapper
    {
        [SerializeField]
        private GAME_SCENE gameScene;
        public GAME_SCENE GameScene => gameScene;
        [SerializeField]
        private string sceneName;
        public string SceneName => sceneName;
        [SerializeField]
        private LoadSceneMode sceneType = LoadSceneMode.Single;
        public LoadSceneMode SceneType => sceneType;
    }

    private string GetSceneName(GAME_SCENE gameScene)
    {
        foreach(GameSceneWrapper gsw in sceneDict)
        {
            if (gsw.GameScene == gameScene)
            {
                return gsw.SceneName;
            }
        }

        Debug.LogError("Couldn't find scene name for scene: " + gameScene);
        return string.Empty;
    }

    private string pauseSceneName => GetSceneName(GAME_SCENE.PAUSE_MENU);

    //add pause menu navigation (make it's own class after base classing a menu script)
    //add main menu in conjunction with developer tools
    //also add main menu script navigation

    protected override void OnAwake()
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string sc = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex( i ));
            if (sc.Contains(pauseSceneName))
            {
                return;
            }
        }

        Debug.LogErrorFormat("The pause menu scene '{0}' you tried to use with the Menu Singleton is not in the build settings list!", pauseSceneName);
    }

    public void TogglePauseMenu()
    {
        if (!pauseToggle)
        {
            Time.timeScale = 0;
            PauseToggled(true);
            SceneManager.LoadSceneAsync(pauseSceneName, LoadSceneMode.Additive);
        }
        else
        {

            Time.timeScale = 1;
            PauseToggled(false);
            SceneManager.UnloadSceneAsync(pauseSceneName);
        }

        pauseToggle = !pauseToggle;
    }

    public void LoadGameScene(GAME_SCENE gs)
    {
        foreach(GameSceneWrapper gsw in sceneDict)
        {
            if (gsw.GameScene == gs)
            {
                StartCoroutine(LoadGameSceneAsycn(GetSceneName(gs), gsw.SceneType));
                return;
            }
        }

        Debug.LogErrorFormat("The game scene {0}, was not found, did you forget to add it in the menu singleton inspector?", gs.ToString());
    }

    private IEnumerator LoadGameSceneAsycn(string sceneName, LoadSceneMode sceneType)
    {
        /*
        AsyncOperation unloadLevelOp = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
        while (!unloadLevelOp.isDone)
        {
            //0.9 because last 10 percent is cleaning up scenes with new and old stuff ("activation phase")
            float progress = Mathf.Clamp01(unloadLevelOp.progress / 0.9f);
            Debug.LogFormat("Level unloading progress: {0}%", progress * 100f);
            yield return null;
        }*/

        if (sceneType == LoadSceneMode.Single)
        {
            ClearDelegateSubscribers();
        }

        Time.timeScale = 0;

        AsyncOperation levelLoadingOp = SceneManager.LoadSceneAsync(sceneName, sceneType);

        while (!levelLoadingOp.isDone)
        {
            //0.9 because last 10 percent is cleaning up scenes with new and old stuff ("activation phase")
            float progress = Mathf.Clamp01(levelLoadingOp.progress / 0.9f);
            Debug.LogFormat("Level loading progress: {0}%", progress * 100f);
            yield return null;
        }
        Time.timeScale = 1;
    }

    private void ClearDelegateSubscribers()
    {
        PauseToggled = delegate { };
        ResetLevel = delegate { };
        ReturnToMainMenu = delegate { };
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }
}
