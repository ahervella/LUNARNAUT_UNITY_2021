using UnityEngine;

public class TEMP_AstroPastColorChanger : MonoBehaviour
{
    SpriteRenderer spriteComponent;

    private void Awake()
    {
        S_TimeTravel.Current.TimelineChanged -= S_TimeTravel_TimelineChanged;
        S_TimeTravel.Current.TimelineChanged += S_TimeTravel_TimelineChanged;
        spriteComponent = GetComponent<SpriteRenderer>();
        S_TimeTravel_TimelineChanged();
    }

    private void S_TimeTravel_TimelineChanged()
    {
        if (S_TimeTravel.Current.InFuture())
        {
            spriteComponent.color = Color.white;
        }
        else
        {
            spriteComponent.color = Color.green;
        }
    }
}
