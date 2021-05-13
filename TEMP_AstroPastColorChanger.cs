using UnityEngine;

public class TEMP_AstroPastColorChanger : MonoBehaviour
{
    [SerializeField, GetSet("HueShift")]
    private float hueShift;
    public float HueShift
    {
        get => hueShift;
        set
        {
            hueShift = value;
            SetShaderHueShifter(value);
        }
    }

    //SpriteRenderer spriteComponent;
    [SerializeField]
    private Material shaderMat;

    private void Awake()
    {
        S_TimeTravel.Current.TimelineChanged -= S_TimeTravel_TimelineChanged;
        S_TimeTravel.Current.TimelineChanged += S_TimeTravel_TimelineChanged;
        //spriteComponent = GetComponent<SpriteRenderer>();
        shaderMat = GetComponent<Material>();
        S_TimeTravel_TimelineChanged();
    }

    private void S_TimeTravel_TimelineChanged()
    {
        if (S_TimeTravel.Current.InFuture())
        {
            SetShaderHueShifter(0);
            //spriteComponent.color = Color.white;
        }
        else
        {
            SetShaderHueShifter(hueShift);
            //spriteComponent.color = tintColor;
        }
    }

    private void SetShaderHueShifter(float value)
    {
        if (shaderMat == null)
        {
            shaderMat = GetComponent<Material>();
        }
        shaderMat.SetFloat("_HueShifter", value);
    }
}
