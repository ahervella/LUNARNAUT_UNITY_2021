using UnityEngine;
using System.Collections;

/// <summary>
/// Will be used throughout the game to provide a bool argument to interactive objects
/// </summary>
public abstract class SO_BoolArgument : ScriptableObject
{
    public abstract bool IsTrue();
}
