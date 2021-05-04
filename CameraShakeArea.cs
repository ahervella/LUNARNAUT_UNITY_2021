using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraShakeArea : MonoBehaviour
{
    private const string ASTRO_TAG = "ASTRO";

    [SerializeField]
    private AstroCamera.SHAKE shakeType = AstroCamera.SHAKE.MED;
    [SerializeField]
    private float duration = -1;
    [SerializeField]
    private float easeInTime = 0.5f;
    [SerializeField]
    private float easeOutTIme = 0.5f;
    [SerializeField]
    private bool oneTimeShake = false;
    private bool oneTimeShakeCache = false;

    public static event Action<CameraShakeArea, AstroCamera.SHAKE, float, float, float> EnteredShakeArea;
    public static event Action<CameraShakeArea, AstroCamera.SHAKE, float, float, float> ExitedShakeArea;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (oneTimeShakeCache)
        {
            return;
        }

        if (collision.CompareTag(ASTRO_TAG))
        {
            EnteredShakeArea(this, shakeType, duration, easeInTime, easeOutTIme);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (oneTimeShakeCache)
        {
            return;
        }

        if (collision.CompareTag(ASTRO_TAG))
        {
            if (oneTimeShake)
            {
                oneTimeShakeCache = true;
            }
            ExitedShakeArea(this, shakeType, duration, easeInTime, easeOutTIme);
        }
    }
}
