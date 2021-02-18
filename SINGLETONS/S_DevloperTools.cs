using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_DevloperTools : Singleton<S_DevloperTools>
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
    public bool EnableInvincibility => enableInvincibility;

    public event System.Action KillAstro = delegate { };
    [SerializeField]
    private bool enableKillKey = true;
    [SerializeField]
    private KeyCode killKey = KeyCode.Backslash;
    private void Update()
    {
        if (!enableKillKey)
        {
            return;
        }

        if (Input.GetKeyDown(killKey))
        {
            KillAstro();
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
    #endregion

}
