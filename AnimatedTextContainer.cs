using System;
using UnityEngine;

[Serializable]
public class AnimatedTextContainer// : MonoBehaviour
{
    [SerializeField]
    private SO_AnimatedText at;
    public SO_AnimatedText AT => at;
    [SerializeField]
    private Transform parent;

    //TODO: find a better system around this so that we don't have to worry about having the same parent at each instance...
    //TODO: cannot make an SO_AnimatedText an object. Maybe use create instance?
    public void OnAwake()
    {
        if (at == null)
        {
            return;
        }

        if (parent == null)
        {
            Debug.LogWarning("No initial parent for AnimatedTextContainer with AT" + at.name);
            return;
        }
        at.CurrParent = parent;
    }
}
