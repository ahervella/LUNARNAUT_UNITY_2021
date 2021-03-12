using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimatedTextContainer// : MonoBehaviour
{
    [SerializeField]
    private SO_AnimatedText at;
    public SO_AnimatedText AT => at;
    [SerializeField]
    private Transform parent;


    /*
    public void Awake()
    {
        at.CurrParent = parent;
    }*/
}
