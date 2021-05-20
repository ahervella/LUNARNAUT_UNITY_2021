using System;
using UnityEngine;

[Serializable]
public class AnimatedTextContainer// : MonoBehaviour
{
    [SerializeField]
    private SO_AnimatedTextTemplate att;
    public SO_AnimatedTextTemplate ATT => att;
    [SerializeField]
    private Transform customParent;
    public Transform CustomParent => customParent;
}
