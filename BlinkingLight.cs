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

    [SerializeField, GetSet("ManualRefreshApplyingCustomColor")]
    private bool manualRefreshApplyingCustomColor = false;
    public bool ManualRefreshApplyingCustomColor
    {
        get => manualRefreshApplyingCustomColor;
        set
        {
            manualRefreshApplyingCustomColor = false;
            CustomColor = customColor;
        }
    }

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

    private float centerBlinkOn_InnerRadius = default;
    private float centerBlinkOn_OuterRadius = default;
    private float glowBlinkOn_InnerRadius = default;
    private float glowBlinkOn_OuterRadius = default;

    [SerializeField]
    private float centerBlinkOffPercent = 0.5f;
    [SerializeField]
    private float glowBlinkOffPercent = 0.5f;


    [SerializeField]
    private bool useCustomBlinkOffColor = false;

    [SerializeField]
    private Color customBlinkOffColor;

    private void Awake()
    {
        //gameObject.SetActive(startOn);
        centerBlinkOn_InnerRadius = TryGetRadius(lightCenter, innerRadius: true) ?? 1f;
        centerBlinkOn_OuterRadius = TryGetRadius(lightCenter, innerRadius: false) ?? 1f;

        glowBlinkOn_InnerRadius = TryGetRadius(lightGlow, innerRadius: true) ?? 1f;
        glowBlinkOn_OuterRadius = TryGetRadius(lightGlow, innerRadius: false) ?? 1f;

        if (blinkOnTime > 0 && blinkOffTime > 0)
        {
            if (!useCustomBlinkOffColor)
            {
                customBlinkOffColor = customColor;
            }
            StartCoroutine(NextBlinkSwitch(blinkOn: true));
        }
    }

    private IEnumerator NextBlinkSwitch(bool blinkOn)
    {
        if (blinkOn)
        {
            TrySetLight(lightCenter, customColor, centerBlinkOn_InnerRadius, centerBlinkOn_OuterRadius);
            TrySetLight(lightGlow, customColor, glowBlinkOn_InnerRadius, glowBlinkOn_OuterRadius);
        }
        else
        {
            TrySetLight(lightCenter, customColor, centerBlinkOn_InnerRadius * centerBlinkOffPercent, centerBlinkOn_OuterRadius * centerBlinkOffPercent);
            TrySetLight(lightGlow, customColor, glowBlinkOn_InnerRadius * glowBlinkOffPercent, glowBlinkOn_OuterRadius * glowBlinkOffPercent);
        }

        yield return new WaitForSeconds(blinkOn ? blinkOnTime : blinkOffTime);
        StartCoroutine(NextBlinkSwitch(!blinkOn));
    }

    private void TrySetLight(Light2D light, Color color, float innerRadius = -1, float outerRadius = -1)
    {
        if (light != null)
        {
            light.color = color;
            light.pointLightInnerRadius = innerRadius >= 0 ? innerRadius : light.pointLightInnerRadius;
            light.pointLightOuterRadius = outerRadius >= 0 ? outerRadius : light.pointLightOuterRadius;
        }
    }

    private float? TryGetRadius(Light2D light, bool innerRadius)
    {
        if (light != null)
        {
            return innerRadius? light.pointLightInnerRadius : light.pointLightOuterRadius;
        }
        return null;
    }
}
