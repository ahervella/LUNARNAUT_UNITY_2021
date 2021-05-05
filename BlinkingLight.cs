using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class BlinkingLight : MonoBehaviour
{
    /*private enum LIGHT_COLOR { RED, YELLOW, BLUE, GREEN }
    private Dictionary<LIGHT_COLOR, Color> lightColorDict = new Dictionary<LIGHT_COLOR, Color>
    {
        { LIGHT_COLOR.RED, Color.red },
        { LIGHT_COLOR.YELLOW, Color.yellow },
        { LIGHT_COLOR.BLUE, Color.blue },
        { LIGHT_COLOR.GREEN, Color.green }
    };*/

    [SerializeField]
    private Light2D lightCenter;

    [SerializeField]
    private Light2D lightGlow;

    [SerializeField]
    private float blinkOnTime;

    [SerializeField]
    private float blinkOffTime;

    //[SerializeField]
    //private bool startOn = true;

    [SerializeField, GetSet("CustomColor")]
    private Color customColor;
    public Color CustomColor
    {
        get => customColor;
        set
        {
            customColor = value;
            TrySetLight(lightCenter, value);
            TrySetLight(lightGlow, value);
        }
    }

    [SerializeField]
    private Color customBlinkOffColor;

    private void Awake()
    {
        //gameObject.SetActive(startOn);

        if (blinkOnTime > 0 && blinkOffTime > 0)
        {
            StartCoroutine(NextBlinkSwitch(blinkOn: true));
        }
    }

    private IEnumerator NextBlinkSwitch(bool blinkOn)
    {
        TrySetLight(lightCenter, blinkOn ? customColor : customBlinkOffColor);
        TrySetLight(lightGlow, blinkOn ? customColor : customBlinkOffColor);
        //TryEnableLight(lightCenter, blinkOn);
        //TryEnableLight(lightGlow, blinkOn);
        yield return new WaitForSeconds(blinkOn ? blinkOnTime : blinkOffTime);
        StartCoroutine(NextBlinkSwitch(!blinkOn));
    }

    private void TrySetLight(Light2D light, Color color)
    {
        if (light != null)
        {
            light.color = color;
        }
    }

    /*
    private void TryEnableLight(Light2D light, bool enable)
    {
        if (light != null)
        {
            light.enabled = enable;
        }
    }*/
}
