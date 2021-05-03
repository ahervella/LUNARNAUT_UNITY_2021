using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ZoomArea : MonoBehaviour
{
    private const string ASTRO_TAG = "ASTRO";

    [SerializeField]
    private AstroCamera.ZOOM zoomType = AstroCamera.ZOOM.NORM;

    public static event Action<ZoomArea, AstroCamera.ZOOM> EnteredZoomArea;
    public static event Action<ZoomArea, AstroCamera.ZOOM> ExitedZoomArea;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(ASTRO_TAG))
        {
            EnteredZoomArea(this, zoomType);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(ASTRO_TAG))
        {
            ExitedZoomArea(this, zoomType);
        }
    }
}
