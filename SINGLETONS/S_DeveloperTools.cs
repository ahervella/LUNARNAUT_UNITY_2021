using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[CreateAssetMenu(fileName = "S_DeveloperTools", menuName = "ScriptableObjects/Singletons/DeveloperTools")]
public class S_DeveloperTools : Singleton<S_DeveloperTools>
{
    [SerializeField, GetSet("EnableDevTools")]
    private bool enableDevTools = false;
    public event System.Action EnableDevToolsChanged = delegate { };
    public bool EnableDevTools
    {
        get => enableDevTools;
        set
        {
            enableDevTools = value;
            EnableDevToolsChanged();
        }
    }

    #region TIME_TRAVEL
    public bool DevToolsEnabled_TIME_TRAVEL()
    {
        return EnableDevTools && TimeTravelDevTools;
    }

    [Header("TIME_TRAVEL")]
    [SerializeField, GetSet("TimeTravelDevTools")]
    private bool timeTravelDevTools = false;
    public event System.Action TimeTravelDevToolsChanged = delegate { };
    public bool TimeTravelDevTools
    {
        get => timeTravelDevTools;
        set
        {
            timeTravelDevTools = value;
            TimeTravelDevToolsChanged();
        }
    }

    [SerializeField, GetSet("TogglePlayerEnableTimeTravel")]
    private bool togglePlayerEnableTimeTravel = false;
    public event System.Action TogglePlayerEnableTimeTravelChanged = delegate { };
    public bool TogglePlayerEnableTimeTravel
    {
        get => togglePlayerEnableTimeTravel;
        set
        {
            togglePlayerEnableTimeTravel = value;
            TogglePlayerEnableTimeTravelChanged();
        }
    }

    [SerializeField, GetSet("SetTTSpawnsAtCurrPos")]
    private bool setTTSpawnsAtCurrPos = false;
    public event System.Action SetTTSpawnsAtCurrPosChanged = delegate { };
    public bool SetTTSpawnsAtCurrPos
    {
        get => setTTSpawnsAtCurrPos;
        set
        {
            setTTSpawnsAtCurrPos = value;
            SetTTSpawnsAtCurrPosChanged();
        }
    }

    [SerializeField, GetSet("EnableInspectorLevelVariables")]
    private bool enableInspectorLevelVariables = false;
    public event System.Action EnableInspectorLevelVariablesChanged = delegate { };
    public bool EnableInspectorLevelVariables
    {
        get => enableInspectorLevelVariables;
        set
        {
            enableInspectorLevelVariables = value;
            EnableInspectorLevelVariablesChanged();
        }
    }

    #endregion


    #region ASTRO_PLAYER
    public bool DevToolsEnabled_ASTRO_PLAYER()
    {
        return EnableDevTools && AstroPlayerDevTools;
    }

    [Header("ASTRO_PLAYER")]

    [SerializeField, GetSet("AstroPlayerDevTools")]
    private bool astroPlayerDevTools = false;
    public event System.Action AstroPlayerDevToolsChanged = delegate { };
    public bool AstroPlayerDevTools
    {
        get => astroPlayerDevTools;
        set
        {
            astroPlayerDevTools = value;
            AstroPlayerDevToolsChanged();
        }
    }

    public event System.Action CurrHealthChanged = delegate { };
    [SerializeField, GetSet("CurrHealth")]
    [Range(0, AstroPlayer.MAX_HEALTH)]
    private int currHealth = 4;
    public int CurrHealth
    {
        get => currHealth;
        set
        {
            currHealth = value;
            CurrHealthChanged();
        }
    }


    [SerializeField]
    private bool enableInvincibility;
    //TODO: impelent invincibility once astro can get hurt

    public event System.Action KillAstro = delegate { };
    [SerializeField]
    private bool enableKillKey = true;
    [SerializeField]
    private Key killKey = Key.Backslash;
    private void Update()
    {
        if (!enableKillKey)
        {
            return;
        }

        foreach (KeyControl kc in Keyboard.current.allKeys)
        {
            if (kc.keyCode == killKey)
            {
                KillAstro();
                break;
            }
        }
    }

    public event System.Action GravityMultiplyerChanged = delegate { };
    [SerializeField, GetSet("GravityMultiplyer")]
    private float gravityMultiplyer = 1f;
    public float GravityMultiplyer
    {
        get => gravityMultiplyer;
        set
        {
            gravityMultiplyer = value;
            GravityMultiplyerChanged();
        }
    }

    public event System.Action EnableUnlimtedJumpChanged = delegate { };
    [SerializeField, GetSet("EnableUnlimtedJump")]
    private bool enableUnlimtedJump;
    public bool EnableUnlimtedJump
    {
        get => enableUnlimtedJump;
        set
        {
            enableUnlimtedJump = value;
            EnableUnlimtedJumpChanged();
        }
    }

    public event System.Action PrintAstroAnimsChanged = delegate { };
    [SerializeField, GetSet("PrintAstroAnims")]
    private bool printAstroAnims;
    public bool PrintAstroAnims
    {
        get => printAstroAnims;
        set
        {
            printAstroAnims = value;
            PrintAstroAnimsChanged();
        }
    }

    public event System.Action ShowPrintAstroVelLinesChanged = delegate { };
    [SerializeField, GetSet("ShowPrintAstroVelLines")]
    private bool showPrintAstroVelLines;
    public bool ShowPrintAstroVelLines
    {
        get => showPrintAstroVelLines;
        set
        {
            showPrintAstroVelLines = value;
            ShowPrintAstroVelLinesChanged();
        }
    }

    public event System.Action PrintRawPlayerInputsChanged = delegate { };
    [SerializeField, GetSet("PrintRawPlayerInputs")]
    private bool printRawPlayerInputs;
    public bool PrintRawPlayerInputs
    {
        get => printRawPlayerInputs;
        set
        {
            printRawPlayerInputs = value;
            PrintRawPlayerInputsChanged();
        }
    }
    #endregion

    #region MOVING_PLATFORMS
    public bool DevToolsEnabled_MOVING_PLATFORMS()
    {
        return EnableDevTools && DevToolsMovingPlatforms;
    }

    public event System.Action DevToolsMovingPlatformsChanged = delegate { };
    [Header("MOVING_PLATFORM")]
    [SerializeField, GetSet("DevToolsMovingPlatforms")]
    private bool devToolsMovingPlatforms;
    public bool DevToolsMovingPlatforms
    {
        get => devToolsMovingPlatforms;
        set
        {
            devToolsMovingPlatforms = value;
            DevToolsMovingPlatformsChanged();
        }
    }
    #endregion
}
