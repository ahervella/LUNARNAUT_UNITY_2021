using System;
using UnityEngine;

[Serializable]
public class AnimatedTextContainer
{
    [SerializeField]
    private SO_AnimatedTextTemplate useTemplate;
    public SO_AnimatedTextTemplate UseTemplate => useTemplate;
    [SerializeField]
    private AnimatedText.ATDetails details;
    public AnimatedText.ATDetails Details => details;
    [SerializeField]
    private Transform customParent;
    public Transform CustomParent => customParent;
}
