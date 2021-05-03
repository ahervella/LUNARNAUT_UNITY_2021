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

    public static event Action<CameraShakeArea, AstroCamera.SHAKE, float> EnteredShakeArea;
    public static event Action<CameraShakeArea, AstroCamera.SHAKE, float> ExitedShakeArea;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(ASTRO_TAG))
        {
            EnteredShakeArea(this, shakeType, duration);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(ASTRO_TAG))
        {
            ExitedShakeArea(this, shakeType, duration);
        }
    }
}
