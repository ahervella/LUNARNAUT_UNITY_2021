using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstroDev : MonoBehaviour
{
    [SerializeField]
    private bool EnableDevTools = false;

    [Header("Astro Player")]
    [SerializeField]
    [Range(0, AstroPlayer.MAX_HEALTH)]
    private int startingHealth;
    [SerializeField]
    private bool enableInvincibility;
    [SerializeField]
    private bool enableKillKey = true;
    [SerializeField]
    private KeyCode killKey = KeyCode.Backslash;
    [SerializeField]
    private float gravityMultiplyer;
    [SerializeField]
    private bool enableUnlimtedJump;


    private AstroPlayer astroPlayer;
    private AstroAnim astroAnim;


}
